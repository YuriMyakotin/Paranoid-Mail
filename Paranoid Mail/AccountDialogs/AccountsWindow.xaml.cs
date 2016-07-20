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
