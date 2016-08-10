using System.Windows;


namespace Paranoid
{
    /// <summary>
    /// Interaction logic for DeleteContactDialog.xaml
    /// </summary>
    public partial class DeleteContactDialog : Window
    {
        public DeleteContactDialog(string ContactName)
        {
            InitializeComponent();
            CntName.Text= "\""+ContactName + "\"?";
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
