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
    /// Interaction logic for Receivable.xaml
    /// </summary>
    public partial class Receivable : Window
    {
        private static int BillType = CBillTypes.CREDIT_RECEIPT;
        private CACTransactionParam mCurrentBillItem;
        private CACTransactionItem mCurrentCreditItem;        
        
        private List<CAccount> mCAccounts = new List<CAccount>();
        private List<CAccount> mMAccounts = new List<CAccount>();

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public Receivable()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {            
            GetCreditAccountsForCombo();
            GetMonetaryAccountsForCombo();
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
            txtBillNo.Text = "";
            cmbCreditAccount.SelectedIndex = -1;
            cmbMonetaryAccount.SelectedIndex = -1;
            txtAmountReceived.Text = "";
            txtNarration.Text = "";            
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
                            CAccount cCAcc = (CAccount)cmbCreditAccount.SelectedItem;
                            CAccount cMAcc = (CAccount)cmbMonetaryAccount.SelectedItem;                            

                            CACTransactionItem cPay = new CACTransactionItem { SubBillType = CBillSubTypes.CREDIT_RECEIPT_CREDIT_MONETARY, SerialNo = 1, AccountId = cCAcc.Id, AccountName = cCAcc.Name, AccountType = cCAcc.AccountType, MainGroup = cCAcc.MainGroup, ParentGroupId = cCAcc.ParentGroupId, Debit = 0, Credit = amount };
                            CACTransactionItem mPay = new CACTransactionItem { SubBillType = CBillSubTypes.CREDIT_RECEIPT_MONETARY_CREDIT, SerialNo = 1, AccountId = cMAcc.Id, AccountName = cMAcc.Name, AccountType = cMAcc.AccountType, MainGroup = cMAcc.MainGroup, ParentGroupId = cMAcc.ParentGroupId, Debit = amount, Credit = 0 };
                            mCurrentBillItem.Items.Add(cPay);
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
                            CAccount cCAcc = (CAccount)cmbCreditAccount.SelectedItem;
                            CAccount cMAcc = (CAccount)cmbMonetaryAccount.SelectedItem;

                            CACTransactionItem cPay = new CACTransactionItem { SubBillType = CBillSubTypes.CREDIT_RECEIPT_CREDIT_MONETARY, SerialNo = 1, AccountId = cCAcc.Id, AccountName = cCAcc.Name, AccountType = cCAcc.AccountType, MainGroup = cCAcc.MainGroup, ParentGroupId = cCAcc.ParentGroupId, Debit = 0, Credit = amount };
                            CACTransactionItem mPay = new CACTransactionItem { SubBillType = CBillSubTypes.CREDIT_RECEIPT_MONETARY_CREDIT, SerialNo = 1, AccountId = cMAcc.Id, AccountName = cMAcc.Name, AccountType = cMAcc.AccountType, MainGroup = cMAcc.MainGroup, ParentGroupId = cMAcc.ParentGroupId, Debit = amount, Credit = 0 };
                            mCurrentBillItem.Items.Add(cPay);
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

        private void txtBillNo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetBillDetails();
            }
        }

        private bool GetBillDetails()
        {

            try
            {                

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
        
                        //Personal Account
                        CACTransactionItem pItem = mCurrentBillItem.Items.Where(e => e.MainGroup == CAccount.CREDIT_ACCOUNT && e.Credit > 0).FirstOrDefault();
                        CAccount cAcc = mCAccounts.Where(e => e.Id == pItem.AccountId).FirstOrDefault();
                        cmbCreditAccount.SelectedItem = cAcc;

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

        private void cmbCreditAccount_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                Accounts a = new Accounts();
                a.ShowDialog();

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

                GetCreditAccountsForCombo();
                GetMonetaryAccountsForCombo();
            }
        }
    }
}
