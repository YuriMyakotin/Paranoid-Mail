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
