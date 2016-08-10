using System.Windows;


namespace Paranoid
{
    /// <summary>
    /// Interaction logic for EditContactDialog.xaml
    /// </summary>
    public partial class EditContactDialog : Window
    {
        private Contact Cnt;
        public EditContactDialog(Contact ContactToEdit)
        {
            Cnt = ContactToEdit;
            InitializeComponent();
            CntAddress.Text = Cnt.ContactAddress;
            CntStatus.Text = Contact.StatusToStr(Cnt.Status);
            ContactNameTextBox.Text = Cnt.Name;
            CommentsTextBox.Text = Cnt.Comments;
        }


        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Cnt.ContactName = ContactNameTextBox.Text;
            Cnt.Comments = CommentsTextBox.Text;
            CryptoData.SaveKeys();
            Close();
        }
    }
}
