using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AquaClient.General;

namespace AquaClient
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            SetControls();
            new CommonMethods();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CheckIfRequiredDataIsGiven()
        {
            if (txtServerIP.Visibility != Visibility.Hidden && txtServerIP.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please Provide Server IP Address", "Login Failed");
                txtServerIP.Focus();
                return false;
            }

            if (txtCompanyUsername.Visibility != Visibility.Hidden && txtCompanyUsername.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please Provide Company Username", "Login Failed");
                txtCompanyUsername.Focus();
                return false;
            }

            if (txtUsername.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please Provide Username", "Login Failed");
                txtUsername.Focus();
                return false;
            }

            if (txtUsername.Text.Trim().Length < 3 || txtUsername.Text.Trim().Length > 15)
            {
                MessageBox.Show("Username should be 3 to 15 characters long", "Login Failed");
                txtUsername.Focus();
                return false;
            }

            if (pswPassword.Password.Trim().Length == 0)
            {
                MessageBox.Show("Please Provide Password", "Login Failed");
                pswPassword.Focus();
                return false;
            }

            if (pswPassword.Password.Trim().Length < 3 || pswPassword.Password.Trim().Length > 15)
            {
                MessageBox.Show("Password should be 3 to 15 characters long", "Login Failed");
                pswPassword.Focus();
                return false;
            }
            return true;
        }

        private void SetControls()
        {
            try
            {
                CommonMethods.GetClientInfo();
                CommonMethods.GetServerInfo();

                if (ApplicationStaticVariables.gClientLocalInfo.CompanyUsername.Length > 0)
                {
                    lblCompany.Visibility = Visibility.Hidden;
                    txtCompanyUsername.Visibility = Visibility.Hidden;

                    lblServerIP.Visibility = Visibility.Hidden;
                    txtServerIP.Visibility = Visibility.Hidden;
                }
                else
                {
                    lblCompany.Visibility = Visibility.Visible;
                    txtCompanyUsername.Visibility = Visibility.Visible;

                    lblServerIP.Visibility = Visibility.Visible;
                    txtServerIP.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                lblCompany.Visibility = Visibility.Visible;
                txtCompanyUsername.Visibility = Visibility.Visible;

                lblServerIP.Visibility = Visibility.Visible;
                txtServerIP.Visibility = Visibility.Visible;
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginToDashboard();
        }

        private void LoginToDashboard()
        {
            if (CheckIfRequiredDataIsGiven())
            {
                try
                {
                    
                    string companyUsername = txtCompanyUsername.Visibility == Visibility.Hidden ? ApplicationStaticVariables.gClientLocalInfo.CompanyUsername : txtCompanyUsername.Text.Trim();
                    string username = txtUsername.Text.Trim();
                    string password = pswPassword.Password.Trim();

                    //Setting Server IP Address If Server IP Text control is visible
                    ApplicationStaticVariables.gServerAddress = txtServerIP.Visibility == Visibility.Visible ? "http://"+txtServerIP.Text.Trim()+":55555" : ApplicationStaticVariables.gServerAddress;

                    BasicHttpBinding bhttb = new BasicHttpBinding()
                    {
                        MaxBufferSize = int.MaxValue,
                        MaxReceivedMessageSize = int.MaxValue,
                    };
                    using (ChannelFactory<IUserLogin> loginProxy = new ChannelFactory<ServerServiceInterface.IUserLogin>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress+ "/UserLoginEndPoint");
                        IUserLogin loginService = loginProxy.CreateChannel(ep);
                        CClientLocalInfo csm = (CClientLocalInfo)loginService.GetLoginDetails(companyUsername,username,password,"Computer");

                        if (csm.IsSuccess)
                        {
                            //Set Current User Details
                            ApplicationStaticVariables.gClientLocalInfo = csm;

                            if (csm.UserType == CClientLocalInfo.SUPER_ADMIN)
                            {
                                Transactions.Company.Dashboard d = new Transactions.Company.Dashboard();
                                d.Show();
                            }                            

                            //Store Current Company details
                            CommonMethods.SetClientInfo();
                            CommonMethods.SetServerInfo();

                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show(csm.Message, "Login Failed");
                            txtUsername.Focus();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + " | " + e.InnerException + " | " + e.Source);
                }
            }
        }

        private void pswPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginToDashboard();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStaticVariables.gClientLocalInfo = new CClientLocalInfo();
            ApplicationStaticVariables.gServerAddress = "";
            CommonMethods.SetClientInfo();
            CommonMethods.SetServerInfo();
            SetControls();
        }
    }
}
