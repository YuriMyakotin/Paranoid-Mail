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
    /// Interaction logic for FirstTimeQuestionWindow.xaml
    /// </summary>
    public partial class FirstTimeQuestionWindow : Window
    {
        public bool isCreateNew;
        public FirstTimeQuestionWindow()
        {
            InitializeComponent();
        }

        private void ButtonNew_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            isCreateNew = true;
            this.Close();
        }
        private void ButtonExisting_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            isCreateNew = false;
            this.Close();
        }
    }
}
