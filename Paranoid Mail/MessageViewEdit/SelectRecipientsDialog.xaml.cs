using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for SelectRecipientsDialog.xaml
    /// </summary>
    public partial class SelectRecipientsDialog : Window
    {
        private ObservableCollection<Contact> ToContacts;
        public SelectRecipientsDialog(ObservableCollection<Contact> ToCnts, Account Acc=null)
        {
            InitializeComponent();
            ToContacts = ToCnts;
            AccountSelectionComboBox.ItemsSource = CryptoData.Accounts;
            if (Acc != null) AccountSelectionComboBox.SelectedItem = Acc;
            else AccountSelectionComboBox.SelectedIndex = 0;
            ToListView.ItemsSource = ToContacts;
        }

        private void AccountSelectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountSelectionComboBox.SelectedItem == null) return;
            ContactsListBox.ItemsSource = ((Account)(AccountSelectionComboBox.SelectedItem)).Contacts.Where(p=>p.Status==ContactStatus.Estabilished);
        }



        private void ContactsListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Contact Cnt = ((FrameworkElement)e.OriginalSource).DataContext as Contact;
            if (Cnt!=null) AddContact (Cnt);
        }

        private void ToListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Contact Cnt = ((FrameworkElement)e.OriginalSource).DataContext as Contact;
            if (Cnt != null) ToContacts.Remove(Cnt);
        }


        private void ToListView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Contact Cnt = ((FrameworkElement) e.OriginalSource).DataContext as Contact;
                if (Cnt != null) ToContacts.Remove(Cnt);
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            Contact Cnt = (Contact)ContactsListBox.SelectedItem;
            if (Cnt != null) AddContact(Cnt);
        }

        private bool AddContact(Contact Cnt)
        {
            if (ToContacts.Contains(Cnt)) return false;
            ToContacts.Add(Cnt);
            return true;
        }

    }

}
