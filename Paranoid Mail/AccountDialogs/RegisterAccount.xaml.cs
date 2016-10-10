using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dapper;


namespace Paranoid
{
	/// <summary>
	/// Interaction logic for RegisterAccount.xaml
	/// </summary>
	public partial class RegisterAccount : Window
	{
		private long SelectedServerID;
		private ulong SelectedUserID;
		private bool isCaptchaResultEntered = false;
		private TaskCompletionSource<object> continueClicked;
		private int CaptchaNumber;

		public RegisterAccount()
		{
			List<Server> srvList;
			using (DB DBC=new DB())
			{

				srvList = DBC.Conn.Query<Server>("Select * from Servers").ToList();
			}
			InitializeComponent();
			ServerSelectionComboBox.ItemsSource = srvList;
		}

		private async void RegisterButton_OnClick(object sender, RoutedEventArgs e)
		{
			ResultsTextBlock.Text = string.Empty;
			using (NetSession Net = new NetSession())
			{
				byte[] CaptchaImg;
				int SrvPort;
				int OverridePort;
				string OverrideIP;


				Server Srv = (Server) ServerSelectionComboBox.SelectedValue;

				if (!int.TryParse(PortTextBox.Text, out SrvPort))
				{
					DisplayError("Invalid Port");
					return;
				}

				if ((SrvPort <= 0) || (SrvPort > 65535))
				{
					DisplayError("Invalid Port");
					return;
				}


				if ((Srv.IP != IpTextBox.Text)|| (PrivatePortPasswordTextBox.Text.Length > 0))
				{
					Srv.IP = IpTextBox.Text;
					OverrideIP = IpTextBox.Text;
				}
				else OverrideIP = string.Empty;

				if ((Srv.Port != SrvPort)||(PrivatePortPasswordTextBox.Text.Length>0))
				{
					Srv.Port = SrvPort;
					OverridePort = SrvPort;
				}
				else OverridePort = 0;

				Mouse.OverrideCursor = Cursors.Wait;

				string AccName = AccountNameTextBox.Text;
				if (AccName.Length == 0) AccName = UserNameTextBox.Text + "@" + Srv.Comments;


				Account Acc = new Account(SelectedServerID, (long) SelectedUserID, AccName, OverrideIP,
					OverridePort, PrivatePortPasswordTextBox.Text, Encoding.UTF8.GetBytes(RandomDataTextBox.Text));

				Net.PortPassword = Acc.PrivatePortPassword;
				NetSessionResult NSR = Net.Connect(Srv);
				Mouse.OverrideCursor = Cursors.Arrow;
				if (NSR != NetSessionResult.Ok)
				{
					switch (NSR)
					{
						case NetSessionResult.CantConnect:
							DisplayError("Can't connect to selected server");
							break;
						case NetSessionResult.InvalidCredentials:
							DisplayError("Security error");
							break;
						case NetSessionResult.InvalidData:
							DisplayError("Data error");
							break;
						case NetSessionResult.NetError:
							DisplayError("Network error");
							break;
					}
					return;
				}
				Net.SendBuff = NetSession.MakeCmd(CmdCode.UserRegistrationRequest);

				if (!Net.SendEncrypted())
				{
					DisplayError("Network Error");
					return;
				}
				if (!Net.ReceiveCommand())
				{
					DisplayError("Network Error");
					return;
				}
				switch ((CmdCode) Net.RecvCmdCode)
				{
					case CmdCode.ServiceNotEnabled:
						DisplayError("Selected server not support auto registration");
						return;
					case CmdCode.UserRegistrationTryAgainLater:
						DisplayError("Registration denied right now, try again later");
						return;
					case CmdCode.CaptchaForRegistration:
						break;
					default:
						DisplayError("Data error");
						return;
				}


				try
				{
					CaptchaImg = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(Net.RecvBuff);
				}
				catch
				{
					DisplayError("Data error");
					return;
				}

				{
					BitmapImage biImg = new BitmapImage();
					MemoryStream ms = new MemoryStream(CaptchaImg);
					biImg.BeginInit();
					biImg.StreamSource = ms;
					biImg.EndInit();

					img.Source = biImg;
				}

				Mouse.OverrideCursor = Cursors.Arrow;
				CapchaGroupBox.Visibility = Visibility.Visible;
				CaptchaNumberTextBox.Text = "";
				RegisterButton.IsEnabled = false;
				CloseButton.IsEnabled = false;

				continueClicked = new TaskCompletionSource<object>();
				await continueClicked.Task;

				RegisterButton.IsEnabled = true;
				CloseButton.IsEnabled = true;

				CapchaGroupBox.Visibility = Visibility.Collapsed;
				if (!isCaptchaResultEntered)
				{
					DisplayError("Cancelled by user");
					return;
				}

				Mouse.OverrideCursor = Cursors.Wait;
				byte[] tmp = BitConverter.GetBytes(CaptchaNumber);
				Net.SendBuff = NetSession.MakeCmd(CmdCode.RegistrationCaptchaReply, tmp, tmp.Length, 0);

				if (!Net.SendEncrypted())
				{
					DisplayError("Network Error");
					return;
				}
				if (!Net.ReceiveCommand())
				{
					DisplayError("Network Error");
					return;
				}
				if (Net.RecvCmdCode != (byte) CmdCode.RegistrationCaptchaOk)
				{
					DisplayError("Wrong Captcha answer");
					return;
				}

				Net.SendBuff = NetSession.MakeCmd<User>(CmdCode.ClientRegistrationData, Acc.MakeUserInfo());

				if (!Net.SendEncrypted())
				{
					DisplayError("Network Error");
					return;
				}
				if (!Net.ReceiveCommand())
				{
					DisplayError("Network Error");
					return;
				}

				Mouse.OverrideCursor = Cursors.Arrow;
				switch ((CmdCode) Net.RecvCmdCode)
				{
					case CmdCode.RegistrationAccepted:
						ResultsTextBlock.Text = "Registration done";
						ResultsTextBlock.Foreground = Brushes.Green;

						CryptoData.Accounts.Add(Acc);
						CryptoData.SaveKeys();
						RegisterButton.IsEnabled = false;
						return;

					case CmdCode.RegistrationIDAlreadyTaken:
						DisplayError("ID already in use");
						return;

					default:
						DisplayError("Data error");
						return;
				}
			}
		}

		private void UserNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			SelectedUserID = Str2Hash.StringToHash(UserNameTextBox.Text);
			CalculateUserID();
		}

		private void SelectServer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectedServerID = ((Server)(ServerSelectionComboBox.SelectedValue)).ServerID;
			CalculateUserID();
			IpTextBox.Text = ((Server) (ServerSelectionComboBox.SelectedValue)).IP;
			PortTextBox.Text = ((Server) (ServerSelectionComboBox.SelectedValue)).Port.ToString();

		}

		private void CalculateUserID()
		{
			if (UserNameTextBox.Text.Length >= 1)
			{
				RegisterButton.IsEnabled = true;
				UserIDTextBlock.Content = SelectedUserID.ToString() + "@"+((ulong)SelectedServerID).ToString();
			}
			else
			{
				RegisterButton.IsEnabled = false;
				UserIDTextBlock.Content = "none";
			}
			ResultsTextBlock.Text=string.Empty;
		}

		private void DisplayError(string ErrorName)
		{
			ResultsTextBlock.Text = ErrorName;
			ResultsTextBlock.Foreground= Brushes.Red;
		}



		private void CaptchaOkButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (Int32.TryParse(CaptchaNumberTextBox.Text, out CaptchaNumber))
			{
				isCaptchaResultEntered = true;
				continueClicked?.TrySetResult(null);
			}
		}

		private void CaptchaCancelButton_OnClick(object sender, RoutedEventArgs e)
		{
			isCaptchaResultEntered = false;
			continueClicked?.TrySetResult(null);
		}
	}
}
