using System.Windows;

namespace Paranoid
{
    /// <summary>
    /// Interaction logic for AcceptIncomingContactRequest.xaml
    /// </summary>
    public partial class AcceptIncomingContactRequest : Window
    {
        private Contact Cnt;
        public AcceptIncomingContactRequest(Contact Cnt)
        {
            this.Cnt = Cnt;
            InitializeComponent();
            CntAddress.Text = Cnt.ContactAddress;
            ContactNameTextBox.Text = Cnt.Name;
            CommentsTextBox.Text = Cnt.Comments;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Cnt.ContactName = ContactNameTextBox.Text;
            Cnt.Comments = CommentsTextBox.Text;
            Cnt.IncomingContactRequestAccepted(RandomDataTextBox.Text);
            Close();
        }
    }
}
