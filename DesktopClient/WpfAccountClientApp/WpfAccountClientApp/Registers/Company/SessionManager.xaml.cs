using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace AquaClient.Registers.Company
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SessionManager : Window
    {        
        ObservableCollection<CSessionStatus> mGridContent = new ObservableCollection<CSessionStatus>();

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public SessionManager()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {
            NewTicketForm();
            GetSessionStatus();
        }

        private void NewTicketForm()
        {
            mGridContent.Clear();                   
        }


        private bool GetSessionStatus()
        {
            try
            {
                mGridContent.Clear();

                try
                {
                    using (ChannelFactory<ISessionManager> dProxy = new ChannelFactory<ServerServiceInterface.ISessionManager>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/SessionManagerEndPoint");
                        ISessionManager dService = dProxy.CreateChannel(ep);
                        List<CSessionStatus> sesStatus = dService.ReadSessionStatus(ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                        int ser = 0;
                        foreach (var i in sesStatus)
                        {
                            mGridContent.Add(new CSessionStatus { SerialNo = ++ser, UserId=i.UserId, Name=i.Name, UserType=i.UserType, IsActive=i.IsActive, Status=i.IsActive?"Connected":"Disconnected"});
                        }
                        grdSessions.ItemsSource = mGridContent;
                        grdSessions.Items.Refresh();

                        return (sesStatus != null && sesStatus.Count > 0);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }
       
        private bool DeleteFromDatabase(CSessionStatus cts)
        {
            if (mGridContent.Count > 0)
            {
                try
                {
                    using (ChannelFactory<ISessionManager> dProxy = new ChannelFactory<ServerServiceInterface.ISessionManager>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/SessionManagerEndPoint");
                        ISessionManager dService = dProxy.CreateChannel(ep);

                        CBoolMessage cbm = dService.RemoveSession(cts.UserId, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);

                        if (cbm.IsSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show(cbm.Message);
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;

                }

            }
            else
            {
                return false;
            }
        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClearSession_Click(object sender, RoutedEventArgs e)
        {
            CSessionStatus cts = ((FrameworkElement)sender).DataContext as CSessionStatus;

            if (!cts.IsActive)
            {
                return;
            }

            if (MessageBox.Show("Are you sure to Remove Session of "+cts.Name+" ?", "Remove Session", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                

                if (DeleteFromDatabase(cts))
                {
                    //Rearrange Serial No
                    GetSessionStatus();                         
                }
            }
        }
        
        private void ScrollGridToLast()
        {
            if (mGridContent.Count > 0)
            {
                grdSessions.ScrollIntoView(mGridContent.ElementAt(mGridContent.Count - 1));
            }
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            GetSessionStatus();
        }
    }
}
