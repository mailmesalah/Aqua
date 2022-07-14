using ServerServiceInterface;
using System;
using System.ServiceModel;
using System.Windows;
using AquaClient.General;

namespace AquaClient.Settings.Company
{
    /// <summary>
    /// Interaction logic for DefaultValueSettings.xaml
    /// </summary>
    public partial class DefaultValueSettings : Window
    {
        BasicHttpBinding bhttb = new BasicHttpBinding()
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
        };

        public DefaultValueSettings()
        {
            InitializeComponent();
            LoadInitialDetails();
        }

        private void LoadInitialDetails()
        {
            GenerateTimes();
            GetDefaultValues();
        }

        private void GenerateTimes()
        {
            for (int i = 1; i <= 12; i++)
            {
                cmbStartMonths.Items.Add(i+"");
                cmbEndMonths.Items.Add(i+"");
            }

            for (int i = 1; i <= 31; i++)
            {
                cmbStartDays.Items.Add(i+"");
                cmbEndDays.Items.Add(i+"");
            }
            
        }

        private bool GetDefaultValues()
        {

            try
            {
                using (ChannelFactory<ISettings> settingsProxy = new ChannelFactory<ServerServiceInterface.ISettings>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/SettingsEndPoint");
                    ISettings settingsService = settingsProxy.CreateChannel(ep);
                    CDefaultValues cdv = settingsService.ReadDefaultValues(ApplicationStaticVariables.gClientLocalInfo.CompanyId);
                    if (cdv != null)
                    {
                        cmbStartMonths.SelectedItem = cdv.StartMonth.ToString();
                        cmbStartDays.SelectedItem = cdv.StartDay.ToString();
                        cmbEndMonths.SelectedItem = cdv.EndMonth.ToString();
                        cmbEndDays.SelectedItem = cdv.EndDay.ToString();
                    }
                    return (cdv != null);
                }
            }
            catch
            {

            }

            return false;
            
        }

        private bool SaveDataToDatabase()
        {
            try
            {
                if (cmbStartMonths.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select Start Hours");
                    cmbStartMonths.Focus();
                    return false;
                }

                if (cmbStartDays.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select Start Minutes");
                    cmbStartDays.Focus();
                    return false;
                }
        
                if (cmbEndMonths.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select End Hours");
                    cmbEndMonths.Focus();
                    return false;
                }

                if (cmbEndDays.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select End Minutes");
                    cmbEndDays.Focus();
                    return false;
                }

                
                using (ChannelFactory<ISettings> settingsProxy = new ChannelFactory<ServerServiceInterface.ISettings>(bhttb))
                {
                    EndpointAddress ep = new EndpointAddress(ApplicationStaticVariables.gServerAddress + "/SettingsEndPoint");
                    ISettings settingsService = settingsProxy.CreateChannel(ep);
                    CDefaultValues cdv = new CDefaultValues();
                    cdv.EndMonth = Convert.ToInt16(cmbEndMonths.Text);
                    cdv.EndDay = Convert.ToInt16(cmbEndDays.Text);
                    cdv.StartMonth = Convert.ToInt16(cmbStartMonths.Text);
                    cdv.StartDay = Convert.ToInt16(cmbStartDays.Text);

                    CBoolMessage cbm = settingsService.UpdateDefaultValues(cdv, ApplicationStaticVariables.gClientLocalInfo.CompanyId, ApplicationStaticVariables.gClientLocalInfo.SessionId);

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


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDataToDatabase())
            {
                MessageBox.Show("Successfully Saved");
            }
        }
    }
}
