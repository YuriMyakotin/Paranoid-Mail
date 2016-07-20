using System.Windows;

namespace Paranoid
{
    /// <summary>
    /// Interaction logic for DeleteAccountDialog.xaml
    /// </summary>
    public partial class DeleteAccountDialog : Window
    {
        public DeleteAccountDialog(string AccountName)
        {
            InitializeComponent();
            AccName.Text = "\"" + AccountName + "\"?";
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}