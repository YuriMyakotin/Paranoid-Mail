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
using Dapper;

namespace Paranoid
{
	/// <summary>
	/// Interaction logic for EditAccountDialog.xaml
	/// </summary>
	public partial class EditAccountDialog : Window
	{
		private Account Acc;
		public EditAccountDialog(Account Acc)
		{
			Server Srv;
			InitializeComponent();
			this.Acc = Acc;
			using (var DBC=new DB())
			{

				Srv = DBC.Conn.QuerySingleOrDefault<Server>("Select * from Servers where ServerID=@SrvID",
					new { SrvID = Acc.ServerID });
			}
			if (Srv == null)
			{
				Close();
				return;
			}

			UserIDText.Text = ((ulong) Acc.UserID).ToString();
			ServerName.Text = Srv.ServerNameStr;
			AccountNameTextBox.Text = Acc.AccountName;
			IpTextBox.Text = Acc.OverrideIP;
			PortTextBox.Text = Acc.OverridePort != 0 ? Acc.OverridePort.ToString() : string.Empty;
			PrivatePortPasswordTextBox.Text = Acc.PrivatePortPassword;

			if (Acc.Status!=AccountStatus.Normal) ChangeKeysButton.Visibility=Visibility.Hidden;
		}

		private void SaveButton_OnClick(object sender, RoutedEventArgs e)
		{
			int SrvPort=0;

			PortTextBox.ClearValue(TextBox.BorderBrushProperty);


			if (PortTextBox.Text.Length != 0)
			{

				if (!int.TryParse(PortTextBox.Text, out SrvPort))
				{
					PortTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
					return;
				}
				;
				if ((SrvPort <= 0) || (SrvPort > 65535))
				{
					PortTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
					return;
				}
			}

			Acc.AccountNameStr = AccountNameTextBox.Text;
			Acc.OverrideIP = IpTextBox.Text;
			Acc.OverridePort = SrvPort;
			Acc.PrivatePortPassword = PrivatePortPasswordTextBox.Text;
			CryptoData.SaveKeys();
			Close();

		}

		private void ChangeKeysButton_OnClick(object sender, RoutedEventArgs e)
		{
			ButtonsPanel.Visibility = Visibility.Collapsed;
			ChangeKeysGroupBox.Visibility = Visibility.Visible;
		}

		private void ChangeKeysOkButton_OnClick(object sender, RoutedEventArgs e)
		{
			Acc.RequestKeysUpdate(Encoding.UTF8.GetBytes(RandomDataTextBox.Text));
			ButtonsPanel.Visibility = Visibility.Visible;
			ChangeKeysGroupBox.Visibility = Visibility.Collapsed;
			ChangeKeysButton.Visibility = Visibility.Hidden;
		}

		private void ChangeKeysCancelButton_OnClick(object sender, RoutedEventArgs e)
		{
			ButtonsPanel.Visibility = Visibility.Visible;
			ChangeKeysGroupBox.Visibility = Visibility.Collapsed;
		}
	}
}
