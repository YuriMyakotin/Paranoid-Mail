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
    /// Interaction logic for ExistingFileReplaceDialog.xaml
    /// </summary>
    public partial class ExistingFileReplaceDialog : Window
    {
        public YesNoToAll Result;
        public ExistingFileReplaceDialog(string Name1, string Name2, bool isMultiple)
        {

            InitializeComponent();
            if (!isMultiple)
            {
                YesToAllBtn.Visibility=Visibility.Collapsed;
                NoToAllBtn.Visibility = Visibility.Collapsed;
            }
            ExistingFileNameTextBlock.Text = Name1;
            NewNameTextBlock.Text = Name2 + "?";
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result=YesNoToAll.Yes;
            DialogResult = true;

        }

        private void YesToAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = YesNoToAll.YesToAll;
            DialogResult = true;
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = YesNoToAll.No;
            DialogResult = true;
        }

        private void NoToAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = YesNoToAll.NoToAll;
            DialogResult = true;
        }
    }
}
