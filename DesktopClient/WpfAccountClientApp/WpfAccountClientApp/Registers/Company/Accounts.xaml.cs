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
    /// Interaction logic for AccountRegister.xaml
    /// </summary>
    public partial class Accounts : Window
    {
        private ObservableCollection<TreeViewItem> mAccountsTreeContents = new ObservableCollection<TreeViewItem>();
        private CAccount mCurrentAccount;
        private ObservableCollection<CAccount> mMonetaryParentGroups = new ObservableCollection<CAccount>();
        private ObservableCollection<CAccount> mPersonalParentGroups = new ObservableCollection<CAccount>();        
        private ObservableCollection<CAccount> mCreditParentGroups = new ObservableCollection<CAccount>();

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };


        public Accounts()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {
            cmbAccountType.Items.Add("Group");
            cmbAccountType.Items.Add("Account");

            cmbMainGroup.Items.Add("Monetary Accounts");
            cmbMainGroup.Items.Add("Personal Accounts");            
            cmbMainGroup.Items.Add("Credit Accounts");

            NewAccountForm();
        }

        private void GetAccountsForTree()
        {
            try
            {
                using (ChannelFactory<IAccountRegister> AccountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister AccountService = AccountProxy.CreateChannel(ep);
                    List<CAccount> Accounts = AccountService.ReadAllAccounts(ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    mAccountsTreeContents = ConvertCAccountToTreeViewItem(Accounts);
                    treeAccounts.ItemsSource = mAccountsTreeContents;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private ObservableCollection<TreeViewItem> ConvertCAccountToTreeViewItem(List<CAccount> Accounts)
        {
            
            ObservableCollection<TreeViewItem> treeContents = new ObservableCollection<TreeViewItem>();
            TreeViewItem monetaryGroup = new TreeViewItem();
            TreeViewItem personalGroup = new TreeViewItem();            
            TreeViewItem creditGroup = new TreeViewItem();
            monetaryGroup.Header = "Monetary Accounts (Company)";
            monetaryGroup.Foreground = Brushes.DarkBlue;
            personalGroup.Header = "Personal Accounts";
            personalGroup.Foreground = Brushes.DarkBlue;
            creditGroup.Header = "Credit Accounts";
            creditGroup.Foreground = Brushes.DarkBlue;
            mMonetaryParentGroups.Clear();
            mPersonalParentGroups.Clear();            
            mCreditParentGroups.Clear();
            //Monetary Account
            foreach (var groupAccount in Accounts.Where(e => e.AccountType == CAccount.GROUP && e.MainGroup == CAccount.MONETARY_ACCOUNT).OrderBy(e => e.Name))
            {
                TreeViewItem aTItem = new TreeViewItem();
                foreach (var subDealer in Accounts.Where(e => e.AccountType == CAccount.ACCOUNT && e.ParentGroupId == groupAccount.Id).OrderBy(e => e.Name))
                {
                    TreeViewItem bTItem = new TreeViewItem();


                    bTItem.Header = subDealer.Name;
                    bTItem.Foreground = Brushes.Blue;
                    bTItem.Tag = subDealer;
                    aTItem.Items.Add(bTItem);

                    bTItem.MouseLeftButtonUp += TreeAccountItem_MouseLeftButtonUp;
                }

                aTItem.Header = groupAccount.Name;
                aTItem.Foreground = Brushes.DarkGreen;
                aTItem.Tag = groupAccount;
                monetaryGroup.Items.Add(aTItem);

                aTItem.MouseLeftButtonUp += TreeAccountItem_MouseLeftButtonUp;

                mMonetaryParentGroups.Add(groupAccount);
            }
            //Personal Account
            foreach (var groupAccount in Accounts.Where(e => e.AccountType == CAccount.GROUP && e.MainGroup == CAccount.PERSONAL_ACCOUNT).OrderBy(e => e.Name))
            {
                TreeViewItem aTItem = new TreeViewItem();
                foreach (var subDealer in Accounts.Where(e => e.AccountType == CAccount.ACCOUNT && e.ParentGroupId == groupAccount.Id).OrderBy(e => e.Name))
                {
                    TreeViewItem bTItem = new TreeViewItem();


                    bTItem.Header = subDealer.Name;
                    bTItem.Foreground = Brushes.Blue;
                    bTItem.Tag = subDealer;
                    aTItem.Items.Add(bTItem);

                    bTItem.MouseLeftButtonUp += TreeAccountItem_MouseLeftButtonUp;
                }

                aTItem.Header = groupAccount.Name;
                aTItem.Foreground = Brushes.DarkGreen;
                aTItem.Tag = groupAccount;
                personalGroup.Items.Add(aTItem);

                aTItem.MouseLeftButtonUp += TreeAccountItem_MouseLeftButtonUp;

                mPersonalParentGroups.Add(groupAccount);
            }
            
            //Credit Account
            foreach (var groupAccount in Accounts.Where(e => e.AccountType == CAccount.GROUP && e.MainGroup == CAccount.CREDIT_ACCOUNT).OrderBy(e => e.Name))
            {
                TreeViewItem aTItem = new TreeViewItem();
                foreach (var subDealer in Accounts.Where(e => e.AccountType == CAccount.ACCOUNT && e.ParentGroupId == groupAccount.Id).OrderBy(e => e.Name))
                {
                    TreeViewItem bTItem = new TreeViewItem();


                    bTItem.Header = subDealer.Name;
                    bTItem.Foreground = Brushes.Blue;
                    bTItem.Tag = subDealer;
                    aTItem.Items.Add(bTItem);

                    bTItem.MouseLeftButtonUp += TreeAccountItem_MouseLeftButtonUp;
                }

                aTItem.Header = groupAccount.Name;
                aTItem.Foreground = Brushes.DarkGreen;
                aTItem.Tag = groupAccount;
                creditGroup.Items.Add(aTItem);

                aTItem.MouseLeftButtonUp += TreeAccountItem_MouseLeftButtonUp;

                mCreditParentGroups.Add(groupAccount);
            }
            treeContents.Add(monetaryGroup);
            treeContents.Add(personalGroup);            
            treeContents.Add(creditGroup);

            cmbParentGroup.SelectedIndex = -1;

            return treeContents;
        }

        private void NewAccountForm()
        {
            GetAccountsForTree();
            ClearEditBoxes();
            cmbAccountType.Focus();
        }

        private void ClearEditBoxes()
        {
            mCurrentAccount = null;
            cmbAccountType.SelectedIndex = -1;
            cmbMainGroup.SelectedIndex = -1;
            cmbParentGroup.SelectedIndex = -1;
            txtName.Text = "";
            txtShortName.Text = "";
            txtDetails1.Text = "";
            txtDetails2.Text = "";
            txtDetails3.Text = "";
        }

        private void SelectDataToEditBoxes(CAccount u)
        {
            try
            {
                if (u != null)
                {
                    mCurrentAccount = u;

                    cmbAccountType.SelectedIndex = u.AccountType;
                    cmbMainGroup.SelectedIndex = u.MainGroup;
                    
                    txtName.Text = u.Name;
                    txtShortName.Text = u.ShortName;
                    txtDetails1.Text = u.Details1;
                    txtDetails2.Text = u.Details2;
                    txtDetails3.Text = u.Details3;
                    
                    if (u.AccountType == CAccount.GROUP)
                    {
                        cmbParentGroup.SelectedIndex = -1;
                    }
                    else
                    {
                        cmbParentGroup.SelectedValue = u.ParentGroupId;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void CreateNewAccount()
        {
            ClearEditBoxes();
            cmbAccountType.Focus();
        }

        private bool DeleteFromDatabase()
        {
            if (mCurrentAccount != null)
            {
                try
                {
                    using (ChannelFactory<IAccountRegister> AccountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                        IAccountRegister AccountService = AccountProxy.CreateChannel(ep);

                        CBoolMessage cbm = AccountService.DeleteAccount(mCurrentAccount.Id, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);


                        if (cbm.IsSuccess)
                        {
                            return true;
                        } else
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

            } else
            {
                return false;
            }
        }

        private bool SaveDataToDatabase()
        {
            try
            {
                if (cmbAccountType.SelectedIndex == -1)
                {
                    MessageBox.Show("Please Select Account Type");
                    cmbAccountType.Focus();
                    return false;
                }

                if (cmbMainGroup.SelectedIndex == -1)
                {
                    MessageBox.Show("Please Select Main Group");
                    cmbMainGroup.Focus();
                    return false;
                }

                if (cmbAccountType.SelectedIndex == 1 && cmbParentGroup.SelectedIndex == -1)
                {
                    MessageBox.Show("Please Select Parent Group");
                    cmbParentGroup.Focus();
                    return false;
                }

                if (txtName.Text.Trim().Length < 3)
                {
                    MessageBox.Show("Please provide Name having 3 or more letters");
                    txtName.Focus();
                    return false;
                }

                if (txtShortName.Text.Trim().Length < 3 || txtShortName.Text.Trim().Length > 10)
                {
                    MessageBox.Show("Please provide Short Name having 3 or more letters and maximum 10 letters");
                    txtShortName.Focus();
                    return false;
                }

                using (ChannelFactory<IAccountRegister> AccountProxy = new ChannelFactory<ServerServiceInterface.IAccountRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/AccountRegisterEndPoint");
                    IAccountRegister AccountService = AccountProxy.CreateChannel(ep);

                    CAccount u = new CAccount();
                    u.Name = txtName.Text.Trim();
                    u.ShortName = txtShortName.Text.Trim();
                    u.Details1 = txtDetails1.Text.Trim();
                    u.Details2 = txtDetails2.Text.Trim();
                    u.Details3 = txtDetails3.Text.Trim();
                    u.CompanyId = ApplicationStaticVariables.gClientLocalInfo.CompanyId;

                    if (mCurrentAccount != null)
                    {
                        u.Id = mCurrentAccount.Id;
                        u.AccountType = mCurrentAccount.AccountType;
                        u.ParentGroupId = mCurrentAccount.ParentGroupId;
                        u.MainGroup = mCurrentAccount.MainGroup;

                        CBoolMessage cbm = AccountService.UpdateAccount(u, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);

                        if (!cbm.IsSuccess)
                        {
                            MessageBox.Show(cbm.Message);
                            return false;
                        }
                    }
                    else
                    {
                        u.AccountType = cmbAccountType.SelectedIndex;
                        if (cmbAccountType.SelectedIndex == CAccount.ACCOUNT)
                        {
                            u.ParentGroupId = ((CAccount)cmbParentGroup.SelectedItem).Id;
                        }
                        else
                        {
                            u.ParentGroupId = -1;
                        }
                        u.MainGroup = cmbMainGroup.SelectedIndex;
                        CBoolMessage cbm = AccountService.CreateAccount(u, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);

                        if (!cbm.IsSuccess)
                        {
                            MessageBox.Show(cbm.Message);
                            return false;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }

    

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void treeAccounts_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectDataToEditBoxes(((sender as TreeView).SelectedItem as TreeViewItem).Tag as CAccount);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (mCurrentAccount==null)
            {
                MessageBox.Show("Please Select a Account");
                return;
            }

            if (MessageBox.Show("Are you sure to Delete the Account?", "Delete Account", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DeleteFromDatabase())
                {
                    NewAccountForm();
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDataToDatabase())
            {
                NewAccountForm();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            CreateNewAccount();
        }

        private void TreeAccountItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectDataToEditBoxes((sender as TreeViewItem).Tag as CAccount);
            e.Handled = true;
        }

        private void cmbStatus_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (SaveDataToDatabase())
                {
                    NewAccountForm();
                }
            }
        }

        private void cmbMainGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMainGroup.SelectedIndex > -1)
            {
                cmbParentGroup.ItemsSource=null;
                cmbParentGroup.DisplayMemberPath = "Name";
                cmbParentGroup.SelectedValuePath = "Id";

                if (cmbMainGroup.SelectedIndex == CAccount.MONETARY_ACCOUNT)
                {
                    cmbParentGroup.ItemsSource = mMonetaryParentGroups;
                }
                else if (cmbMainGroup.SelectedIndex == CAccount.PERSONAL_ACCOUNT)
                {
                    cmbParentGroup.ItemsSource = mPersonalParentGroups;
                }
                else if (cmbMainGroup.SelectedIndex == CAccount.CREDIT_ACCOUNT)
                {
                    cmbParentGroup.ItemsSource = mCreditParentGroups;
                }
            }
        }
    }
}
