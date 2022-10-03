﻿using AquaClient.General;
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
    /// Interaction logic for AccountTransfer.xaml
    /// </summary>
    public partial class AccountTransfer : Window
    {
        private static int BillType = CBillTypes.ACCOUNT_TRANSFER;
        private CACTransactionParam mCurrentBillItem;
        private CACTransactionItem mCurrentCreditItem;
        ObservableCollection<CACTransactionItem> mCreditGridContent = new ObservableCollection<CACTransactionItem>();

        private List<CAccount> mPAccounts = new List<CAccount>();
        //private List<CAccount> mCAccounts = new List<CAccount>();


        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public AccountTransfer()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {

            GetPersonalAccountsForCombo();           
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

                    cmbFromPersonalAccount.ItemsSource = accounts;
                    cmbFromPersonalAccount.SelectedValuePath = "Id";
                    cmbFromPersonalAccount.DisplayMemberPath = "Name";

                    cmbToPersonalAccount.ItemsSource = accounts;
                    cmbToPersonalAccount.SelectedValuePath = "Id";
                    cmbToPersonalAccount.DisplayMemberPath = "Name";
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
            cmbFromPersonalAccount.SelectedIndex = -1;
            ClearEditBoxes();
            cmbFromPersonalAccount.Focus();

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
            cmbToPersonalAccount.SelectedIndex = -1;
            txtDebit.Text = "";
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

        private void txtDebit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                if (AddDataToGrid())
                {
                    ScrollGridToRow(int.Parse(lblSerialNo.Content.ToString()) - 1);
                    ClearEditBoxes();
                    cmbToPersonalAccount.Focus();
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
                if (cmbFromPersonalAccount.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a 'From' Account");
                    cmbFromPersonalAccount.Focus();
                    return false;
                }

                if (grdCredits.Items.Count <= 0)
                {
                    MessageBox.Show("Please Enter 'To' Account Entries");
                    cmbToPersonalAccount.Focus();
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
                            CAccount cPAcc = (CAccount)cmbFromPersonalAccount.SelectedItem;
                            
                            double totalDebit = mCreditGridContent.Sum(e => e.Debit);
                            CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.ACCOUNT_TRANSFER_CREDIT_PERSONAL, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = 0, Credit = totalDebit };
                            mCurrentBillItem.Items.AddRange(mCreditGridContent);
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
                            CAccount cPAcc = (CAccount)cmbFromPersonalAccount.SelectedItem;
                                                   
                            double totalDebit = mCreditGridContent.Sum(e => e.Debit);
                            CACTransactionItem pCred = new CACTransactionItem { SubBillType = CBillSubTypes.ACCOUNT_TRANSFER_CREDIT_PERSONAL, SerialNo = 1, AccountId = cPAcc.Id, AccountName = cPAcc.Name, AccountType = cPAcc.AccountType, MainGroup = cPAcc.MainGroup, ParentGroupId = cPAcc.ParentGroupId, Debit = 0, Credit = 0 };
                            mCurrentBillItem.Items.AddRange(mCreditGridContent);
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
            if (cmbToPersonalAccount.SelectedIndex < 0)
            {
                MessageBox.Show("Please select Account");
                cmbToPersonalAccount.Focus();
                return false;
            }

            double debit;
            try
            {
                debit = double.Parse(txtDebit.Text.Trim());
                if (debit <= 0)
                {
                    MessageBox.Show("Please provide Amount greater than 0");
                    txtDebit.Focus();
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Please provide Amount");
                txtDebit.Focus();
                return false;
            }

            try
            {

                CACTransactionItem u = new CACTransactionItem();
                CAccount account = (CAccount)cmbToPersonalAccount.SelectedItem;

                u.AccountId = account.Id;
                u.AccountName = account.Name;
                u.AccountType = account.AccountType;
                u.MainGroup = account.MainGroup;
                u.ParentGroupId = account.ParentGroupId;
                u.SubBillType = CBillSubTypes.ACCOUNT_TRANSFER_DEBIT_PERSONAL;
                u.SerialNo = Convert.ToInt32(lblSerialNo.Content);
                u.Credit = 0;
                u.Debit = debit;

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
                        //To Account Entries
                        foreach (var i in mCurrentBillItem.Items.Where(e => e.SubBillType == CBillSubTypes.ACCOUNT_TRANSFER_DEBIT_PERSONAL).OrderBy(e => e.SerialNo))
                        {
                            mCreditGridContent.Add(new CACTransactionItem { Id = i.Id, SerialNo = i.SerialNo, AccountId = i.AccountId, AccountName = i.AccountName, AccountType = i.AccountType, SubBillType = i.SubBillType, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId, Credit = i.Credit, Debit = i.Debit });
                        }
                        
                        //From Account
                        CACTransactionItem pItem = mCurrentBillItem.Items.Where(e => e.MainGroup == CAccount.PERSONAL_ACCOUNT && e.Credit > 0).FirstOrDefault();
                        CAccount pAcc = mPAccounts.Where(e => e.Id == pItem.AccountId).FirstOrDefault();
                        cmbFromPersonalAccount.SelectedItem = pAcc;

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

        private void cmbFromPersonalAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

                GetPersonalAccountsForCombo();                
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
                    cmbToPersonalAccount.SelectedValue = u.AccountId;
                    txtDebit.Text = u.Debit.ToString("F2");
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

        private void cmbToPersonalAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

                GetPersonalAccountsForCombo();
                
            }
        }
    }
}
