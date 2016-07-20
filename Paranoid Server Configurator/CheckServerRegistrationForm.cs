using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using static Paranoid.Utils;

namespace Paranoid
{
	public partial class CheckServerRegistrationForm : Form
	{
		public CheckServerRegistrationForm()
		{
			InitializeComponent();
		}

		private void CheckBtn_Click(object sender, EventArgs e)
		{
			ErrorLabel.Text = "";
			ErrorLabel.ForeColor = Color.Red;
			Cursor.Current = Cursors.WaitCursor;
			bool Result = CheckRegistration();
			Cursor.Current = Cursors.Default;
			if (Result)
			{
				DialogResult=DialogResult.OK;
				Close();
			}



		}

		private bool CheckRegistration()
		{
			long RootServerID = GetIntValue("RegistratorID", 0);
			if (RootServerID == 0)
			{
				ErrorLabel.Text = "Invalid configuration";
				return false;
			}
			ServerRegData MySrvRegData;
			using (DB DBC = new DB())
			{
				MySrvRegData = DBC.Conn.QuerySingleOrDefault<ServerRegData>("Select * from NewServerRegData");
			}
			if (MySrvRegData==null)
			{
				ErrorLabel.Text = "Invalid configuration";
				return false;
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
							ErrorLabel.Text = "Can't connect to root server";
							return false;
						case NetSessionResult.InvalidCredentials:
							ErrorLabel.Text = "Security error";
							return false;
						case NetSessionResult.InvalidData:
							ErrorLabel.Text = "Data error";
							return false;
						case NetSessionResult.NetError:
							ErrorLabel.Text = "Network error";
							return false;
					}

				}
				{
					byte[] tmp = new byte[16];
					byte[] tmp1 = BitConverter.GetBytes(MySrvRegData.NewSrvID);
					Buffer.BlockCopy(tmp1, 0, tmp, 0, 8);
					tmp1 = BitConverter.GetBytes(MySrvRegData.RequestID);
					Buffer.BlockCopy(tmp1, 0, tmp, 8, 8);
					Net.SendBuff = NetSession.MakeCmd(CmdCode.SrvRegistrationCheck, tmp,16,0);
					if (!Net.SendEncrypted())
					{
						ErrorLabel.Text="Network Error";
						return false;
					}
					if (!Net.ReceiveCommand())
					{
						ErrorLabel.Text = "Network Error";
						return false;
					}
					switch ((CmdCode) Net.RecvCmdCode)
					{
						case CmdCode.SrvRegistrationInProgress:
							ErrorLabel.Text = "Registration still in progress, check again later";
							ErrorLabel.ForeColor = Color.ForestGreen;
							return false;

						case CmdCode.SrvIDAlreadyTaken:
							using (DB DBC = new DB())
							{
								DBC.Conn.Execute("Delete from IntValues;Delete from BinaryValues;Delete from NewServerRegData;");
							}

							MessageBox.Show("Registration refused - selected server ID already in use. Start registration process again and select another Server ID");
							DialogResult=DialogResult.Cancel;
							Close();
							return false;

						case CmdCode.SrvRegistrationDone:
							Server NewSrv = new Server()
							{
								ServerID = MySrvRegData.NewSrvID,
								ServerFlags = MySrvRegData.Flags,
								ServerInfoTime = LongTime.Now,
								CurrentPublicKey = MySrvRegData.PKey,
								Comments = MySrvRegData.Comments,
								CurrentPublicKeyExpirationTime = MySrvRegData.KeyExpTime,
								IP = MySrvRegData.IP,
								Port = MySrvRegData.Port,
								NextPublicKey = MySrvRegData.NextPKey
							};

							UpdateIntValue("ServerID",NewSrv.ServerID);
							DeleteValue(ValueType.IntType,"RegistratorID");
							using (DB DBC = new DB())
							{
								DBC.Conn.Execute("Delete from NewServerRegData");
								NewSrv.InsertIntoDB(DBC);
							}
							MessageBox.Show("Server registration successfully completed");
							return true;

						default:
							ErrorLabel.Text = "Invalid data";
							return false;
					}


				}


			}


		}
	}
}
