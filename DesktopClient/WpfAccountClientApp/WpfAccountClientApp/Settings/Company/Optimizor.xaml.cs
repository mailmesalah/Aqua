using ServerServiceInterface;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using AquaClient.General;

namespace AquaClient.Settings.Company
{
    /// <summary>
    /// Interaction logic for DefaultValueSettings.xaml
    /// </summary>
    public partial class Optimizor : Window
    {
        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public Optimizor()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {
            dtpDate.SelectedDate = DateTime.Now;
            ClearStatus();
        }
        /*
        private long ConvertTicketSalesCacheToTicketSales()
        {
            DateTime dDate = dtpDate.SelectedDate.Value;
            try
            {
                using (ChannelFactory<ITicketSales> ticketProxy = new ChannelFactory<ServerServiceInterface.ITicketSales>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TicketSalesEndPoint");
                    ITicketSales ticketService = ticketProxy.CreateChannel(ep);
                    CConvertReply retObj = ticketService.ConvertTicketSalesCacheToTicketSales(dDate, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    return retObj.Count;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;         
            }

        }

        private long GenerateWinners()
        {
            DateTime dDate = dtpDate.SelectedDate.Value;
            try
            {
                using (ChannelFactory<ITicketSales> ticketProxy = new ChannelFactory<ServerServiceInterface.ITicketSales>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TicketSalesEndPoint");
                    ITicketSales ticketService = ticketProxy.CreateChannel(ep);
                    CConvertReply retObj = ticketService.GenerateWinners(dDate, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    return retObj.Count;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }

        }

        private long GenerateProfitnLosses()
        {
            DateTime dDate = dtpDate.SelectedDate.Value;
            try
            {
                using (ChannelFactory<ITicketSales> ticketProxy = new ChannelFactory<ServerServiceInterface.ITicketSales>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TicketSalesEndPoint");
                    ITicketSales ticketService = ticketProxy.CreateChannel(ep);
                    CConvertReply retObj = ticketService.GenerateProfitnLoss(dDate, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    return retObj.Count;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }

        }

        private long GenerateAccounts()
        {
            DateTime dDate = dtpDate.SelectedDate.Value;
            try
            {
                using (ChannelFactory<ITicketSales> ticketProxy = new ChannelFactory<ServerServiceInterface.ITicketSales>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TicketSalesEndPoint");
                    ITicketSales ticketService = ticketProxy.CreateChannel(ep);
                    CConvertReply retObj = ticketService.GenerateAccounts(dDate, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    return retObj.Count;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }

        }

        private long GenerateProfitReport()
        {
            DateTime dDate = dtpDate.SelectedDate.Value;
            try
            {
                using (ChannelFactory<ITicketSales> ticketProxy = new ChannelFactory<ServerServiceInterface.ITicketSales>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TicketSalesEndPoint");
                    ITicketSales ticketService = ticketProxy.CreateChannel(ep);
                    CConvertReply retObj = ticketService.GenerateProfitReport(dDate, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    return retObj.Count;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }

        }

        private long RemoveBlockedTickets()
        {
            DateTime dDate = dtpDate.SelectedDate.Value;
            try
            {
                using (ChannelFactory<ITicketSales> ticketProxy = new ChannelFactory<ServerServiceInterface.ITicketSales>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TicketSalesEndPoint");
                    ITicketSales ticketService = ticketProxy.CreateChannel(ep);
                    CConvertReply retObj = ticketService.RemoveBlockedTickets(dDate, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    return retObj.Count;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }

        }

        private long RemoveTicketCounts()
        {
            DateTime dDate = dtpDate.SelectedDate.Value;
            try
            {
                using (ChannelFactory<ITicketSales> ticketProxy = new ChannelFactory<ServerServiceInterface.ITicketSales>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TicketSalesEndPoint");
                    ITicketSales ticketService = ticketProxy.CreateChannel(ep);
                    CConvertReply retObj = ticketService.RemoveTicketCounts(dDate, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    return retObj.Count;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }

        }*/

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();
           /* lblStatus1.Content = "Converting Tickets Sold";
            await Task.Delay(100);
            lblStatus1.Content = ConvertTicketSalesCacheToTicketSales().ToString() + " Items Converted";
            await Task.Delay(100);
            lblStatus2.Content = "Clearing Ticket Count Cache";
            await Task.Delay(100);
            lblStatus2.Content = RemoveTicketCounts().ToString() + " Ticket Count Cache Cleared";
            await Task.Delay(100);
            lblStatus3.Content = "Clearing Blocked Ticket Cache";
            await Task.Delay(100);
            lblStatus3.Content = RemoveBlockedTickets().ToString() + " Blocked Ticket Cache Cleared";
            await Task.Delay(100);
            lblStatus4.Content = "Generating Winners";
            await Task.Delay(100);
            lblStatus4.Content = GenerateWinners().ToString() + " Winners Generated";
            await Task.Delay(100);
            lblStatus5.Content = "Generating Profit n Loss";
            await Task.Delay(100);
            lblStatus5.Content = GenerateProfitnLosses().ToString() + " Profit n Loss Items Generated";
            await Task.Delay(100);
            lblStatus6.Content = "Generating Accounts";
            await Task.Delay(100);
            lblStatus6.Content = GenerateAccounts().ToString() + " Account Items Generated";
            await Task.Delay(100);
            lblStatus7.Content = "Generating Profit Report";
            await Task.Delay(100);
            lblStatus7.Content = GenerateProfitReport().ToString() + " Profit Report Items Generated";
            await Task.Delay(100);
            lblStatus8.Content = "Conversion Completed";            */
        }

        private void ClearStatus()
        {
            lblStatus1.Content = "";
            lblStatus2.Content = "";
            lblStatus3.Content = "";
            lblStatus4.Content = "";
            lblStatus5.Content = "";
            lblStatus6.Content = "";
            lblStatus7.Content = "";
            lblStatus8.Content = "";
        }

        private void dtpDate_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ClearStatus();
        }
    }
}
