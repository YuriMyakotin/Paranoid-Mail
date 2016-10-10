using System;
using System.Windows;
using System.IO;
using Dapper;
using System.Reflection;
using static Paranoid.LongTime;
using static Paranoid.Utils;

namespace Paranoid
{
	public static class Startup
	{
		public static bool Start()
		{
			try
			{
				if (!DBCreation.isDBExists())
				{
					if (!DBCreation.CreateDB())
					{
						MessageBox.Show("Error - can't create database");
						return false;
					}

				}
				UpdateType AutoUpdateMode = (UpdateType) GetIntValue("AutoUpdateMode", 3);
				if (AutoUpdateMode != UpdateType.Disabled)
				{
					long LastUpdateCheckTime = GetIntValue("LastUpdateCheckTime", 0);
					if (LastUpdateCheckTime + Days(1) <= Now)
					{

						UpdateIntValue("LastUpdateCheckTime", Now + Days(1));
						Version CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
						long Build = CurrentVersion.Build*uint.MaxValue + CurrentVersion.Revision;

						UpdateInfoDataModel UIDM = WebUpdate.CheckForUpdate(2422857701752703557, Build, AutoUpdateMode);
						if (UIDM != null)
						{
							if (MessageBox.Show(
								"New version availiable:\r\n" + UIDM.VersionName + "\r\n" + UIDM.BuildDescription +
								"\r\nDo you want install it now?",
								"Program update", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
							{
								if (WebUpdate.UpdateApp(2422857701752703557, Build, AutoUpdateMode))
								{
									return false;
								}
								;
								MessageBox.Show("Update failed");
							}
						}
					}
				}



			}
			catch (Exception Ex)
			{
				MessageBox.Show(Ex.Message);
			}

			using (DB DBC=new DB())
			{

				if (DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Servers") == 0)
				{
					if (!GetServerListFromWeb())
					{
						MessageBox.Show("Error - can't download list of servers");
						return false;
					}
				}
			}


		startlabel:
				CryptoData.FileName = GetStringValue("KeyFileName");
				bool? result;
				if (CryptoData.FileName == string.Empty)
				{
					FirstTimeQuestionWindow FTQW = new FirstTimeQuestionWindow();
					result = FTQW.ShowDialog();
					if ((result == null) || (result == false))
					{
						return false;
					}

					if (FTQW.isCreateNew)
					{
						using (DB DBC=new DB())
						{
						    if (DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Messages") != 0)
						    {

						        if (MessageBox.Show("Do you want to delete old messages?", "", MessageBoxButton.YesNo,
						                MessageBoxImage.Question) == MessageBoxResult.Yes)
						        {
						            DBC.Conn.Execute("Delete from Messages");
						            if (DB.DatabaseType == DBType.SQLite) DBC.Conn.Execute("Vacuum");
						        }
						    }
						}

						SetPasswordWindow SPW = new SetPasswordWindow(true);
						result = SPW.ShowDialog();
						if ((result == null) || (result == false))
						{
							return false;
						}


						SetKeyFileLocationDialog SKFLD = new SetKeyFileLocationDialog();
						result = SKFLD.ShowDialog();
						if ((result == null) || (result == false))
						{
							return false;
						}

						return true;
					}

					OpenExistingKeyFileDialog OEKFD = new OpenExistingKeyFileDialog();
					result = OEKFD.ShowDialog();
					if ((result == null) || (result == false)) return false;
				}
				else
				{
				    if (CryptoData.FileName != "::DB")
				    {
				        while (!File.Exists(CryptoData.FileName))
				        {
				            OpenExistingKeyFileDialog OEKFD = new OpenExistingKeyFileDialog();
				            result = OEKFD.ShowDialog();
				            if ((result == null) || (result == false))
				            {
				                DeleteValue(ValueType.StringType, "KeyFileName");
				                goto startlabel;
				            }
				        }
				    }
				}


				EnterPasswordDialog EPD = new EnterPasswordDialog();
				result = EPD.ShowDialog();
				if ((result == null) || (result == false))
				{
					return false;
				}

			loadkeys_label:

				switch (CryptoData.LoadKeys())
				{
					case LoadKeysResult.Ok:
						break;
					case LoadKeysResult.FileOpeningError:
					case LoadKeysResult.InvalidFileSize:
						OpenExistingKeyFileDialog OEKFD = new OpenExistingKeyFileDialog();
						result = OEKFD.ShowDialog();
						if ((result == null) || (result == false))
						{
							DeleteValue(ValueType.StringType, "KeyFileName");
							goto startlabel;
						}
						goto loadkeys_label;
					case LoadKeysResult.InvalidData:
						MessageBox.Show("Error: incorrect password or keys file damaged, exiting");
						return false;
				}
			return true;
		}

		public static void GetUnreadCounts()
		{
			using (var DBC=new DB())
			{

				foreach (Account Acc in CryptoData.Accounts)
					foreach (Contact Cnt in Acc.Contacts)
						Cnt.UnreadMessages = DBC.Conn.QuerySingleOrDefault<int>("Select Count(*) from Messages where MessageStatus=1 and MessageType=10 and FromServer=@AccID and FromUser=@CntID", new {AccID=Acc.AccountID,CntID=Cnt.ContactID});
			}
		}
	}
}
