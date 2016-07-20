using System.Windows;


namespace Paranoid
{
	/// <summary>
	/// Interaction logic for SetPasswordWindow.xaml
	/// </summary>
	public partial class SetPasswordWindow : Window
	{
		private readonly bool isFirstTime;
		public SetPasswordWindow(bool isFirstTime)
		{
			this.isFirstTime = isFirstTime;
			InitializeComponent();
			if (isFirstTime)
			{
				Title = "Set master password";
				OldPasswordRow.Visibility = Visibility.Collapsed;
				textBlock1.Visibility=Visibility.Visible;
			}
			else
			{
				Title = "Change master password";
			}
		}


		private void NewPwd__OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			if (NewPwdTextBox.Password.Length == 0)
			{
				PasswordStrenght.Visibility = Visibility.Hidden;
				PasswordMatch.Visibility = Visibility.Hidden;
				OkButton.IsEnabled = false;
				return;
			}

			if (NewPwdTextBox.Password == NewPwdCopyTextBox.Password)
			{
				PasswordMatch.Fill = System.Windows.Media.Brushes.LightGreen;
				PasswordMatch.ToolTip = "Match";
			}
			else
			{
				PasswordMatch.ToolTip = "Not match";
				PasswordMatch.Fill = System.Windows.Media.Brushes.OrangeRed;
			}
			PasswordStrenght.Visibility = Visibility.Visible;
			PasswordMatch.Visibility = Visibility.Visible;
			int PwdQuality = CryptoData.isPasswordStrong(NewPwdTextBox.Password);
			if (PwdQuality < 3)
			{
				PasswordStrenght.ToolTip= "Too weak";
				PasswordStrenght.Fill = System.Windows.Media.Brushes.OrangeRed;
				OkButton.IsEnabled = false;
			}
			else
			{
				OkButton.IsEnabled = true;
				if (PwdQuality > 3)
				{
					PasswordStrenght.ToolTip = "Excellent";
					PasswordStrenght.Fill = System.Windows.Media.Brushes.LightGreen;
				}
				else
				{
					PasswordStrenght.ToolTip = "Acceptable";
					PasswordStrenght.Fill = System.Windows.Media.Brushes.Gold;
				}


			}
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (!isFirstTime)
			{
				if (!CryptoData.ComparePassword(OldPwdTextBox.Password))
				{
					MessageBox.Show("Error - current password not match");
					return;
				}
				CryptoData.SetMasterKey(NewPwdTextBox.Password);
				CryptoData.SaveKeys();
			}
			else
			{
				CryptoData.SetMasterKey(NewPwdTextBox.Password);
			}

			NewPwdCopyTextBox.Password.ClearString();
			NewPwdTextBox.Password.ClearString();
			OldPwdTextBox.Password.ClearString();

			DialogResult = true;
		}

		private void OldPwdTextBox_OnGotFocus(object sender, RoutedEventArgs e)
		{
			VKeys.BindPasswordBox(OldPwdTextBox);
		}

		private void NewPwdTextBox_OnGotFocus(object sender, RoutedEventArgs e)
		{
			VKeys.BindPasswordBox(NewPwdTextBox);
		}

		private void NewPwdCopyTextBox_OnGotFocus(object sender, RoutedEventArgs e)
		{
			VKeys.BindPasswordBox(NewPwdCopyTextBox);
		}
	}
}
