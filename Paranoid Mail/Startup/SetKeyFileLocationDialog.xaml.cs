using System;
using System.Collections.ObjectModel;
using System.Windows;


namespace Paranoid
{
    /// <summary>
    /// Interaction logic for SetKeyFileLocationDialog.xaml
    /// </summary>
    public partial class SetKeyFileLocationDialog : Window
    {
        public SetKeyFileLocationDialog()
        {
            InitializeComponent();
            FileNameTextBox.Text = System.AppDomain.CurrentDomain.BaseDirectory + "paranoid.key";
        }

        private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = System.IO.Path.GetFileName(FileNameTextBox.Text),
                InitialDirectory = System.IO.Path.GetDirectoryName(FileNameTextBox.Text),
                Filter = "All files (*.*)|*.*"
            };
            bool? result=dlg.ShowDialog();
            if (result == true) FileNameTextBox.Text = dlg.FileName;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            CryptoData.FileName = FileNameTextBox.Text;
            CryptoData.Accounts=new ObservableCollection<Account>();

            try
            {
                CryptoData.SaveKeys();
            }
            catch
            {
                MessageBox.Show("Error - can't create key file, please select another location");
                return;
            }
            Utils.UpdateStringValue("KeyFileName", FileNameTextBox.Text);
            DialogResult = true;

        }
    }
}
