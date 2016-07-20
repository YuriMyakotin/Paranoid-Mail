using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Paranoid
{
    /// <summary>
    /// Interaction logic for OpenExistingKeyFileDialog.xaml
    /// </summary>
    public partial class OpenExistingKeyFileDialog : Window
    {
        public OpenExistingKeyFileDialog()
        {
            InitializeComponent();
            FileNameTextBox.Text= Utils.GetStringValue("KeyFileName");
        }
        private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (FileNameTextBox.Text != String.Empty)
            {
                dlg.FileName = System.IO.Path.GetFileName(FileNameTextBox.Text);
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileNameTextBox.Text);
            }
            dlg.Filter = "All files (*.*)|*.*";
            bool? result = dlg.ShowDialog();
            if (result == true) FileNameTextBox.Text = dlg.FileName;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            CryptoData.FileName = FileNameTextBox.Text;
            Utils.UpdateStringValue("KeyFileName", FileNameTextBox.Text);
            this.DialogResult = true;

        }
    }
}
