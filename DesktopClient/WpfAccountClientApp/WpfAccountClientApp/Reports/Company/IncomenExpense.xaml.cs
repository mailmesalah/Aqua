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
    /// Interaction logic for IncomenExpense.xaml
    /// </summary>
    public partial class IncomenExpense : Window
    {
        List<CInEReportItem> mIncomeGridContent = new List<CInEReportItem>();
        List<CInEReportItem> mExpenseGridContent = new List<CInEReportItem>();

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public IncomenExpense()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {
            grdIncome.ItemsSource = mIncomeGridContent;
            grdExpense.ItemsSource = mExpenseGridContent;

            cmbBillType.Items.Add("Payment");
            cmbBillType.Items.Add("Receipt");
            cmbBillType.Items.Add("Payable");
            cmbBillType.Items.Add("Receivable");
            cmbBillType.Items.Add("Account Transfer");

            cmbMainGroup.Items.Add("Monetary");
            cmbMainGroup.Items.Add("Personal");            
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

        private void GetReport(DateTime startDate, DateTime endDate,int billType, int mainGroup, long parentGroupId, long accountId, long billNo)
        {
            try
            {
                using (ChannelFactory<ITransaction> proxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                    ITransaction service = proxy.CreateChannel(ep);
                    CIncomenExpenseReportRE retObj = service.ReadIncomenExpense(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    if (!retObj.Success)
                    {
                        MessageBox.Show(retObj.Message);
                    }
                    else
                    {
                        List<CInEReportItem> income = new List<CInEReportItem>();
                        string lastGroup = "";
                        foreach(var item in retObj.Income)
                        {
                            string sgroup = item.MainGroup == 0 ? "Monetary" : item.MainGroup == 1 ? "Personal" : item.MainGroup == 2 ?  "Credit" : "";
                            if (!lastGroup.Equals(sgroup))
                            {
                                item.SMainGroup = sgroup;
                                lastGroup = sgroup;
                            }                            
                            income.Add(item);                            
                        }

                        List<CInEReportItem> expense = new List<CInEReportItem>();
                        lastGroup = "";
                        foreach (var item in retObj.Expense)
                        {
                            string sgroup = item.MainGroup == 0 ? "Monetary" : item.MainGroup == 1 ? "Personal" : item.MainGroup == 2 ? "Credit" : "";
                            if (!lastGroup.Equals(sgroup))
                            {
                                item.SMainGroup = sgroup;
                                lastGroup = sgroup;
                            }
                            
                            expense.Add(item);                            
                        }

                        mIncomeGridContent = income;
                        mExpenseGridContent = expense;                                                                        

                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);                
            }                      
        }

        private string GetBillTypeName(int billType)
        {
            switch (billType)
            {
                case CBillTypes.PERSONAL_PAYMENT:
                    return "Payment";
                case CBillTypes.PERSONAL_RECEIPT:
                    return "Receipt";
                case CBillTypes.PAYABLE:
                    return "Payable";
                case CBillTypes.RECEIVABLE:
                    return "Receivable";
                case CBillTypes.ACCOUNT_TRANSFER:
                    return "Account Transfer";
            }
            return "";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private async void btnShow_Click(object sender, RoutedEventArgs e)
        {

            mIncomeGridContent.Clear();

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

                await Task.Factory.StartNew(() => this.GetReport(startDate, endDate, billType, mainGroup, parentGroupId, accountId, billNo));

                lblTotalDebit.Content = mIncomeGridContent.Sum(c => c.Amount).ToString("N2");
                lblTotalCredit.Content = mExpenseGridContent.Sum(c => c.Amount).ToString("N2");
                grdIncome.ItemsSource = mIncomeGridContent;
                grdExpense.ItemsSource = mExpenseGridContent;
                grdIncome.Items.Refresh();
                grdExpense.Items.Refresh();
            }
            catch { }
        }
        
        private void ScrollGridToLast()
        {
            if (mIncomeGridContent.Count > 0)
            {
                grdIncome.ScrollIntoView(mIncomeGridContent.ElementAt(mIncomeGridContent.Count - 1));
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
