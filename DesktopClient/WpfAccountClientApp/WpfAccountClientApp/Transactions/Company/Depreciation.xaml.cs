using AquaClient.General;
using AquaClient.Registers.Company;
using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ThreeDigitClient.Transactions.Company
{
    /// <summary>
    /// Interaction logic for Depreciation.xaml
    /// </summary>
    public partial class Depreciation : Window
    {
        private static int BillType = CBillTypes.DEPRECIATION;
        private CACTransactionParam mCurrentBillItem;
        private CACTransactionItem mCurrentCreditItem;
        ObservableCollection<CACTransactionItem> mGridContent = new ObservableCollection<CACTransactionItem>();

        private List<CAccount> mCAccounts = new List<CAccount>();
        private List<CAccount> mSAccounts = new List<CAccount>();

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public Depreciation()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {

            GetCreditAccountsForCombo();
            GetStockAccountsForCombo();
            NewBillForm();
        }

        private void GetCreditAccountsForCombo()
        {
            mCAccounts.Clear();
            try
            {
                using (ChannelFactory<IAccountRegister> accountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister accountService = accountProxy.CreateChannel(ep);
                    List<CAccount> accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.CREDIT_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    mCAccounts = accounts;

                    cmbCreditAccount.ItemsSource = accounts;
                    cmbCreditAccount.SelectedValuePath = "Id";
                    cmbCreditAccount.DisplayMemberPath = "Name";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void GetStockAccountsForCombo()
        {
            mSAccounts.Clear();
            try
            {
                using (ChannelFactory<IAccountRegister> accountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister accountService = accountProxy.CreateChannel(ep);
                    List<CAccount> accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.STOCK_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    mSAccounts = accounts;

                    cmbStockAccount.ItemsSource = accounts;
                    cmbStockAccount.SelectedValuePath = "Id";
                    cmbStockAccount.DisplayMemberPath = "Name";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void NewBillForm()
        {
            mCurrentBillItem = null;
            dtpDate.SelectedDate = DateTime.Now;
            mGridContent.Clear();
            grdStocks.ItemsSource = mGridContent;
            txtBillNo.Text = "";
            cmbCreditAccount.SelectedIndex = -1;
            ClearEditBoxes();
            cmbCreditAccount.Focus();

            long billNo = GetNextBillNo();
            txtBillNo.Text = billNo > 0 ? billNo.ToString() : "";
        }

        private long GetNextBillNo()
        {
            long billNo = 0;
            try
            {

                using (ChannelFactory<ITransaction> ticketProxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                    ITransaction transactionService = ticketProxy.CreateChannel(ep);
                    billNo = transactionService.ReadNextBillNo(BillType, ApplicationStaticVariables.gClientLocalInfo.CompanyId);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return billNo;
        }

        private void ClearEditBoxes()
        {
            mCurrentCreditItem = null;
            cmbStockAccount.SelectedIndex = -1;
            txtAmount.Text = "";
            lblSerialNo.Content = mGridContent.Count + 1;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (txtBillNo.Text.Trim().Length<=0)
            {
                MessageBox.Show("Please provide Bill No");
                return;
            }

            if (MessageBox.Show("Are you sure to Delete the Bill?", "Delete Bill", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DeleteFromDatabase())
                {
                    NewBillForm();
                }
            }
        }

        private bool DeleteFromDatabase()
        {
            if (txtBillNo.Text.Trim().Length > 0)
            {
                try
                {
                    using (ChannelFactory<ITransaction> proxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                        ITransaction service = proxy.CreateChannel(ep);

                        CBoolMessage cbm = service.DeleteBill(long.Parse(txtBillNo.Text.Trim()), BillType, mCurrentBillItem.FinancialCode, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);

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

        private void btnNewBill_Click(object sender, RoutedEventArgs e)
        {
            NewBillForm();
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to Delete the Bill Item?", "Delete Bill Item", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CACTransactionItem cts = ((FrameworkElement)sender).DataContext as CACTransactionItem;
                mGridContent.Remove(cts);
                for (int i = 0; i < mGridContent.Count; i++)
                {
                    mGridContent.ElementAt(i).SerialNo = mGridContent.ElementAt(i).SerialNo > cts.SerialNo ? --mGridContent.ElementAt(i).SerialNo : mGridContent.ElementAt(i).SerialNo;
                }
                grdStocks.Items.Refresh();

                lblSerialNo.Content = mGridContent.Count + 1;

                ScrollGridToLast();
                ClearEditBoxes();

            }
        }

        private void txtCredit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                if (AddDataToGrid())
                {
                    ScrollGridToRow(int.Parse(lblSerialNo.Content.ToString()) - 1);
                    ClearEditBoxes();
                    cmbStockAccount.Focus();
                }

            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            AddDataToDatabase();
        }

        private bool AddDataToDatabase()
        {
            try
            {
                if (cmbCreditAccount.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a Credit Account");
                    cmbCreditAccount.Focus();
                    return false;
                }

                if (grdStocks.Items.Count <= 0)
                {
                    MessageBox.Show("Please Enter Stock Account Entries");
                    cmbStockAccount.Focus();
                    return false;
                }


                try
                {
                    using (ChannelFactory<ITransaction> proxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                        ITransaction service = proxy.CreateChannel(ep);

                        if (mCurrentBillItem == null)
                        {
                            mCurrentBillItem = new CACTransactionParam();
                            mCurrentBillItem.CompanyId = ApplicationStaticVariables.gClientLocalInfo.CompanyId;
                            mCurrentBillItem.AddedUserId = ApplicationStaticVariables.gClientLocalInfo.UserId;
                            mCurrentBillItem.BillType = BillType;
                            mCurrentBillItem.Narration = "";
                            mCurrentBillItem.BillDate = dtpDate.SelectedDate.Value;
            
                            mCurrentBillItem.Items = new List<CACTransactionItem>();
                            CAccount cCAcc = (CAccount)cmbCreditAccount.SelectedItem;
                            
                            double totalCredit = mGridContent.Sum(e => e.Credit);
                            CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.DEPRECIATION_STOCK_CREDIT, SerialNo = 1, AccountId = cCAcc.Id, AccountName = cCAcc.Name, AccountType = cCAcc.AccountType, MainGroup = cCAcc.MainGroup, ParentGroupId = cCAcc.ParentGroupId, Debit = totalCredit, Credit = 0 };
                            mCurrentBillItem.Items.AddRange(mGridContent);
                            mCurrentBillItem.Items.Add(pCred);


                            CTransactionMessage cbm = service.AddBill(mCurrentBillItem, ApplicationStaticVariables.gClientLocalInfo.SessionId);

                            if (!cbm.IsSuccess)
                            {
                                MessageBox.Show(cbm.Message);
                                return false;
                            }
                            else
                            {
                                NewBillForm();
                            }
                        }
                        else
                        {
                            mCurrentBillItem.AddedUserId = ApplicationStaticVariables.gClientLocalInfo.UserId;
                            mCurrentBillItem.Narration = "";
                            mCurrentBillItem.BillDate = dtpDate.SelectedDate.Value;

                            mCurrentBillItem.Items = new List<CACTransactionItem>();
                            CAccount cCAcc = (CAccount)cmbCreditAccount.SelectedItem;
                                                        
                            double totalCredit = mGridContent.Sum(e => e.Credit);
                            CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.DEPRECIATION_STOCK_CREDIT, SerialNo = 1, AccountId = cCAcc.Id, AccountName = cCAcc.Name, AccountType = cCAcc.AccountType, MainGroup = cCAcc.MainGroup, ParentGroupId = cCAcc.ParentGroupId, Debit = totalCredit, Credit = 0 };
                            mCurrentBillItem.Items.AddRange(mGridContent);
                            mCurrentBillItem.Items.Add(pCred);                            
                            
                            CTransactionMessage cbm = service.UpdateBill(mCurrentBillItem, ApplicationStaticVariables.gClientLocalInfo.SessionId);

                            if (!cbm.IsSuccess)
                            {
                                MessageBox.Show(cbm.Message);
                                return false;
                            }
                            else
                            {
                                NewBillForm();
                            }
                        }                        
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Data provided is Wrong, Please check the Data");
                return false;
            }

            return true;
        }

        private bool AddDataToGrid()
        {
            if (cmbStockAccount.SelectedIndex < 0)
            {
                MessageBox.Show("Please select Account");
                cmbStockAccount.Focus();
                return false;
            }

            double amount;
            try
            {
                amount = double.Parse(txtAmount.Text.Trim());
                if (amount <= 0)
                {
                    MessageBox.Show("Please provide Amount greater than 0");
                    txtAmount.Focus();
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Please provide Amount");
                txtAmount.Focus();
                return false;
            }

            try
            {

                CACTransactionItem u = new CACTransactionItem();
                CAccount account = (CAccount)cmbStockAccount.SelectedItem;

                u.AccountId = account.Id;
                u.AccountName = account.Name;
                u.AccountType = account.AccountType;
                u.MainGroup = account.MainGroup;
                u.ParentGroupId = account.ParentGroupId;
                u.SubBillType = CBillSubTypes.DEPRECIATION_CREDIT_STOCK;
                u.SerialNo = Convert.ToInt32(lblSerialNo.Content);
                u.Credit = amount;
                u.Debit = 0;

                if (mCurrentCreditItem != null)
                {
                    int index = grdStocks.SelectedIndex;
                    mGridContent.Remove(grdStocks.SelectedItem as CACTransactionItem);
                    mGridContent.Insert(index, u);
                    grdStocks.Items.Refresh();

                }
                else
                {
                    mGridContent.Add(u);
                    grdStocks.Items.Refresh();

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }


            return true;
        }

        private void txtBillNo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetBillDetails();
                ClearEditBoxes();
                ScrollGridToLast();
            }
        }

        private bool GetBillDetails()
        {

            try
            {
                mGridContent.Clear();

                if (Convert.ToInt32(txtBillNo.Text.Trim()) <= 0)
                {
                    return false;
                }

                try
                {
                    using (ChannelFactory<ITransaction> proxy = new ChannelFactory<ServerServiceInterface.ITransaction>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/TransactionEndPoint");
                        ITransaction service = proxy.CreateChannel(ep);
                        mCurrentBillItem = service.ReadBill(Convert.ToInt32(txtBillNo.Text.Trim()), BillType, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                        dtpDate.SelectedDate = mCurrentBillItem.BillDate;
                        //Stock Entries
                        foreach (var i in mCurrentBillItem.Items.Where(e => e.SubBillType == CBillSubTypes.DEPRECIATION_CREDIT_STOCK).OrderBy(e => e.SerialNo))
                        {
                            mGridContent.Add(new CACTransactionItem { Id = i.Id, SerialNo = i.SerialNo, AccountId = i.AccountId, AccountName = i.AccountName, AccountType = i.AccountType, SubBillType = i.SubBillType, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId, Credit = i.Credit, Debit = i.Debit });
                        }
                        
                        //Credit Account
                        CACTransactionItem pItem = mCurrentBillItem.Items.Where(e => e.MainGroup == CAccount.CREDIT_ACCOUNT && e.Debit > 0).FirstOrDefault();
                        CAccount cAcc = mCAccounts.Where(e => e.Id == pItem.AccountId).FirstOrDefault();
                        cmbCreditAccount.SelectedItem = cAcc;

                        return (mCurrentBillItem != null && mCurrentBillItem.Items.Count > 0);
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cmbCreditAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

                GetCreditAccountsForCombo();
                GetStockAccountsForCombo();
            }
        }

        private void SelectDataToEditBoxes(CACTransactionItem u)
        {
            try
            {
                if (u != null)
                {
                    mCurrentCreditItem = u;
                    lblSerialNo.Content = u.SerialNo;
                    cmbStockAccount.SelectedValue = u.AccountId;
                    txtAmount.Text = u.Credit.ToString("F2");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void ScrollGridToLast()
        {
            if (mGridContent.Count > 0)
            {
                grdStocks.ScrollIntoView(mGridContent.ElementAt(mGridContent.Count - 1));
            }
        }

        private void ScrollGridToRow(int n)
        {
            if (mGridContent.Count > 0)
            {
                grdStocks.ScrollIntoView(mGridContent.ElementAt(n));
            }
        }

        private void grdStocks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACTransactionItem cts = (CACTransactionItem)grdStocks.SelectedItem;
            if (cts != null)
            {
                SelectDataToEditBoxes(cts);
            }
        }

        private void cmbStockAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

                GetCreditAccountsForCombo();
                GetStockAccountsForCombo();
            }
        }

    }
}
