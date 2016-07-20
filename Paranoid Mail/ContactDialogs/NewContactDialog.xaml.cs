using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Media;
using Dapper;

namespace Paranoid
{
    /// <summary>
    /// Interaction logic for NewContactDialog.xaml
    /// </summary>
    public partial class NewContactDialog : Window
    {
        public NewContactDialog(Account Acc)
        {
            InitializeComponent();
            AccountSelectionComboBox.ItemsSource = CryptoData.Accounts;
            if (Acc != null) AccountSelectionComboBox.SelectedItem = Acc;
            else AccountSelectionComboBox.SelectedIndex = 0;
            List<Server> SrvList;

            using (var DBC=new DB())
            {

                SrvList = DBC.Conn.Query<Server>("Select * from Servers").ToList();
            }
            ServerSelectionComboBox.ItemsSource = SrvList;

        }


        private void UserName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UserID.Text = Str2Hash.StringToHash(UserName.Text).ToString();
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            ulong NewUserID;
            if (!UInt64.TryParse(UserID.Text, out NewUserID))
            {
                UserID.BorderBrush=Brushes.Red;
                SetErrorMsg("Invalid User ID");
                return;
            }
            UserID.BorderBrush = Brushes.Black;

            if (ServerSelectionComboBox.SelectedIndex == -1)
            {
                SetErrorMsg("You must select contact's server");
                return;
            }
            Account Acc = (Account) (AccountSelectionComboBox.SelectedItem);
            long SrvID = ((Server) (ServerSelectionComboBox.SelectedItem)).ServerID;
            if (Acc.FindContact((long) NewUserID, SrvID) != null)
            {
                SetErrorMsg("This contact already exist");
                return;
            }

            Contact.RequestContactInfo(Acc,SrvID,(long)NewUserID,ContactName.Text,Comments.Text,Message.Text,RandomDataTextBox.Text);
            Close();
        }

        private void SetErrorMsg(string Msg)
        {
            ErrorMsg.Text =Msg;
            ErrorMsg.Visibility = Visibility.Visible;
        }
    }
}
