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
using AquaServer.Local;
using AquaServer.StorageModel;

namespace AquaServer.UI
{
    /// <summary>
    /// Interaction logic for UserRegister.xaml
    /// </summary>
    public partial class Company : Window
    {
        private ObservableCollection<TreeViewItem> mTreeContents = new ObservableCollection<TreeViewItem>();
        private CCompany mCurrentCompany;
        CompanyBackend mCompanyBackend = new CompanyBackend();

        public Company()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {

            cmbStatus.Items.Add("Active");
            cmbStatus.Items.Add("Deactive");

            NewForm();
        }

        private void GetCompaniesForTree()
        {
            try
            {
                List<CCompany> companies = mCompanyBackend.ReadAllCompanies();
                mTreeContents = ConvertCUserToTreeViewItem(companies);
                tree.ItemsSource = mTreeContents;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private ObservableCollection<TreeViewItem> ConvertCUserToTreeViewItem(List<CCompany> companies)
        {
            ObservableCollection<TreeViewItem> treeContents = new ObservableCollection<TreeViewItem>();
            try
            {
                foreach (var i in companies)
                {
                    TreeViewItem aTItem = new TreeViewItem();
                    aTItem.Header = i.Company;
                    aTItem.Tag = i;
                    treeContents.Add(aTItem);

                    aTItem.MouseLeftButtonUp += TreeItem_MouseLeftButtonUp;
                }
            }
            catch { }
            return treeContents;
        }

        private void NewForm()
        {
            GetCompaniesForTree();
            ClearEditBoxes();
            txtCompany.Focus();
        }

        private void ClearEditBoxes()
        {
            mCurrentCompany = null;
            txtCompany.Text = "";
            txtCompanyUsername.Text = "";
            txtAdminName.Text = "";
            txtAdminUsername.Text = "";
            txtAdminPassword.Text = "";
            cmbStatus.SelectedIndex = -1;
            dtpDate.SelectedDate = DateTime.Now;
        }

        private void SelectDataToEditBoxes(CCompany c)
        {
            try
            {
                if (c != null)
                {
                    mCurrentCompany = c;

                    txtCompany.Text = c.Company;
                    txtCompanyUsername.Text = c.CompanyUsername;
                    dtpDate.SelectedDate = c.Expiry;
                    txtAdminName.Text = c.AdminName;
                    txtAdminUsername.Text = c.AdminUsername;
                    txtAdminPassword.Text = c.AdminPassword;
                    cmbStatus.SelectedIndex = c.IsActive == true ? 0 : 1;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void CreateNewCompany()
        {
            ClearEditBoxes();
            txtCompany.Focus();
        }

        private bool DeleteFromDatabase()
        {
            if (mCurrentCompany != null)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        private bool SaveDataToDatabase()
        {
            try
            {

                if (txtCompany.Text.Trim().Length < 3)
                {
                    MessageBox.Show("Please provide Company Name having 3 or more letters");
                    txtCompany.Focus();
                    return false;
                }

                if (txtCompanyUsername.Text.Trim().Length < 3 || txtCompanyUsername.Text.Trim().Length > 8)
                {
                    MessageBox.Show("Please provide Company Username having 3 or more letters and maximum 8 letters");
                    txtCompanyUsername.Focus();
                    return false;
                }

                if (txtAdminUsername.Text.Trim().Length < 3 || txtAdminUsername.Text.Trim().Length > 8)
                {
                    MessageBox.Show("Please provide Admin Username having 3 or more letters and maximum 8 letters");
                    txtAdminUsername.Focus();
                    return false;
                }

                if (txtAdminPassword.Text.Trim().Length < 3 || txtAdminPassword.Text.Trim().Length > 8)
                {
                    MessageBox.Show("Please provide Admin Password having 3 or more letters and maximum 8 letters");
                    txtAdminPassword.Focus();
                    return false;
                }


                CCompany c = new CCompany();
                c.Company = txtCompany.Text.Trim();
                c.CompanyUsername = txtCompanyUsername.Text.Trim();
                c.Expiry = dtpDate.SelectedDate.Value;
                c.AdminName = txtAdminName.Text.Trim();
                c.AdminUsername = txtAdminUsername.Text.Trim();
                c.AdminPassword = txtAdminPassword.Text.Trim();
                c.IsActive = cmbStatus.SelectedIndex == 1 ? false : true;

                if (mCurrentCompany != null)
                {
                    c.Id = mCurrentCompany.Id;
                    c.AdminId = mCurrentCompany.AdminId;

                    CBoolMessage cbm = mCompanyBackend.UpdateCompany(c);

                    if (!cbm.IsSuccess)
                    {
                        MessageBox.Show(cbm.Message);
                        return false;
                    }
                }
                else
                {
                    CBoolMessage cbm = mCompanyBackend.CreateCompany(c);

                    if (!cbm.IsSuccess)
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

            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectDataToEditBoxes(((sender as TreeView).SelectedItem as TreeViewItem).Tag as CCompany);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDataToDatabase())
            {
                NewForm();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            CreateNewCompany();
        }

        private void TreeItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectDataToEditBoxes((sender as TreeViewItem).Tag as CCompany);
        }

        private void cmbStatus_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (SaveDataToDatabase())
                {
                    NewForm();
                }
            }
        }
        
    }
}
