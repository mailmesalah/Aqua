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
    /// Interaction logic for Receipt.xaml
    /// </summary>
    public partial class Receipt : Window
    {
        private static int BillType = CBillTypes.PERSONAL_RECEIPT;
        private CACTransactionParam mCurrentBillItem;
        private CACTransactionItem mCurrentCreditItem;
        ObservableCollection<CACTransactionItem> mCreditGridContent = new ObservableCollection<CACTransactionItem>();

        private List<CAccount> mPAccounts = new List<CAccount>();
        private List<CAccount> mCAccounts = new List<CAccount>();
        private List<CAccount> mMAccounts = new List<CAccount>();

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public Receipt()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {

            GetPersonalAccountsForCombo();
            GetCreditAccountsForCombo();
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
            mCreditGridContent.Clear();
            grdCredits.ItemsSource = mCreditGridContent;
            txtBillNo.Text = "";
            cmbPersonalAccount.SelectedIndex = -1;
            cmbMonetaryAccount.SelectedIndex = -1;
            txtAmountReceived.Text = "";
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
            cmbCreditAccount.SelectedIndex = -1;
            txtCredit.Text = "";
            lblSerialNo.Content = mCreditGridContent.Count + 1;
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
                mCreditGridContent.Remove(cts);
                for (int i = 0; i < mCreditGridContent.Count; i++)
                {
                    mCreditGridContent.ElementAt(i).SerialNo = mCreditGridContent.ElementAt(i).SerialNo > cts.SerialNo ? --mCreditGridContent.ElementAt(i).SerialNo : mCreditGridContent.ElementAt(i).SerialNo;
                }
                grdCredits.Items.Refresh();

                lblSerialNo.Content = mCreditGridContent.Count + 1;

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
                    cmbCreditAccount.Focus();
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

                if (cmbMonetaryAccount.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a Monetary Account");
                    cmbMonetaryAccount.Focus();
                    return false;
                }

                double amount;
                try
                {
                    amount = Convert.ToDouble(txtAmountReceived.Text.Trim());
                    if ( amount<= 0)
                    {
                        MessageBox.Show("Please provide Amount Received");
                        txtAmountReceived.Focus();
                        return false;
                    }
                }
                catch
                {
                    MessageBox.Show("Please provide Amount Received");
                    txtAmountReceived.Focus();
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
                            mCurrentBillItem.Narration = txtNarration.Text.Trim();
                            mCurrentBillItem.BillDate = dtpDate.SelectedDate.Value;
            
                            mCurrentBillItem.Items = new List<CACTransactionItem>();
                            CAccount cPAcc = (CAccount)cmbPersonalAccount.SelectedItem;
                            CAccount cMAcc = (CAccount)cmbMonetaryAccount.SelectedItem;

                            if (mCreditGridContent!=null && mCreditGridContent.Count > 0)
                            {
                                double totalCredit=mCreditGridContent.Sum(e => e.Credit);
                                CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.PERSONAL_RECEIPT_PERSONAL_CREDIT, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = totalCredit, Credit = 0 };
                                mCurrentBillItem.Items.AddRange(mCreditGridContent);
                                mCurrentBillItem.Items.Add(pCred);
                            }

                            CACTransactionItem pPay = new CACTransactionItem { SubBillType = CBillSubTypes.PERSONAL_RECEIPT_PERSONAL_MONETARY, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = 0, Credit = amount };
                            CACTransactionItem mPay = new CACTransactionItem { SubBillType = CBillSubTypes.PERSONAL_RECEIPT_MONETARY_PERSONAL, SerialNo = 1, AccountId = cMAcc.Id, AccountName = cMAcc.Name, AccountType = cMAcc.AccountType, MainGroup = cMAcc.MainGroup, ParentGroupId = cMAcc.ParentGroupId, Debit = amount, Credit = 0 };
                            mCurrentBillItem.Items.Add(pPay);
                            mCurrentBillItem.Items.Add(mPay);


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
                            CAccount cMAcc = (CAccount)cmbMonetaryAccount.SelectedItem;

                            if (mCreditGridContent != null && mCreditGridContent.Count > 0)
                            {
                                double totalDebit = mCreditGridContent.Sum(e => e.Debit);
                                CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.PERSONAL_RECEIPT_PERSONAL_CREDIT, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = totalDebit, Credit = 0 };
                                mCurrentBillItem.Items.AddRange(mCreditGridContent);
                                mCurrentBillItem.Items.Add(pCred);
                            }

                            CACTransactionItem pPay = new CACTransactionItem { SubBillType = CBillSubTypes.PERSONAL_RECEIPT_PERSONAL_MONETARY, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = 0, Credit = amount };
                            CACTransactionItem mPay = new CACTransactionItem { SubBillType = CBillSubTypes.PERSONAL_RECEIPT_MONETARY_PERSONAL, SerialNo = 1, AccountId = cMAcc.Id, AccountName = cMAcc.Name, AccountType = cMAcc.AccountType, MainGroup = cMAcc.MainGroup, ParentGroupId = cMAcc.ParentGroupId, Debit = amount, Credit = 0 };
                            mCurrentBillItem.Items.Add(pPay);
                            mCurrentBillItem.Items.Add(mPay);

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
            if (cmbCreditAccount.SelectedIndex < 0)
            {
                MessageBox.Show("Please select Account");
                cmbCreditAccount.Focus();
                return false;
            }

            double credit;
            try
            {
                credit = double.Parse(txtCredit.Text.Trim());
                if (credit <= 0)
                {
                    MessageBox.Show("Please provide Amount greater than 0");
                    txtCredit.Focus();
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Please provide Amount");
                txtCredit.Focus();
                return false;
            }

            try
            {

                CACTransactionItem u = new CACTransactionItem();
                CAccount account = (CAccount)cmbCreditAccount.SelectedItem;

                u.AccountId = account.Id;
                u.AccountName = account.Name;
                u.AccountType = account.AccountType;
                u.MainGroup = account.MainGroup;
                u.ParentGroupId = account.ParentGroupId;
                u.SubBillType = CBillSubTypes.PERSONAL_RECEIPT_CREDIT_PERSONAL;
                u.SerialNo = Convert.ToInt32(lblSerialNo.Content);
                u.Credit = credit;
                u.Debit = 0;

                if (mCurrentCreditItem != null)
                {
                    int index = grdCredits.SelectedIndex;
                    mCreditGridContent.Remove(grdCredits.SelectedItem as CACTransactionItem);
                    mCreditGridContent.Insert(index, u);
                    grdCredits.Items.Refresh();

                }
                else
                {
                    mCreditGridContent.Add(u);
                    grdCredits.Items.Refresh();

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
                mCreditGridContent.Clear();

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
                        //Credit Entries
                        foreach (var i in mCurrentBillItem.Items.Where(e => e.SubBillType == CBillSubTypes.PERSONAL_RECEIPT_CREDIT_PERSONAL).OrderBy(e => e.SerialNo))
                        {
                            mCreditGridContent.Add(new CACTransactionItem { Id = i.Id, SerialNo = i.SerialNo, AccountId = i.AccountId, AccountName = i.AccountName, AccountType = i.AccountType, SubBillType = i.SubBillType, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId, Credit = i.Credit, Debit = i.Debit });
                        }

                        expCredit.IsExpanded = mCreditGridContent.Count > 0;

                        //Personal Account
                        CACTransactionItem pItem = mCurrentBillItem.Items.Where(e => e.MainGroup == CAccount.PERSONAL_ACCOUNT && e.Credit > 0).FirstOrDefault();
                        CAccount pAcc = mPAccounts.Where(e => e.Id == pItem.AccountId).FirstOrDefault();
                        cmbPersonalAccount.SelectedItem = pAcc;

                        //Monetary Account
                        pItem = mCurrentBillItem.Items.Where(e => e.MainGroup == CAccount.MONETARY_ACCOUNT && e.Debit > 0).FirstOrDefault();
                        CAccount mAcc = mMAccounts.Where(e => e.Id == pItem.AccountId).FirstOrDefault();
                        cmbMonetaryAccount.SelectedItem = mAcc;
                        txtAmountReceived.Text = pItem.Debit.ToString();
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
                GetCreditAccountsForCombo();
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
                    cmbCreditAccount.SelectedValue = u.AccountId;
                    txtCredit.Text = u.Credit.ToString("F2");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void ScrollGridToLast()
        {
            if (mCreditGridContent.Count > 0)
            {
                grdCredits.ScrollIntoView(mCreditGridContent.ElementAt(mCreditGridContent.Count - 1));
            }
        }

        private void ScrollGridToRow(int n)
        {
            if (mCreditGridContent.Count > 0)
            {
                grdCredits.ScrollIntoView(mCreditGridContent.ElementAt(n));
            }
        }

        private void grdCredits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACTransactionItem cts = (CACTransactionItem)grdCredits.SelectedItem;
            if (cts != null)
            {
                SelectDataToEditBoxes(cts);
            }
        }

        private void cmbCreditAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

                GetPersonalAccountsForCombo();
                GetCreditAccountsForCombo();
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
                GetCreditAccountsForCombo();
                GetMonetaryAccountsForCombo();
            }
        }
    }
}
