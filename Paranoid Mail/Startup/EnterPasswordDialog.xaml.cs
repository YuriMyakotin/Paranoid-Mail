using System.Windows;


namespace Paranoid
{
	/// <summary>
	/// Interaction logic for EnterPasswordDialog.xaml
	/// </summary>
	public partial class EnterPasswordDialog : Window
	{
		public EnterPasswordDialog()
		{
			InitializeComponent();
			PwdTextBox.Focus();
			VKeys.BindPasswordBox(PwdTextBox);
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (PwdTextBox.Password.Length > 0)
			{
				DialogResult = true;
				CryptoData.SetMasterKey(PwdTextBox.Password);
				PwdTextBox.Password.ClearString();

			}
		}
	}
}
