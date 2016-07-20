using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paranoid
{
	/// <summary>
	/// Interaction logic for VirtualKey.xaml
	/// </summary>
	public partial class VirtualKey : UserControl
	{
		private char NormalChar { get; set; }
		private char ShiftChar { get; set; }
		private VirtualKeyboard Kbd { get; set; }

		private bool isShift=false;

		public VirtualKey(VirtualKeyboard Parent,char NormChar,char ShftChar)
		{
			Kbd = Parent;
			NormalChar = NormChar;
			ShiftChar = ShftChar;
			InitializeComponent();
			Chr.Content = NormalChar.ToString();

		}

		public void SetShift(bool isShiftPressed)
		{
			isShift = isShiftPressed;
			Chr.Content = isShiftPressed ? ShiftChar.ToString() : NormalChar.ToString();
		}

		private void Chr_OnClick(object sender, RoutedEventArgs e)
		{
			Kbd.KeyPressed(isShift ? ShiftChar.ToString() : NormalChar.ToString());
		}
	}
}
