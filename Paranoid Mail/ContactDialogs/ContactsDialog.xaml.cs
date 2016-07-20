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
    /// Interaction logic for ContactsDialog.xaml
    /// </summary>
    public partial class ContactsDialog : Window
    {
        public ContactsDialog(Account Acc)
        {
            InitializeComponent();
            AccountSelectionComboBox.ItemsSource = CryptoData.Accounts;
            if (Acc != null) AccountSelectionComboBox.SelectedItem = Acc;
            else AccountSelectionComboBox.SelectedIndex = 0;
        }

        private void AccountSelectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountSelectionComboBox.SelectedItem == null) return;
            ContactsListBox.ItemsSource = ((Account) (AccountSelectionComboBox.SelectedItem)).Contacts;
            if (((Account) (AccountSelectionComboBox.SelectedItem)).Contacts.Count != 0)
            {
                ChangeButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                ContactsListBox.SelectedIndex = 0;
            }
            else
            {
                ChangeButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }

        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
