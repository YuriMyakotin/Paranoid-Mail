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
