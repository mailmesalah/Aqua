using AquaClient.General;
using Microsoft.Win32;
using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AquaClient.Reports.Company
{
    /// <summary>
    /// Interaction logic for DayBook.xaml
    /// </summary>
    public partial class DayBook : Window
    {
        List<CDayBookReportItem> mGridContent = new List<CDayBookReportItem>();

        private int mCurrentLimitIndex = 0;
        private int mLimitSize = 100;
        private bool bReportLoading = true;
        private bool bReportFinished = false;

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public DayBook()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {
            grdReport.ItemsSource = mGridContent;
            
            cmbBillType.Items.Add("Personal Payment");
            cmbBillType.Items.Add("Personal Receipt");
            cmbBillType.Items.Add("Credit Payment");
            cmbBillType.Items.Add("Credit Receipt");
            cmbBillType.Items.Add("Payable");
            cmbBillType.Items.Add("Receivable");
            cmbBillType.Items.Add("Account Transfer");
            cmbBillType.Items.Add("Purchase");
            cmbBillType.Items.Add("Sales");
            cmbBillType.Items.Add("Appreciation");
            cmbBillType.Items.Add("Depreciation");

            cmbMainGroup.Items.Add("Monetary");
            cmbMainGroup.Items.Add("Personal");
            cmbMainGroup.Items.Add("Stock");
            cmbMainGroup.Items.Add("Credit");

            GetParentAccountsForCombo();
            GetAccountsForCombo();

            dtpFromDate.SelectedDate = DateTime.Now;
            dtpToDate.SelectedDate = DateTime.Now;

            lblTotalDebit.Content = "0.00";
            lblTotalCredit.Content = "0.00";
        }

        private void GetParentAccountsForCombo()
        {
            try
            {
                using (ChannelFactory<IAccountRegister> accountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister accountService = accountProxy.CreateChannel(ep);
                    List<CAccount> accounts = null;

                    if (cmbMainGroup.SelectedIndex == 0)
                    {
                        accounts = accountService.ReadAllParentGroupsUnderMainGroup(CAccount.MONETARY_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    }
                    else if (cmbMainGroup.SelectedIndex == 1)
                    {
                        accounts = accountService.ReadAllParentGroupsUnderMainGroup(CAccount.PERSONAL_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    }
                    else if (cmbMainGroup.SelectedIndex == 2)
                    {
                        accounts = accountService.ReadAllParentGroupsUnderMainGroup(CAccount.STOCK_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    }
                    else if (cmbMainGroup.SelectedIndex == 3)
                    {
                        accounts = accountService.ReadAllParentGroupsUnderMainGroup(CAccount.CREDIT_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    }                    

                    cmbParentGroup.ItemsSource = accounts;
                    cmbParentGroup.SelectedValuePath = "Id";
                    cmbParentGroup.DisplayMemberPath = "Name";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void GetAccountsForCombo()
        {
            try
            {
                using (ChannelFactory<IAccountRegister> accountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister accountService = accountProxy.CreateChannel(ep);
                    List<CAccount> accounts = null;

                    if (cmbParentGroup.SelectedIndex == -1)
                    {
                        if (cmbMainGroup.SelectedIndex == 0)
                        {
                            accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.MONETARY_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                        }
                        else if (cmbMainGroup.SelectedIndex == 1)
                        {
                            accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.PERSONAL_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                        }
                        else if (cmbMainGroup.SelectedIndex == 2)
                        {
                            accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.STOCK_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                        }
                        else if (cmbMainGroup.SelectedIndex == 3)
                        {
                            accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.CREDIT_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                        }
                        else
                        {
                            accounts = accountService.ReadAllAccountTypes(ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                        }
                    }
                    else
                    {
                        CAccount ca = (CAccount)cmbParentGroup.SelectedItem;
                        accounts = accountService.ReadAllAccountsUnderParentGroup(ca.Id,ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    }

                    cmbAccount.ItemsSource = accounts;
                    cmbAccount.SelectedValuePath = "Id";
                    cmbAccount.DisplayMemberPath = "Name";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private List<CDayBookReportItem> GetReport(DateTime startDate, DateTime endDate,int billType, int mainGroup, long parentGroupId, long accountId, long billNo, int limitIndex, int limitSize)
        {
            
            try
            {
                using (ChannelFactory<ITransaction> proxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                    ITransaction service = proxy.CreateChannel(ep);
                    CDayBookReportRE retObj= service.ReadDayBook(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo, limitIndex, limitSize, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                        return null;
                    }
                    else
                    {                        
                        foreach(var item in retObj.Report)
                        {
                            string sgroup = item.MainGroup == 0 ? "Monetary" : item.MainGroup == 1 ? "Personal" : item.MainGroup == 2 ? "Stock" : item.MainGroup == 3 ? "Credit" : "";
                            item.Narration = sgroup + item.Narration;
                            item.SBillType = GetBillTypeName(item.BillType);
                        }
                        return retObj.Report;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
            
           
        }

        private string GetBillTypeName(int billType)
        {
            switch (billType)
            {
                case CBillTypes.PERSONAL_PAYMENT:
                    return "Personal Payment";
                case CBillTypes.PERSONAL_RECEIPT:
                    return "Personal Receipt";                
                case CBillTypes.CREDIT_PAYMENT:
                    return "Credit Payment";
                case CBillTypes.CREDIT_RECEIPT:
                    return "Credit Receipt";
                case CBillTypes.PAYABLE:
                    return "Payable";
                case CBillTypes.RECEIVABLE:
                    return "Receivable";
                case CBillTypes.PURCHASE:
                    return "Purchase";
                case CBillTypes.SALES:
                    return "Sales";
                case CBillTypes.ACCOUNT_TRANSFER:
                    return "Account Transfer";
                case CBillTypes.APPRECIATION:
                    return "Appreciation";
                case CBillTypes.DEPRECIATION:
                    return "Depreciation";
            }
            return "";
        }

        private void ShowTotal(DateTime startDate, DateTime endDate,int billType, int mainGroup, long parentGroupId, long accountId, long billNo)
        {
            
            try
            {
                using (ChannelFactory<ITransaction> proxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                    ITransaction service = proxy.CreateChannel(ep);
                    CDayBookReportTotalRE total = service.ReadDayBookTotal(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (total.Success)
                    {
                        lblTotalDebit.Content = total.Total.Debit.ToString("N2");
                        lblTotalCredit.Content = total.Total.Credit.ToString("N2");
                    }
                    else
                    {
                        MessageBox.Show(total.Message);

                        lblTotalDebit.Content = "0.00";
                        lblTotalCredit.Content = "0.00";
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);

            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private async void btnShow_Click(object sender, RoutedEventArgs e)
        {
            mCurrentLimitIndex = 0;
            bReportFinished = false;
            mGridContent.Clear();

            try
            {
                bReportLoading = true;

                int billNo = -1;
                try
                {
                    billNo = Convert.ToInt32(txtBillNo.Text.Trim());
                }
                catch
                {

                }

                int billType = -1;
                if (cmbBillType.SelectedIndex > -1)
                {
                    switch (cmbBillType.SelectedIndex)
                    {

                        case 0:
                            billType = CBillTypes.PERSONAL_PAYMENT;
                            break;
                        case 1:
                            billType = CBillTypes.PERSONAL_RECEIPT;
                            break;
                        case 2:
                            billType = CBillTypes.CREDIT_PAYMENT;
                            break;
                        case 3:
                            billType = CBillTypes.CREDIT_RECEIPT;
                            break;
                        case 4:
                            billType = CBillTypes.PAYABLE;
                            break;
                        case 5:
                            billType = CBillTypes.RECEIVABLE;
                            break;
                        case 6:
                            billType = CBillTypes.ACCOUNT_TRANSFER;
                            break;
                        case 7:
                            billType = CBillTypes.PURCHASE;
                            break;
                        case 8:
                            billType = CBillTypes.SALES;
                            break;
                        case 9:
                            billType = CBillTypes.APPRECIATION;
                            break;
                        case 10:
                            billType = CBillTypes.DEPRECIATION;
                            break;

                    }                    
                }

                int mainGroup = cmbMainGroup.SelectedIndex;

                long parentGroupId = -1;
                if (cmbParentGroup.SelectedIndex > -1)
                {
                    parentGroupId = (long)cmbParentGroup.SelectedValue;
                }

                long accountId = -1;
                if (cmbAccount.SelectedIndex > -1)
                {
                    accountId = (long)cmbAccount.SelectedValue;
                }

                DateTime startDate = dtpFromDate.SelectedDate.Value;
                DateTime endDate = dtpToDate.SelectedDate.Value;

                var listData = await Task.Factory.StartNew<List<CDayBookReportItem>>(() => this.GetReport(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo, mCurrentLimitIndex, mLimitSize));
                mGridContent.AddRange(listData);
                grdReport.Items.Refresh();

                if (listData.Count < mLimitSize)
                {
                    bReportFinished = true;
                }
                mCurrentLimitIndex += mLimitSize;

                ShowTotal(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo);

                bReportLoading = false;
            }
            catch { }
        }
        
        private void ScrollGridToLast()
        {
            if (mGridContent.Count > 0)
            {
                grdReport.ScrollIntoView(mGridContent.ElementAt(mGridContent.Count - 1));
            }
        }

        private async void grdReport_ScrollChanged(object sender, RoutedEventArgs e)
        {
            var scroll = CommonMethods.FindVisualChild<ScrollViewer>((DependencyObject)sender);
            int lastRow = (int)scroll.VerticalOffset + (int)scroll.ViewportHeight;
            if (mGridContent.Count - lastRow <= 20 && !bReportLoading && !bReportFinished)
            {
                try
                {
                    bReportLoading = true;

                    int billNo = -1;
                    try
                    {
                        billNo = Convert.ToInt32(txtBillNo.Text.Trim());
                    }
                    catch
                    {

                    }

                    int billType = -1;
                    if (cmbBillType.SelectedIndex > -1)
                    {
                        switch (cmbBillType.SelectedIndex)
                        {

                            case 0:
                                billType = CBillTypes.PERSONAL_PAYMENT;
                                break;
                            case 1:
                                billType = CBillTypes.PERSONAL_RECEIPT;
                                break;
                            case 2:
                                billType = CBillTypes.PAYABLE;
                                break;
                            case 3:
                                billType = CBillTypes.RECEIVABLE;
                                break;
                            case 4:
                                billType = CBillTypes.ACCOUNT_TRANSFER;
                                break;
                            case 5:
                                billType = CBillTypes.PURCHASE;
                                break;
                            case 6:
                                billType = CBillTypes.SALES;
                                break;
                            case 7:
                                billType = CBillTypes.APPRECIATION;
                                break;
                            case 8:
                                billType = CBillTypes.DEPRECIATION;
                                break;

                        }
                    }

                    int mainGroup = cmbMainGroup.SelectedIndex;

                    long parentGroupId = -1;
                    if (cmbParentGroup.SelectedIndex > -1)
                    {
                        parentGroupId = (long)cmbParentGroup.SelectedValue;
                    }

                    long accountId = -1;
                    if (cmbAccount.SelectedIndex > -1)
                    {
                        accountId = (long)cmbAccount.SelectedValue;
                    }

                    DateTime startDate = dtpFromDate.SelectedDate.Value;
                    DateTime endDate = dtpToDate.SelectedDate.Value;

                    var listData = await Task.Factory.StartNew<List<CDayBookReportItem>>(() => this.GetReport(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo, mCurrentLimitIndex, mLimitSize));
                    mGridContent.AddRange(listData);
                    grdReport.Items.Refresh();

                    if (listData.Count < mLimitSize)
                    {
                        bReportFinished = true;
                    }
                    mCurrentLimitIndex += mLimitSize;

                    ShowTotal(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo);

                    bReportLoading = false;
                }
                catch { }
            }
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            /*DateTime date = System.DateTime.Now;
            string sdatetime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}", date.Year, date.Month, date.Day, date.Hour, date.Minute);

            try
            {
                int billNo = -1;
                try
                {
                    billNo = Convert.ToInt32(txtBillNo.Text.Trim());
                }
                catch
                {

                }

                long userId = -1;
                if (cmbUser.SelectedIndex > -1)
                {
                    userId = (long)cmbUser.SelectedValue;
                }

                long ticketId = -1;
                if (cmbTicket.SelectedIndex > -1)
                {
                    ticketId = (long)cmbTicket.SelectedValue;
                }

                DateTime startDate = dtpFromDate.SelectedDate.Value;
                DateTime endDate = dtpToDate.SelectedDate.Value;

                string ticketNo = txtTicketNo.Text.Trim();

                var listData = await Task.Factory.StartNew<List<CACTransactionItem>>(() => this.DownloadReport(startDate, endDate, userId, ticketId, billNo, ticketNo));

                if (listData != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var i in listData)
                    {
                        sb.AppendLine(i.BillDate + "," + i.UserName + "," + i.BillNo + "," + i.SerialNo + "," + i.TicketName + "," + i.TicketNo + "," + i.Quantity + "," + i.Amount);
                    }

                    //Save to Local Computer
                    SaveFileDialog dialog = new SaveFileDialog()
                    {
                        Filter = "CSV Files(*.csv)|*.csv", FileName = "SalesAdminReport_" + sdatetime
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        File.WriteAllText(dialog.FileName, sb.ToString());
                        MessageBox.Show("Successfully Downloaded File : " + dialog.FileName + ".csv");
                    }
                    else
                    {
                        File.WriteAllText("SalesAdminReport_" + sdatetime + ".csv", sb.ToString());
                        MessageBox.Show("Successfully Downloaded File : SalesAdminReport_" + sdatetime + ".csv");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } */           

        }

        private List<CACTransactionItem> DownloadReport(DateTime startDate, DateTime endDate, long userId, long ticketId, long billNo, string ticketNo)
        {
            
            try
            {
                using (ChannelFactory<ITransaction> proxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                    ITransaction service = proxy.CreateChannel(ep);
                    //return service.ReadAllTicketSalesAdminReport(startDate, endDate, userId, ticketId, billNo, ticketNo, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
            return null;
        }

        private void cmbMainGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetParentAccountsForCombo();
            GetAccountsForCombo();
        }

        private void cmbParentGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetAccountsForCombo();
        }
    }
}
