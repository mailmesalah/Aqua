using ServerServiceInterface;
using System;
using System.ServiceModel;
using System.Windows;
using AquaClient.General;
using AquaClient.Registers.Company;
using AquaClient.Settings.Company;
using ThreeDigitClient.Transactions.Company;
using AquaClient.Reports.Company;

namespace AquaClient.Transactions.Company
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        bool logout = false;

        public Dashboard()
        {
            InitializeComponent();
            GetUserDetails();
        }

        private void GetUserDetails()
        {
            try
            {
                using (ChannelFactory<IUserRegister> userProxy = new ChannelFactory<ServerServiceInterface.IUserRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/UserRegisterEndPoint");
                    IUserRegister userService = userProxy.CreateChannel(ep);
                    CUser user = userService.ReadUser(ApplicationStaticVariables.gClientLocalInfo.UserId, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    this.Title += " - " + user.Name;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void MenuUsers_Click(object sender, RoutedEventArgs e)
        {
            Users ur = new Users();
            ur.ShowDialog();
        }

        private void MenuAccounts_Click(object sender, RoutedEventArgs e)
        {
            Accounts dr = new Accounts();
            dr.ShowDialog();
        }

        private void Dashboard_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!logout)
            {
                Application.Current.Shutdown();                
            }
            
        }

        private void MenuDefaultValueSettings_Click(object sender, RoutedEventArgs e)
        {
            DefaultValueSettings dvs = new DefaultValueSettings();
            dvs.Show();
        }
        
        private void menuLogout_Click(object sender, RoutedEventArgs e)
        {
            logout = true;
            Login l = new Login();
            l.Show();
            this.Close();
        }


        private void MenuOptimizor_Click(object sender, RoutedEventArgs e)
        {
            Optimizor o = new Optimizor();
            o.Show();
        }

        private void MenuSessionManager_Click(object sender, RoutedEventArgs e)
        {
            SessionManager sm = new SessionManager();
            sm.Show();
        }

        private void MenuPayment_Click(object sender, RoutedEventArgs e)
        {
            Payment p = new Payment();
            p.Show();
        }

        private void MenuReceipt_Click(object sender, RoutedEventArgs e)
        {
            Receipt r = new Receipt();
            r.Show();
        }

        private void MenuDayBook_Click(object sender, RoutedEventArgs e)
        {
            DayBook db = new DayBook();
            db.Show();
        }

        private void MenuTrialBalance_Click(object sender, RoutedEventArgs e)
        {
            TrialBalance tb = new TrialBalance();
            tb.Show();
        }

        private void MenuAssetnLiability_Click(object sender, RoutedEventArgs e)
        {
            AssetsnLiabilities anl = new AssetsnLiabilities();
            anl.Show();
        }

        private void MenuIncomenExpense_Click(object sender, RoutedEventArgs e)
        {
            IncomenExpense ie = new IncomenExpense();
            ie.Show();
        }
    }
}
