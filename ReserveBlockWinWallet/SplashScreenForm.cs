namespace ReserveBlockWinWallet
{
    public partial class SplashScreenForm : Form
    {
        public SplashScreenForm()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 2500;
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

            void Timer_Tick(object sender, EventArgs e)
            {
                this.Dispose();
            }
        }

    }
}