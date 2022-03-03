using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ReaLTaiizor.Child.Crown;
using ReaLTaiizor.Forms;
using ReserveBlockWinWallet.Models;

namespace ReserveBlockWinWallet
{
    public partial class WalletForm : LostForm
    {
        public static Process proc = new Process();
        public static string nodeURL;
        public static bool IsWalletSyncing = true;
        public static bool ShowWalletSyncMessage = true;
        public  WalletForm()
        {
            InitializeComponent();
            txSendAddressDropDown.SelectedItemChanged += delegate { txSendAddressDropDown_SelectedIndexChanged(txSendAddressDropDown.SelectedItem != null ? txSendAddressDropDown.SelectedItem.Text : "empty"); };
            recAddressDropDownList.SelectedItemChanged += delegate { recAddressDropDownList_SelectedIndexChanged(recAddressDropDownList.SelectedItem != null ? recAddressDropDownList.SelectedItem.Text : "empty"); };
            valiDropDownList.SelectedItemChanged += delegate { valiAddressDropDown_SelectedIndexChanged(valiDropDownList.SelectedItem != null ? valiDropDownList.SelectedItem.Text : "empty"); };
            this.FormClosing += WalletForm_FormClosing;
            walletInfo.AppendText("RBX Wallet Started on " + DateTime.Now.ToString());
            walletInfo.AppendText(Environment.NewLine);
            walletInfo.AppendText("Connecting to RBX Network.");
            //Perform connect to peers check
            walletInfo.AppendText(Environment.NewLine);
            walletInfo.AppendText("Connected. Looking for new blocks.");
            //Look for new blocks
            walletStartTimeDate.Text = DateTime.Now.ToString();

            System.Threading.Timer walletRefresh = new System.Threading.Timer(walletRefresh_Elapsed);
            walletRefresh.Change(10000, 20000);


            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = Directory.GetCurrentDirectory() + @"\RBXCore\ReserveBlockCore.exe";
                start.WindowStyle = ProcessWindowStyle.Hidden; //Hides GUI
                start.CreateNoWindow = true; //Hides console
                start.Arguments = "enableapi";

                proc.StartInfo = start;
                proc.Start();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the RBX Core CLI. Please read the readme.txt in RBXCore folder");
            }


            nodeURL = "http://localhost:8080";
            //nodeURL = "https://localhost:7777";// testurl - not for production

            GetWalletOnline();

            SetWalletInfo();

            GetWalletAddresses();

            GetWalletValidatorAddresses();

            DashPrintWalletTransactions();

        }
        delegate void SetTextCallback(Form form, Control ctrl, string text);

        public static void SetText(Form form, Control ctrl, string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                
                form.Invoke(d, new object[] { form, ctrl, text });
            }
            else
            {
                ctrl.Text = text;
            }
        }

        public static void SetTextAppend(Form form, Control ctrl, string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetTextAppend);

                form.Invoke(d, new object[] { form, ctrl, text });
            }
            else
            {
                ctrl.Text += text;
            }
        }

        private async void walletRefresh_Elapsed(object sender)
        {
            await SetWalletInfo();
            await GetWalletValidatorAddresses();
            await DashPrintWalletTransactions();
            await UpdateBalance();
        }

        #region Wallet Get Walelt Online
        public async Task GetWalletOnline()
        {
            string onlineStatus = "Offline";
            dashboardMainBox.AppendText("Hello! Welcome to RBX Wallet.");
            while (onlineStatus == "Offline")
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string endpoint = nodeURL + "/api/V1/CheckStatus";
                        using (var Response = await client.GetAsync(endpoint))
                        {
                            if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string data = await Response.Content.ReadAsStringAsync();
                                

                                if(data == "Online")
                                {
                                    onlineStatus = "Online";
                                }
                                dashboardMainBox.AppendText(Environment.NewLine);
                                dashboardMainBox.AppendText("RBX Wallet has conneted to local node.");
                                dashboardMainBox.AppendText(Environment.NewLine);
                                dashboardMainBox.AppendText("Your RBX wallet needs to sync. Please wait for sync before sending any transactions or validating.");

                            }
                            else
                            {

                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

        }

        #endregion

        #region Sets the Wallet Info
        public async Task SetWalletInfo()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetWalletInfo";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();
                            var walInfo = data.Split(':');

                            var blockHeight = walInfo[0];
                            var peerCount = walInfo[1];
                            var walletSync = walInfo[2].ToLower();

                            if(walletSync == "true")
                            {
                                IsWalletSyncing = true;
                            }
                            else
                            {
                                IsWalletSyncing = false;
                                if(ShowWalletSyncMessage == true)
                                {
                                    ShowWalletSyncMessage = false;
                                    var walletSyncedText = Environment.NewLine + "Your wallet is now synced. Thank you for waiting.";
                                    SetTextAppend(this, dashboardMainBox, walletSyncedText);
                                }
                            }

                            SetText(this, peerCountLabel, peerCount + " / 6");
                            SetText(this, blockHeightLabel, blockHeight);
                            //peerCountLabel.Text = peerCount + " / 6";
                            //blockHeightLabel.Text = blockHeight;

                            dashboardMainBox.AppendText(Environment.NewLine);
                            dashboardMainBox.AppendText("Current Block Height is: " + blockHeight);
                            dashboardMainBox.AppendText(Environment.NewLine);
                            dashboardMainBox.AppendText("You are connected to -> " + peerCount + " <- peers.");
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Update balance
        public async Task UpdateBalance()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetAllAddresses";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if (data != "No Accounts")
                            {
                                var accounts = JsonConvert.DeserializeObject<List<Account>>(data);

                                var bal = accounts.Sum(x => x.Balance).ToString("0.00000000");
                                SetText(this, dashMainBalLabel, bal + " RBX");

                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Get the Wallet Addresses
        public async Task GetWalletAddresses()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetAllAddresses";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if(data != "No Accounts")
                            {
                                var accounts = JsonConvert.DeserializeObject<List<Account>>(data);
                                accounts.ForEach(x =>
                                {
                                    var addr = new CrownDropDownItem(x.Address);
                                    //SetText(this, blockHeightLabel, blockHeight);
                                    
                                    txSendAddressDropDown.Items.Add(addr);
                                    recAddressDropDownList.Items.Add(addr);
                                });

                                var bal = accounts.Sum(x => x.Balance).ToString("0.00000000");
                                SetText(this, dashMainBalLabel, bal + " RBX");

                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
        #endregion

        #region Get the Wallet Validator
        public async Task GetWalletValidatorAddresses()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetValidatorAddresses";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if (data != "No Accounts")
                            {
                                var accounts = JsonConvert.DeserializeObject<List<Account>>(data);
                                accounts.ForEach(x =>
                                {
                                    valiDropDownList.Items.Clear();
                                    var addr = new CrownDropDownItem(x.Address);
                                    valiDropDownList.Items.Add(addr);
                                });

                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Get Validator Info
        public async Task<string> GetValidatorInfo(string address)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetValidatorInfo/" + address;
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            return data;

                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return "Failed to get name";
        }
        #endregion

        #region Get the Wallet Info
        public async Task<Account?> GetWalletInfo(string addr)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetAddressInfo/" + addr;
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if (data != "No Accounts")
                            {
                                var account = JsonConvert.DeserializeObject<Account>(data);

                                return account;
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        #endregion

        #region Get New Address
        public async Task<string> GetNewAddress()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetNewAddress";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if(data != "Fail")
                            {
                                return data;
                            }
                            
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return "Fail";
        }
        #endregion

        #region Get the Wallet Addresses
        public async Task DashPrintWalletAddresses()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetAllAddresses";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if (data != "No Accounts")
                            {
                                dashboardMainBox.Clear();
                                var accounts = JsonConvert.DeserializeObject<List<Account>>(data);
                                accounts.ForEach(x =>
                                {
                                    dashboardMainBox.AppendText("Address : " + x.Address + " - Balance: " + x.Balance.ToString());
                                    dashboardMainBox.AppendText(Environment.NewLine);
                                });

                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Get the Wallet Transactions
        public async Task DashPrintWalletTransactions()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/GetAllTransactions";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if (data != "No Accounts")
                            {
                                //dashboardMainBox.Clear();
                                SetText(this, dashRecentTxBox, " ");

                                var transactions = JsonConvert.DeserializeObject<List<Transaction>>(data);
                                if(transactions != null)
                                {
                                    transactions.OrderByDescending(x => x.Height).Take(20).ToList().ForEach(x => {
                                        var row = "From: " + x.FromAddress + Environment.NewLine
                                        + "To: " + x.ToAddress + Environment.NewLine
                                        + "Amount: " + x.Amount.ToString("0.00") + Environment.NewLine
                                        + "Height: " + x.Height.ToString() + Environment.NewLine + Environment.NewLine;
                                        
                                        SetTextAppend(this, dashRecentTxBox, row);
                                    });

                                    
                                    transactionsGrid.DataSource = transactions.OrderByDescending(x => x.Height).ToList();
                                }
                                

                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Import Private Key
        public async Task<string> ImportPrivateKey(string privKey)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/ImportPrivateKey/" + privKey;
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            if (data != "NAC")
                            {
                                return data;
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return "FAIL";
        }
        #endregion

        #region Send TX Out
        public async Task<string> SendTxOut(string fromAddr, string toAddr, string amount)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/SendTransaction/" + fromAddr + "/" + toAddr + "/" + amount;
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            return data;
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return "FAIL";
        }

        #endregion

        #region Start Validating
        public async Task<string> StartValidating(string addr, string uname)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/StartValidating/" + addr + "/" + uname;
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();

                            return data;
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return "FAIL";
        }

        #endregion

        #region Exit
        public async Task Exit()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string endpoint = nodeURL + "/api/V1/SendExit";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Drop down on change events

        private async void txSendAddressDropDown_SelectedIndexChanged(string item)
        {
            if (item != "empty")
            {
                var account = await GetWalletInfo(item);

                if (account != null)
                {
                    SetText(this, txSendBalanceLabel, account.Balance.ToString() + " RBX");
                }
                else
                {
                    SetText(this, txSendBalanceLabel, "0.00" + " RBX");
                }
            }
        }

        private async void recAddressDropDownList_SelectedIndexChanged(string item)
        {
            if(item != "empty")
            {
                var account = await GetWalletInfo(item);

                if (account != null)
                {
                    SetText(this, recAddressLabel, account.Address);
                    SetText(this, recBalance, account.Balance.ToString() + " RBX");
                    SetText(this, recValiFlag, account.IsValidating == true ? "Yes" : "No");
                }
                else
                {

                }

            }
        }

        private async void valiAddressDropDown_SelectedIndexChanged(string item)
        {
            if (item != "empty")
            {
                var account = await GetWalletInfo(item);

                if (account != null)
                {
                    var uName = await GetValidatorInfo(account.Address);
                    SetText(this, skyLabel31, account.Address);
                    SetText(this, skyLabel29, account.Balance.ToString() + " RBX");
                    SetText(this, skyLabel27, account.IsValidating == true ? "Yes" : "No");
                    SetText(this, skyLabel35, account.Balance >= 1000M ? "Yes" : "No");
                    SetText(this, valiNameLabel, uName);
                }
                else
                {

                }

            }
        }

        #endregion

        #region Wallet Form Closing Event
        private void WalletForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Exit();
            //proc.Kill();
            //proc.Dispose();
        }

        #endregion

        #region Move side panel options

        private void MoveSidePanel(Control c)
        {
            SidePanel.Height = c.Height;
            SidePanel.Top = c.Top;
        }
        private void dashBtn_Click(object sender, EventArgs e)
        {
            MoveSidePanel(dashBtn);
            foreverTabPage1.SelectedIndex = 0;
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            MoveSidePanel(sendBtn);
            foreverTabPage1.SelectedIndex = 1;
        }

        private void recBtn_Click(object sender, EventArgs e)
        {
            MoveSidePanel(recBtn);
            foreverTabPage1.SelectedIndex = 2;
        }

        private void tranBtn_Click(object sender, EventArgs e)
        {
            MoveSidePanel(tranBtn);
            foreverTabPage1.SelectedIndex = 3;
        }

        private void valiBtn_Click(object sender, EventArgs e)
        {
            MoveSidePanel(valiBtn);
            foreverTabPage1.SelectedIndex = 4;
        }

        private void nftBtn_Click(object sender, EventArgs e)
        {
            MoveSidePanel(nftBtn);
            foreverTabPage1.SelectedIndex = 5;
        }

        #endregion

        #region Show dialong options
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        public static string ShowDialogValidator(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text, Width = 300 };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
        #endregion

        private async void lostAcceptButton5_Click(object sender, EventArgs e)
        {
            if(IsWalletSyncing == false)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to send funds?", "Are you sure you want to send funds?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //do something
                    var txFrom = txSendAddressDropDown.SelectedItem.Text;
                    var txTo = sendToTextBox.Text;
                    var amount = amountSendTextBox.Text;

                    if (txFrom != "" && txTo != "" && amount != "")
                    {
                        var result = await SendTxOut(txFrom, txTo, amount);
                        MessageBox.Show(result);
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    //do something else
                }
            }
            else
            {
                MessageBox.Show("Wallet is currently syncing. Please wait for it to finish.");
            }
        }

        private async void recGetNewAddressBtn_Click(object sender, EventArgs e)
        {
            var data = await GetNewAddress();

            var addrInfo = data.Split(':');

            var nAddr = addrInfo[0];
            var nPrivKey = addrInfo[1];

            var post = new StringBuilder();

            post.AppendLine("New Address Created!");
            post.Append(Environment.NewLine);
            post.AppendLine("Public Address: " + nAddr);
            post.Append(Environment.NewLine);
            post.AppendLine("Private Key: " + nPrivKey);
            post.Append(Environment.NewLine);
            post.AppendLine("Please make a back up of your private key! If it is lost you cannot recover account.");
            post.Append(Environment.NewLine);
            post.AppendLine("Never share your private key with anyone else!");
            post.Append(Environment.NewLine);

            post.AppendLine("Your private key has been copied to clipboard.");
            post.Append(Environment.NewLine);

            MessageBox.Show(post.ToString());

            Clipboard.SetText(nPrivKey);

            var addr = new CrownDropDownItem(nAddr);

            txSendAddressDropDown.Items.Add(addr);
            recAddressDropDownList.Items.Add(addr);
        }

        private async void startValiBtn_Click(object sender, EventArgs e)
        {
            if(IsWalletSyncing == false)
            {
                var post = new StringBuilder();
                if (valiDropDownList.SelectedItem == null)
                {
                    MessageBox.Show("You must select an address!");
                }
                else
                {
                    var addr = valiDropDownList.SelectedItem.Text;

                    if (addr != "")
                    {
                        string promptValue = ShowDialogValidator("Choose a Unique Name for your Masternode.", "Name your Masternode!");

                        if (promptValue != "")
                        {
                            DialogResult dialogResult = MessageBox.Show("Are you sure you want to activate masternode?", "Are you sure you?", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                //do something
                                var result = await StartValidating(addr, promptValue);
                                MessageBox.Show(result);

                            }
                            else if (dialogResult == DialogResult.No)
                            {
                                //do something else
                            }
                        }


                    }
                    //MessageBox.Show(post.ToString());
                }
            }
            else
            {
                MessageBox.Show("Wallet is currently syncing. Please wait for it to finish.");
            }


        }

        private async void dashPrintAllAddrBtn_Click(object sender, EventArgs e)
        {
            await DashPrintWalletAddresses();
        }

        private void dashClearMainBtn_Click(object sender, EventArgs e)
        {
            dashboardMainBox.Clear();
        }

        private void chatButtonRight1_Click(object sender, EventArgs e)
        {
            if(recAddressLabel.Text != "--" && recAddressLabel.Text.StartsWith("R"))
            {
                Clipboard.SetText(recAddressLabel.Text);
                MessageBox.Show("Address copied to clipboard.");
            }
        }

        private async void recImpPrvKey_Click(object sender, EventArgs e)
        {
            string promptValue = ShowDialog("Insert Private Key", "Import Private Key");

            if(promptValue != "")
            {
                var data = await ImportPrivateKey(promptValue);

                if (data != "FAIL")
                {
                    var account = JsonConvert.DeserializeObject<Account>(data);

                    var message = "Account: " + account.Address + " imported.";

                    MessageBox.Show(message);

                    await SetWalletInfo();
                    await GetWalletValidatorAddresses();
                    await UpdateBalance();

                    var addr = new CrownDropDownItem(account.Address);
                    txSendAddressDropDown.Items.Add(addr);
                    recAddressDropDownList.Items.Add(addr);
                }
            }
        }

    }
}
