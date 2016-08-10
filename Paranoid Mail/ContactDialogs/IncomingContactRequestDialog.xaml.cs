using System.Windows;


namespace Paranoid
{
    /// <summary>
    /// Interaction logic for IncomingContactRequestDialog.xaml
    /// </summary>
    public partial class IncomingContactRequestDialog : Window
    {
        Contact Cnt;
        public IncomingContactRequestDialog(Contact Cnt)
        {
            this.Cnt = Cnt;
            InitializeComponent();
            ContactNameTextBlock.Text = Cnt.ContactName;
            MessageTextBlock.Text = Cnt.Comments;
        }

        private void RejectButton_OnClick(object sender, RoutedEventArgs e)
        {
            Cnt.IncomingContactRequestRefused();
            Close();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
            (new AcceptIncomingContactRequest(Cnt)).ShowDialog();

        }
    }
}
