using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Paranoid
{
	/// <summary>
	/// Interaction logic for VirtualKeyboard.xaml
	/// </summary>
	public partial class VirtualKeyboard : UserControl
	{
		private readonly List<VirtualKey> Keys=new List<VirtualKey>();

		private PasswordBox PB;

		public VirtualKeyboard()
		{
			InitializeComponent();

			AddKey(P1, '`', '~');
			AddKey(P1, '1', '!');
			AddKey(P1, '2', '@');
			AddKey(P1, '3', '#');
			AddKey(P1, '4', '$');
			AddKey(P1, '5', '%');
			AddKey(P1, '6', '^');
			AddKey(P1, '7', '&');
			AddKey(P1, '8', '*');
			AddKey(P1, '9', '(');
			AddKey(P1, '0', ')');
			AddKey(P1, '-', '_');
			AddKey(P1, '=', '+');

			AddKey(P2, 'q', 'Q');
			AddKey(P2, 'w', 'W');
			AddKey(P2, 'e', 'E');
			AddKey(P2, 'r', 'R');
			AddKey(P2, 't', 'T');
			AddKey(P2, 'y', 'Y');
			AddKey(P2, 'u', 'U');
			AddKey(P2, 'i', 'I');
			AddKey(P2, 'o', 'O');
			AddKey(P2, 'p', 'P');
			AddKey(P2, '[', '{');
			AddKey(P2, ']', '}');
			AddKey(P2, '\\', '|');

			AddKey(P3, 'a', 'A');
			AddKey(P3, 's', 'S');
			AddKey(P3, 'd', 'D');
			AddKey(P3, 'f', 'F');
			AddKey(P3, 'g', 'G');
			AddKey(P3, 'h', 'H');
			AddKey(P3, 'j', 'J');
			AddKey(P3, 'k', 'K');
			AddKey(P3, 'l', 'L');
			AddKey(P3, ';', ':');
			AddKey(P3, '\'', '"');
			AddKey(P3, '♠', '♣');
			AddKey(P3, '♥', '♦');


			AddKey(P4, 'z', 'Z');
			AddKey(P4, 'x', 'X');
			AddKey(P4, 'c', 'C');
			AddKey(P4, 'v', 'V');
			AddKey(P4, 'b', 'B');
			AddKey(P4, 'n', 'N');
			AddKey(P4, 'm', 'M');
			AddKey(P4, ',', '<');
			AddKey(P4, '.', '>');
			AddKey(P4, '/', '?');
			AddKey(P4, '©', '®');
			AddKey(P4, '№', '™');
			AddKey(P4, '±', '∑');
		}

		private void AddKey(Panel panel, char NormChar, char ShiftChar)
		{
			VirtualKey Key=new VirtualKey(this,NormChar,ShiftChar);
			Keys.Add(Key);
			panel.Children.Add(Key);
		}

		public void KeyPressed(string Char)
		{
			if (PB != null)
				PB.Password += Char;
		}

		public void BindPasswordBox(PasswordBox PassBox)
		{
			PB = PassBox;
		}

		private void ShiftChanged(bool isShift)
		{
			foreach(VirtualKey Key in Keys)
			  Key.SetShift(isShift);
		}


		private void ShiftButton_OnChecked(object sender, RoutedEventArgs e)
		{
			ShiftChanged(true);
		}

		private void ShiftButton_OnUnchecked(object sender, RoutedEventArgs e)
		{
			ShiftChanged(false);
		}

		private void SpaceButton_Click(object sender, RoutedEventArgs e)
		{
			KeyPressed(" ");
		}

		private void BackSpaceButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (PB?.Password.Length > 0) PB.Password=PB.Password.Remove(PB.Password.Length - 1);
		}
	}
}
