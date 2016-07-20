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
    /// Interaction logic for IDCalculatorWindow.xaml
    /// </summary>
    public partial class IDCalculatorWindow : Window
    {
        public IDCalculatorWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            IDBox.Text = InputBox.Text.Length < 1 ? string.Empty : (Str2Hash.StringToHash(InputBox.Text)).ToString();
        }
    }
}
