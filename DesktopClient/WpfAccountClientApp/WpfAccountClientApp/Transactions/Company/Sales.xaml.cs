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
    /// Interaction logic for Sales.xaml
    /// </summary>
    public partial class Sales : Window
    {
        private static int BillType = CBillTypes.SALES;
        private CACTransactionParam mCurrentBillItem;
        private CACTransactionItem mCurrentCreditItem;
        ObservableCollection<CACTransactionItem> mGridContent = new ObservableCollection<CACTransactionItem>();

        private List<CAccount> mPAccounts = new List<CAccount>();
        private List<CAccount> mSAccounts = new List<CAccount>();
        private List<CAccount> mMAccounts = new List<CAccount>();

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public Sales()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {

            GetPersonalAccountsForCombo();
            GetStockAccountsForCombo();
            GetMonetaryAccountsForCombo();
            NewBillForm();
        }

        private void GetPersonalAccountsForCombo()
        {
            mPAccounts.Clear();
            try
            {
                using (ChannelFactory<IAccountRegister> accountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister accountService = accountProxy.CreateChannel(ep);
                    List<CAccount> accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.PERSONAL_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    mPAccounts = accounts;

                    cmbPersonalAccount.ItemsSource = accounts;
                    cmbPersonalAccount.SelectedValuePath = "Id";
                    cmbPersonalAccount.DisplayMemberPath = "Name";
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

        private void GetMonetaryAccountsForCombo()
        {
            mMAccounts.Clear();
            try
            {
                using (ChannelFactory<IAccountRegister> accountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister accountService = accountProxy.CreateChannel(ep);
                    List<CAccount> accounts = accountService.ReadAllAccountsUnderMainGroup(CAccount.MONETARY_ACCOUNT, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    mMAccounts = accounts;

                    cmbMonetaryAccount.ItemsSource = accounts;
                    cmbMonetaryAccount.SelectedValuePath = "Id";
                    cmbMonetaryAccount.DisplayMemberPath = "Name";
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
            cmbPersonalAccount.SelectedIndex = -1;
            cmbMonetaryAccount.SelectedIndex = -1;
            txtAmountPaid.Text = "";
            txtNarration.Text = "";
            ClearEditBoxes();
            cmbPersonalAccount.Focus();

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
                if (cmbPersonalAccount.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a Personal Account");
                    cmbPersonalAccount.Focus();
                    return false;
                }

                if (mGridContent.Count <= 0)
                {
                    MessageBox.Show("Please Add Stock Entry");
                    cmbStockAccount.Focus();
                    return false;
                }

                double amount=0;
                try
                {
                    amount = Convert.ToDouble(txtAmountPaid.Text.Trim());
                
                }
                catch
                {
                   
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
                            mCurrentBillItem.Narration = txtNarration.Text.Trim();
                            mCurrentBillItem.BillDate = dtpDate.SelectedDate.Value;
            
                            mCurrentBillItem.Items = new List<CACTransactionItem>();
                            CAccount cPAcc = (CAccount)cmbPersonalAccount.SelectedItem;
                                                        
                            double totalDebit=mGridContent.Sum(e => e.Credit);
                            CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.SALES_PERSONAL_STOCK, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = totalDebit, Credit = 0 };
                            mCurrentBillItem.Items.AddRange(mGridContent);
                            mCurrentBillItem.Items.Add(pCred);

                            CAccount cMAcc = (CAccount)cmbMonetaryAccount.SelectedItem;
                            if (cMAcc != null && amount > 0)
                            {
                                CACTransactionItem pPay = new CACTransactionItem { SubBillType = CBillSubTypes.SALES_PERSONAL_MONETARY, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = 0, Credit = amount };
                                CACTransactionItem mPay = new CACTransactionItem { SubBillType = CBillSubTypes.SALES_MONETARY_PERSONAL, SerialNo = 1, AccountId = cMAcc.Id, AccountName = cMAcc.Name, AccountType = cMAcc.AccountType, MainGroup = cMAcc.MainGroup, ParentGroupId = cMAcc.ParentGroupId, Debit = amount, Credit = 0 };
                                mCurrentBillItem.Items.Add(pPay);
                                mCurrentBillItem.Items.Add(mPay);
                            }


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
                            mCurrentBillItem.Narration = txtNarration.Text.Trim();
                            mCurrentBillItem.BillDate = dtpDate.SelectedDate.Value;

                            mCurrentBillItem.Items = new List<CACTransactionItem>();
                            CAccount cPAcc = (CAccount)cmbPersonalAccount.SelectedItem;
                            
                            double totalDebit = mGridContent.Sum(e => e.Credit);
                            CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.SALES_PERSONAL_STOCK, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = totalDebit, Credit = 0 };
                            mCurrentBillItem.Items.AddRange(mGridContent);
                            mCurrentBillItem.Items.Add(pCred);
                            
                            CAccount cMAcc = (CAccount)cmbMonetaryAccount.SelectedItem;
                            if (cMAcc != null && amount > 0)
                            {
                                CACTransactionItem pPay = new CACTransactionItem { SubBillType = CBillSubTypes.SALES_PERSONAL_MONETARY, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = 0, Credit = amount };
                                CACTransactionItem mPay = new CACTransactionItem { SubBillType = CBillSubTypes.SALES_MONETARY_PERSONAL, SerialNo = 1, AccountId = cMAcc.Id, AccountName = cMAcc.Name, AccountType = cMAcc.AccountType, MainGroup = cMAcc.MainGroup, ParentGroupId = cMAcc.ParentGroupId, Debit = amount, Credit = 0 };
                                mCurrentBillItem.Items.Add(pPay);
                                mCurrentBillItem.Items.Add(mPay);
                            }

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

            double credit;
            try
            {
                credit = double.Parse(txtAmount.Text.Trim());
                if (credit <= 0)
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
                u.SubBillType = CBillSubTypes.SALES_STOCK_PERSONAL;
                u.SerialNo = Convert.ToInt32(lblSerialNo.Content);
                u.Credit = credit;
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
                        foreach (var i in mCurrentBillItem.Items.Where(e => e.SubBillType == CBillSubTypes.SALES_STOCK_PERSONAL).OrderBy(e => e.SerialNo))
                        {
                            mGridContent.Add(new CACTransactionItem { Id = i.Id, SerialNo = i.SerialNo, AccountId = i.AccountId, AccountName = i.AccountName, AccountType = i.AccountType, SubBillType = i.SubBillType, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId, Credit = i.Credit, Debit = i.Debit });                        }                       

                        //Personal Account
                        CACTransactionItem pItem = mCurrentBillItem.Items.Where(e => e.MainGroup == CAccount.PERSONAL_ACCOUNT && e.Debit > 0).FirstOrDefault();
                        CAccount pAcc = mPAccounts.Where(e => e.Id == pItem.AccountId).FirstOrDefault();
                        cmbPersonalAccount.SelectedItem = pAcc;

                        //Monetary Account
                        pItem = mCurrentBillItem.Items.Where(e => e.MainGroup == CAccount.MONETARY_ACCOUNT && e.Debit > 0).FirstOrDefault();
                        expMoney.IsExpanded = pItem!=null;
                        if (pItem != null)
                        {                            
                            CAccount mAcc = mMAccounts.Where(e => e.Id == pItem.AccountId).FirstOrDefault();
                            cmbMonetaryAccount.SelectedItem = mAcc;
                            txtAmountPaid.Text = pItem.Debit.ToString();
                        }
                        txtNarration.Text = mCurrentBillItem.Narration;

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

        private void cmbPersonalAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

                GetPersonalAccountsForCombo();
                GetStockAccountsForCombo();
                GetMonetaryAccountsForCombo();
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

                GetPersonalAccountsForCombo();
                GetStockAccountsForCombo();
                GetMonetaryAccountsForCombo();
            }
        }

        private void cmbMonetaryAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

                GetPersonalAccountsForCombo();
                GetStockAccountsForCombo();
                GetMonetaryAccountsForCombo();
            }
        }
    }
}
