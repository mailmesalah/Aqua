using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AquaServer.General;
using AquaServer.Services;
using AquaServer.UI;

namespace AquaServer
{
    
    public partial class MainWindow : Window
    {

        string strAddr = "http://localhost:55555" ;

        ServiceHost hostUserLoginService;
        ServiceHost hostUserRegisterService;
        ServiceHost hostAccountRegisterService;
        ServiceHost hostTransactionsService;
        ServiceHost hostSettingsService;

        ServiceHost hostRESTService;

        ServiceHost hostRESTDownloadService;
        ServiceHost hostSessionManagerService;


        public MainWindow()
        {
            InitializeComponent();

            //Initialising host object
            try
            {                
                BasicHttpBinding bhttpb = new BasicHttpBinding()
                {
                    MaxBufferSize = int.MaxValue,
                    MaxReceivedMessageSize = int.MaxValue,                    
                };
                
                bhttpb.ReaderQuotas.MaxStringContentLength = int.MaxValue;
                bhttpb.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
                bhttpb.ReaderQuotas.MaxDepth = int.MaxValue;
                bhttpb.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
                bhttpb.ReaderQuotas.MaxArrayLength = int.MaxValue;

                hostUserLoginService = new ServiceHost(typeof(Services.UserLoginService), new Uri(strAddr + "/UserLoginEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostUserLoginService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;
                        hostUserLoginService.Description.Behaviors.Add(throttleBehavior);
                    }
                }
                hostUserLoginService.AddServiceEndpoint(typeof(IUserLogin), bhttpb, strAddr+ "/UserLoginEndPoint");
                                                
                hostUserRegisterService = new ServiceHost(typeof(Services.UserRegisterService), new Uri(strAddr + "/UserRegisterEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostUserRegisterService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;
                        hostUserRegisterService.Description.Behaviors.Add(throttleBehavior);
                    }
                }
                hostUserRegisterService.AddServiceEndpoint(typeof(IUserRegister), bhttpb, strAddr + "/UserRegisterEndPoint");

                hostAccountRegisterService = new ServiceHost(typeof(Services.AccountRegisterService), new Uri(strAddr + "/AccountRegisterEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostAccountRegisterService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;
                        hostAccountRegisterService.Description.Behaviors.Add(throttleBehavior);
                    }
                }
                hostAccountRegisterService.AddServiceEndpoint(typeof(IAccountRegister), bhttpb, strAddr + "/AccountRegisterEndPoint");


                hostTransactionsService = new ServiceHost(typeof(Services.TransactionService), new Uri(strAddr + "/TransactionEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostTransactionsService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;
                        hostTransactionsService.Description.Behaviors.Add(throttleBehavior);
                    }
                }
                hostTransactionsService.AddServiceEndpoint(typeof(ITransaction), bhttpb, strAddr + "/TransactionEndPoint");

                
                hostSettingsService = new ServiceHost(typeof(Services.SettingsService), new Uri(strAddr + "/SettingsEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostSettingsService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;
                        hostSettingsService.Description.Behaviors.Add(throttleBehavior);
                    }
                }
                hostSettingsService.AddServiceEndpoint(typeof(ISettings), bhttpb, strAddr + "/SettingsEndPoint");

                //REST Service
                WebHttpBinding whttpb = new WebHttpBinding();
                whttpb.MaxBufferSize = 2147483647;
                whttpb.MaxReceivedMessageSize = 2147483647;
                whttpb.MaxBufferPoolSize = 2147483647;
                //whttpb.TransferMode = TransferMode.StreamedRequest;             

                hostRESTService = new ServiceHost(typeof(Services.RESTService), new Uri(strAddr + "/RESTEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostRESTService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;                        
                        hostRESTService.Description.Behaviors.Add(throttleBehavior);
                    }

                    /*DataContractSerializerOperationBehavior dataContractBehavior = hostRESTService.Description.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                        dataContractBehavior.IgnoreExtensionDataObject = false;                        
                    }*/
                }
                hostRESTService.AddServiceEndpoint(typeof(IRESTService), whttpb, strAddr + "/RESTEndPoint").EndpointBehaviors.Add(new WebHttpBehavior());
        
                hostRESTDownloadService = new ServiceHost(typeof(Services.RESTDownloadAPK), new Uri(strAddr + "/RESTDownloadEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostRESTService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;
                        hostRESTService.Description.Behaviors.Add(throttleBehavior);
                    }

                }
                hostRESTDownloadService.AddServiceEndpoint(typeof(IRESTDownloadAPK), whttpb, strAddr + "/RESTDownloadEndPoint").EndpointBehaviors.Add(new WebHttpBehavior());


                hostSessionManagerService = new ServiceHost(typeof(Services.SessionManagerService), new Uri(strAddr + "/SessionManagerEndPoint"));
                {
                    ServiceThrottlingBehavior throttleBehavior = hostSessionManagerService.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                    if (throttleBehavior == null)
                    {
                        throttleBehavior = new ServiceThrottlingBehavior();
                        throttleBehavior.MaxConcurrentCalls = 1000;
                        throttleBehavior.MaxConcurrentSessions = 250;
                        throttleBehavior.MaxConcurrentInstances = 500;
                        hostSessionManagerService.Description.Behaviors.Add(throttleBehavior);
                    }
                }
                hostSessionManagerService.AddServiceEndpoint(typeof(ISessionManager), bhttpb, strAddr + "/SessionManagerEndPoint");


                //Open Service
                hostUserLoginService.Open();
                hostUserRegisterService.Open();
                hostAccountRegisterService.Open();
                hostTransactionsService.Open();
                hostSettingsService.Open();

               // hostRESTService.Open();

                hostRESTDownloadService.Open();

                hostSessionManagerService.Open();

                //Loading Static values
                //StaticGlobalInfos tdsgi = new StaticGlobalInfos();
                //StaticGlobalInfos.CreateTriggers();


                Console.WriteLine("Services are started and running");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //Closing the service hoster
            hostUserLoginService.Close();
            hostUserRegisterService.Close();
            hostAccountRegisterService.Close();
            hostTransactionsService.Close();
            hostSettingsService.Close();

            //hostRESTService.Close();

            hostRESTDownloadService.Close();
            hostSessionManagerService.Close();

            Console.WriteLine("Services are stopped");
        }

        private void menuCompany_Click(object sender, RoutedEventArgs e)
        {
            Company c = new Company();
            c.Show();
        }

        private void menuManage_Click(object sender, RoutedEventArgs e)
        {
            ManageCompany mc = new ManageCompany();
            mc.Show();
        }
    }
}
