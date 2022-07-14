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
    public partial class ManageCompany : Window
    {
        private ObservableCollection<TreeViewItem> mTreeContents = new ObservableCollection<TreeViewItem>();
        CompanyBackend mCompanyBackend = new CompanyBackend();

        public ManageCompany()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {
            NewForm();
        }

        private void GetCompanies()
        {
            try
            {
                List<CCompany> companies = mCompanyBackend.ReadAllCompanies();
                cmbCompany.ItemsSource = companies;
                cmbCompany.SelectedValuePath = "Id";
                cmbCompany.DisplayMemberPath = "Company";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void NewForm()
        {
            GetCompanies();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CompanyBackend cb = new CompanyBackend();
                if (cb.Backup())
                {
                    MessageBox.Show("Successfully Backed up.");
                }
                else
                {
                    MessageBox.Show("Failed due to Error.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime dt = dtpDate.SelectedDate.Value;
                DateTime now = System.DateTime.Now;
                int days = (int)now.Subtract(dt).TotalDays;
                if (days < 0)
                {
                    MessageBox.Show("Selected Date is incorrect.");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("Are you sure to Delete Everything before " + days + " Days", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {

                    if (((CCompany)cmbCompany.SelectedItem).AdminPassword.Equals(txtPassword.Text))
                    {
                        CompanyBackend cb = new CompanyBackend();
                        if (cb.Delete((long)cmbCompany.SelectedValue, dt))
                        {
                            MessageBox.Show("Successfully Deleted.");
                        }
                        else
                        {
                            MessageBox.Show("Failed due to Error.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wrong Password");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
