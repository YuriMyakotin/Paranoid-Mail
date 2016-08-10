using System.Windows;


namespace Paranoid
{
    /// <summary>
    /// Interaction logic for AccountsWindow.xaml
    /// </summary>
    public partial class AccountsWindow : Window
    {
        public AccountsWindow()
        {
            InitializeComponent();
            AccountsBox.ItemsSource = CryptoData.Accounts;
            AccountsBox.SelectedIndex = 0;
        }


        private void NewButton_OnClick(object sender, RoutedEventArgs e)
        {
            RegisterAccount RA=new RegisterAccount();
            RA.ShowDialog();
        }


        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
