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

        private void MenuAdmins_Click(object sender, RoutedEventArgs e)
        {
            Users ur = new Users();
            ur.ShowDialog();
        }

        private void MenuDealers_Click(object sender, RoutedEventArgs e)
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

        private void MenuTicketLimits_Click(object sender, RoutedEventArgs e)
        {
            
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

        private void MenuUsers_Click(object sender, RoutedEventArgs e)
        {
            Users u = new Users();
            u.Show();
        }

        private void MenuAccounts_Click(object sender, RoutedEventArgs e)
        {
            Accounts a = new Accounts();
            a.Show();
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

        private void MenuReceivable_Click(object sender, RoutedEventArgs e)
        {
            Receivable r = new Receivable();
            r.Show();
        }

        private void MenuPayable_Click(object sender, RoutedEventArgs e)
        {
            Payable p = new Payable();
            p.Show();
        }

        private void MenuAccountTransfer_Click(object sender, RoutedEventArgs e)
        {
            AccountTransfer at = new AccountTransfer();
            at.Show();
        }

        private void MenuPurchase_Click(object sender, RoutedEventArgs e)
        {
            Purchase p = new Purchase();
            p.Show();
        }

        private void MenuSales_Click(object sender, RoutedEventArgs e)
        {
            Sales s = new Sales();
            s.Show();
        }

        private void MenuAppreciation_Click(object sender, RoutedEventArgs e)
        {
            Appreciation a = new Appreciation();
            a.Show();
        }

        private void MenuDepreciation_Click(object sender, RoutedEventArgs e)
        {
            Depreciation d = new Depreciation();
            d.Show();
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

        private void MenuCreditPayment_Click(object sender, RoutedEventArgs e)
        {
            CreditPayment cp = new CreditPayment();
            cp.Show();
        }

        private void MenuCreditReceipt_Click(object sender, RoutedEventArgs e)
        {
            CreditReceipt cr = new CreditReceipt();
            cr.Show();
        }
    }
}
