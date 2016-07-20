using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using static Paranoid.Utils;

namespace Paranoid
{
	public partial class RegisterNewServerForm : Form
	{

		private bool isCaptchaResultEntered = false;
		private TaskCompletionSource<object> continueClicked;
		private int CaptchaNumber;

		public RegisterNewServerForm()
		{
			InitializeComponent();
		}

		private void ServerNameTextBox_TextChanged(object sender, EventArgs e)
		{
			ErrorLabel.Visible = false;
			if (ServerNameTextBox.Text.Length < 1)
			{
				IDLabel.Text = string.Empty;
				RegisterButton.Enabled = false;
			}
			else
			{
				IDLabel.Text = (Str2Hash.StringToHash(ServerNameTextBox.Text)).ToString();
				RegisterButton.Enabled = true;
			}
		}

		private async void RegisterButton_Click(object sender, EventArgs e)
		{
			ErrorLabel.Visible = false;


			if (!Checks.CheckPort(PortTextBox.Text))
			{
				DisplayError("Invalid port number");
				return;
			}
			PortTextBox.BackColor = Color.White;
			if (!Checks.CheckIP(IpTextBox.Text))
			{
				DisplayError("Invalid IP");
				return;
			}
			long MyServerID = (long) Str2Hash.StringToHash(ServerNameTextBox.Text);

			if (isServerExists(MyServerID))
			{
				DisplayError("Server ID already in use");
				return;
			}

			Server NewSrv = new Server
			{
				ServerID = MyServerID,
				Comments = DescriptionTextBox.Text,
				IP = IpTextBox.Text,
				Port = int.Parse(PortTextBox.Text),
				CurrentPublicKeyExpirationTime = DateTimeToLong(DateTime.Today) + LongTime.Days(90),
				ServerInfoTime = LongTime.Now
			};

			byte[] CurrentPrivateKey = new byte[32];
			byte[] NextPrivateKey = new byte[32];

			ParanoidRNG.GetBytes(CurrentPrivateKey, 0, 32, Encoding.UTF8.GetBytes(RandomDataTextBox.Text));
			ParanoidRNG.GetBytes(NextPrivateKey, 0, 32, Encoding.Unicode.GetBytes(RandomDataTextBox.Text));

			UpdateBinaryValue("CurrentPrivateKey",CurrentPrivateKey);
			UpdateBinaryValue("NextPrivateKey", NextPrivateKey);

			NewSrv.CurrentPublicKey = Chaos.NaCl.Ed25519.PublicKeyFromSeed(CurrentPrivateKey);
			NewSrv.NextPublicKey = Chaos.NaCl.Ed25519.PublicKeyFromSeed(NextPrivateKey);



			if (RelayCheckBox.Checked)
				NewSrv.ServerFlags = (int) ServerFlagBits.RelayingEnabled;


			if (RegistrationCheckBox.Checked)
				NewSrv.ServerFlags |= (int) ServerFlagBits.UsersRegistrationEnabled;


			Cursor.Current = Cursors.WaitCursor;
			long RootServerID = GetRandomRootServerID();
			if (RootServerID == 0)
			{
				DisplayError("Can't connect to root servers right now, try again later");
				Cursor.Current = Cursors.Default;
				return;
			}

			using (NetSession Net = new NetSession())
			{

				NetSessionResult NSR = Net.Connect(RootServerID);
				Cursor.Current = Cursors.Default;
				if (NSR != NetSessionResult.Ok)
				{
					NetSession.AfterSession(RootServerID, NSR);
					switch (NSR)
					{
						case NetSessionResult.CantConnect:
							DisplayError("Can't connect to root server");
							return;
						case NetSessionResult.InvalidCredentials:
							DisplayError("Security error");
							return;
						case NetSessionResult.InvalidData:
							DisplayError("Data error");
							return;
						case NetSessionResult.NetError:
							DisplayError("Network error");
							return;
					}

				}

				Net.SendBuff = NetSession.MakeCmd<Server>(CmdCode.SrvRegistrationRequest, NewSrv);
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
				switch ((CmdCode)Net.RecvCmdCode)
				{
					case CmdCode.ServiceNotEnabled:
						DisplayError("Server registration not supported");
						return;
					case CmdCode.CaptchaForRegistration:
						break;
					default:
						DisplayError("Data error");
						return;
				}

				byte[] CaptchaImg;
				try
				{
					CaptchaImg = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(Net.RecvBuff);
				}
				catch
				{
					DisplayError("Data error");
					return;
				}
				Net.Sock.SendTimeout = NetworkVariables.TimeoutInteractive;
				Net.Sock.ReceiveTimeout = NetworkVariables.TimeoutInteractive;

				using (MemoryStream MS = new MemoryStream(CaptchaImg))
				{
					CaptchaPictureBox.Image = Image.FromStream(MS);
				}
				CaptchaGroupBox.Visible = true;

				CaptchaAnswerTextBox.Text = "";
				continueClicked = new TaskCompletionSource<object>();
				await continueClicked.Task;

				CaptchaGroupBox.Visible = false;
				if (!isCaptchaResultEntered)
				{
					DisplayError("Cancelled by user");
					return;
				}

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


				switch ((CmdCode) Net.RecvCmdCode)
				{
					case CmdCode.RegistrationCaptchaOk:
						break;
					case CmdCode.RegistrationBadCaptcha:
						DisplayError("Incorrect captcha answer");
						return;
					default:
						DisplayError("Data Error");
						return;

				}

				if (!Net.ReceiveCommand())
				{
					DisplayError("Network Error");
					return;
				}

				switch ((CmdCode) Net.RecvCmdCode)
				{
					case CmdCode.SrvRegistrationInvalidData:
						DisplayError("Invalid server registration data");
						return;
					case CmdCode.SrvIDAlreadyTaken:
						DisplayError("This Server ID already in use");
						return;

					case CmdCode.SrvRegistrationDone:
						NewSrv.InsertIntoDB();
						UpdateIntValue("ServerID",NewSrv.ServerID);
						MessageBox.Show("Server registration successfully completed");
						DialogResult=DialogResult.OK;
						Close();
						return;

					case CmdCode.SrvRegistrationInProgress:
						if (Net.RecvCmdSize != 8)
						{
							DisplayError("Data Error");
							return;
						}
						long ReqID = BitConverter.ToInt64(Net.RecvBuff, 0);

						using (DB DBC = new DB())
						{
							try
							{
								DBC.Conn.Execute(
									"Insert Into NewServerRegData values(@RequestID,@RegTime,@NewSrvID,@Flags,@IP,@Port,@Pkey,@KeyExpTime,@NextPKey,@Comments,0)",
									new
									{
										RequestID = ReqID,
										RegTime = LongTime.Now,
										NewSrvID = NewSrv.ServerID,
										Flags = NewSrv.ServerFlags,
										IP = NewSrv.IP,
										Port = NewSrv.Port,
										Pkey = NewSrv.CurrentPublicKey,
										KeyExpTime = NewSrv.CurrentPublicKeyExpirationTime,
										NextPKey = NewSrv.NextPublicKey,
										Comments = NewSrv.Comments
									});
							}
							catch (Exception Ex)
							{
								MessageBox.Show(Ex.Message);
							}
						}
						UpdateIntValue("RegistratorID",RootServerID);

						MessageBox.Show("Server registration in progress, please check result later");
						DialogResult = DialogResult.Cancel;
						Close();
						return;
					default:
						DisplayError("Data Error");
						return;
				}



			}
	}

		private void CaptchaOkButton_Click(object sender, EventArgs e)
		{
			if (int.TryParse(CaptchaAnswerTextBox.Text, out CaptchaNumber))
			{
				isCaptchaResultEntered = true;
				continueClicked?.TrySetResult(null);
			}
		}

		private void CaptchaCancelButton_Click(object sender, EventArgs e)
		{
			isCaptchaResultEntered = false;
			continueClicked?.TrySetResult(null);
		}


		private void DisplayError(string Message)
		{
			ErrorLabel.Text = Message;
			ErrorLabel.Visible = true;
		}
	}
}
