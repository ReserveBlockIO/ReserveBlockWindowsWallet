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
        public  WalletForm()
        {
            InitializeComponent();
            txSendAddressDropDown.SelectedItemChanged += delegate { txSendAddressDropDown_SelectedIndexChanged(txSendAddressDropDown.SelectedItem.Text); };
            recAddressDropDownList.SelectedItemChanged += delegate { recAddressDropDownList_SelectedIndexChanged(recAddressDropDownList.SelectedItem.Text); };
            valiDropDownList.SelectedItemChanged += delegate { txSendAddressDropDown_SelectedIndexChanged(valiDropDownList.SelectedItem.Text); };
            this.FormClosing += WalletForm_FormClosing;
            walletInfo.AppendText("RBX Wallet Started on " + DateTime.Now.ToString());
            walletInfo.AppendText(Environment.NewLine);
            walletInfo.AppendText("Connecting to RBX Network.");
            //Perform connect to peers check
            walletInfo.AppendText(Environment.NewLine);
            walletInfo.AppendText("Connected. Looking for new blocks.");
            //Look for new blocks
            walletStartTimeDate.Text = DateTime.Now.ToString();

            ProcessStartInfo start =
            new ProcessStartInfo();
            start.FileName = Directory.GetCurrentDirectory() + @"\RBXCore\ReserveBlockCore.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden; //Hides GUI
            start.CreateNoWindow = true; //Hides console

            proc.StartInfo = start;
            proc.Start();

            nodeURL = "https://localhost:7777";

            GetWalletOnline();

            SetWalletInfo();

            GetWalletAddresses();

            GetWalletValidatorAddresses();

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
                                dashboardMainBox.AppendText("RBX Wallet has conneted to local node. Please wait for sync.");

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

                            peerCountLabel.Text = peerCount + " / 6";
                            blockHeightLabel.Text = blockHeight;


                            
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
                                    
                                    txSendAddressDropDown.Items.Add(addr);
                                    recAddressDropDownList.Items.Add(addr);
                                });

                                var bal = accounts.Sum(x => x.Balance).ToString();
                                dashMainBalLabel.Text = bal + " RBX";

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

        private async void txSendAddressDropDown_SelectedIndexChanged(string item)
        {
            var account = await GetWalletInfo(item);

            if(account != null)
            {
                txSendBalanceLabel.Text = account.Balance.ToString() + " RBX";
            }
            else
            {
                txSendBalanceLabel.Text = "0.00" + " RBX";
            }
            


        }

        private async void recAddressDropDownList_SelectedIndexChanged(string item)
        {
            var account = await GetWalletInfo(item);

            if (account != null)
            {
                recAddressLabel.Text = account.Address;
                recBalance.Text = account.Balance.ToString() + " RBX";
                recValiFlag.Text = account.IsValidating == true ? "Yes" : "No";
            }
            else
            {

            }



        }
        private void WalletForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            //proc.Kill();
            //proc.Dispose();
        }

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



        private void lostAcceptButton5_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to send funds to: ", "Are you sure you want to send funds?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //do something
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
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

        private void startValiBtn_Click(object sender, EventArgs e)
        {
            var post = new StringBuilder();

            post.AppendLine("The Testnet will allow new validators starting on 27 February 2022.");
            post.Append(Environment.NewLine);
            post.AppendLine("If you have any questions please reach out to us on Discord or through GitHub.");
            post.Append(Environment.NewLine);

            MessageBox.Show(post.ToString());
        }

        private async void dashPrintAllAddrBtn_Click(object sender, EventArgs e)
        {
            await DashPrintWalletAddresses();
        }

        private void dashClearMainBtn_Click(object sender, EventArgs e)
        {
            dashboardMainBox.Clear();
        }
    }
}
