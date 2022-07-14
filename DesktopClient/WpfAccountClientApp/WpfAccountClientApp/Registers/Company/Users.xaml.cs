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
    /// Interaction logic for UserRegister.xaml
    /// </summary>
    public partial class Users : Window
    {
        private ObservableCollection<TreeViewItem> mUserTreeContents = new ObservableCollection<TreeViewItem>();
        private CUser mCurrentUser;

        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public Users()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {         

            NewUserForm();
        }

        private void GetUsersForTree()
        {
            try
            {
                using (ChannelFactory<IUserRegister> userProxy = new ChannelFactory<ServerServiceInterface.IUserRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/UserRegisterEndPoint");
                    IUserRegister userService = userProxy.CreateChannel(ep);
                    List<CUser> users = userService.ReadAllUsers(ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);
                    mUserTreeContents = ConvertCUserToTreeViewItem(users);
                    treeUsers.ItemsSource = mUserTreeContents;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private ObservableCollection<TreeViewItem> ConvertCUserToTreeViewItem(List<CUser> users)
        {        
            ObservableCollection<TreeViewItem> treeContents = new ObservableCollection<TreeViewItem>();
            foreach (var admin in users)
            {
                TreeViewItem aTItem = new TreeViewItem();
                if (admin.UserType == CUser.SUPER_ADMIN)
                {
                    aTItem.Foreground = Brushes.Red;

                    aTItem.Header = admin.Name;
                    aTItem.Tag = admin;
                    treeContents.Add(aTItem);

                    aTItem.MouseLeftButtonUp += TreeUserItem_MouseLeftButtonUp;
                }
                else if(admin.UserType == CUser.USER)
                {                    
                    aTItem.Header = admin.Name;
                    aTItem.Tag = admin;
                    treeContents.Add(aTItem);

                    aTItem.MouseLeftButtonUp += TreeUserItem_MouseLeftButtonUp;
                }                
            }
            return treeContents;
        }
        
        private void NewUserForm()
        {
            GetUsersForTree();
            ClearEditBoxes();
            txtName.Focus();
        }

        private void ClearEditBoxes()
        {
            mCurrentUser = null;
            txtName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";           
        }

        private void SelectDataToEditBoxes(CUser u)
        {
            try
            {
                if (u != null)
                {
                    mCurrentUser = u;

                    txtName.Text = u.Name;
                    txtUsername.Text = u.Username;
                    txtPassword.Text = u.Password;                                        
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void CreateNewUser()
        {
            ClearEditBoxes();
            txtName.Focus();
        }

        private bool DeleteFromDatabase()
        {
            if (mCurrentUser != null)
            {
                try
                {
                    using (ChannelFactory<IUserRegister> userProxy = new ChannelFactory<ServerServiceInterface.IUserRegister>(bhttb))
                    {
                        EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/UserRegisterEndPoint");
                        IUserRegister userService = userProxy.CreateChannel(ep);

                        CBoolMessage cbm = userService.DeleteUser(mCurrentUser.Id, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);


                        if (cbm.IsSuccess)
                        {
                            return true;
                        }else
                        {
                            MessageBox.Show(cbm.Message);
                            return false;
                        }
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;

                }

            }else
            {
                return false;
            }
        }

        private bool SaveDataToDatabase()
        {
            try
            {
                
                if (txtName.Text.Trim().Length <3)
                {
                    MessageBox.Show("Please provide Name having 3 or more letters");
                    txtName.Focus();
                    return false;
                }

                if (txtUsername.Text.Trim().Length < 3 || txtUsername.Text.Trim().Length > 15)
                {
                    MessageBox.Show("Please provide Username having 3 or more letters and maximum 15 letters");
                    txtUsername.Focus();
                    return false;
                }

                if (txtPassword.Text.Trim().Length < 3 || txtPassword.Text.Trim().Length > 20)
                {
                    MessageBox.Show("Please provide Password having 3 or more letters and maximum 20 letters");
                    txtPassword.Focus();
                    return false;
                }

                using (ChannelFactory<IUserRegister> userProxy = new ChannelFactory<ServerServiceInterface.IUserRegister>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/UserRegisterEndPoint");
                    IUserRegister userService = userProxy.CreateChannel(ep);

                    CUser u = new CUser();
                    u.Name = txtName.Text.Trim();
                    u.Username = txtUsername.Text.Trim();
                    u.Password = txtPassword.Text.Trim();
                    u.CompanyId = ApplicationStaticVariables.gClientLocalInfo.CompanyId;                    

                    if (mCurrentUser!=null)
                    {                        
                        u.UserType = mCurrentUser.UserType;
                        u.Id = mCurrentUser.Id;
                    
                        CBoolMessage cbm = userService.UpdateUser(u, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);

                        if (!cbm.IsSuccess)
                        {
                            MessageBox.Show(cbm.Message);
                            return false;
                        }
                    }
                    else
                    {
                        u.UserType = CUser.USER;
                        CBoolMessage cbm = userService.CreateUser(u, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);

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

        private void treeUsers_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectDataToEditBoxes(((sender as TreeView).SelectedItem as TreeViewItem).Tag as CUser);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (mCurrentUser==null)
            {
                MessageBox.Show("Please Select a User");
                return;
            }

            if (MessageBox.Show("Are you sure to Delete the User?", "Delete User", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DeleteFromDatabase())
                {
                    NewUserForm();
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDataToDatabase())
            {
                NewUserForm();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            CreateNewUser();
        }

        private void TreeUserItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectDataToEditBoxes((sender as TreeViewItem).Tag as CUser);
            e.Handled = true;
        }

        private void cmbStatus_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (SaveDataToDatabase())
                {
                    NewUserForm();
                }
            }
        }
    }
}
