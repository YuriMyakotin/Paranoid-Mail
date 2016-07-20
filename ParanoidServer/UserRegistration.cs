
using System;
using System.Net;
using Dapper;
using static Paranoid.Utils;


namespace Paranoid
{
	public partial class NetSessionServer : NetSessionExtended
	{
		private NetSessionResult RegisterUser()
		{
			switch (RegistrationMode)
			{

				case AutoRegistrationModes.Disabled:
					SendBuff = MakeCmd(CmdCode.ServiceNotEnabled);
					return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;

				case AutoRegistrationModes.EnabledWithTimeout:
				{
					long Time = LongTime.Now;
					int Cnt;
					using (DB DBC = new DB())
					{
						Cnt = DBC.Conn.QuerySingleOrDefault<int>(
							"Select count(*) from Registrations where IpHash=@Hash and LastTime>=@TimeNow",
							new { Hash = AddrHash, TimeNow = Time });
						switch (DB.DatabaseType)
						{
							case DBType.SQLite:
							case DBType.MySQL:
								DBC.Conn.Execute("Replace into Registrations values(@Hash,@TimeNow)",
									new { Hash = AddrHash, TimeNow = Time + Cfg.SameIpRegistrationTimeout });
								break;
							case DBType.MSSQL:
								DBC.Conn.Execute(
									"Delete from Registrations where IpHash=@Hash; Insert into Registrations values(@Hash,@TimeNow)",
									new { Hash = AddrHash, TimeNow = Time + Cfg.SameIpRegistrationTimeout });
								break;
						}
					}
					if (Cnt != 0)
					{
						SendBuff = MakeCmd(CmdCode.UserRegistrationTryAgainLater);
						return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
					}
					break;
				}
			}

			bool isCapchaOk;

			if (CaptchaChecking(out isCapchaOk)!=NetSessionResult.Ok) return NetSessionResult.NetError;
			if (!isCapchaOk) return NetSessionResult.Ok; //bad captcha, but no net error


			if (!ReceiveCommand()) return EndResult;
			if (RecvCmdCode != (byte) CmdCode.ClientRegistrationData) return NetSessionResult.InvalidData;

			User NewUser = BytesToObject<User>(RecvBuff);
			if (NewUser == null) return NetSessionResult.InvalidData;

			if ((NewUser.UserID == 0) || (NewUser.AuthKey.Length != 32) || (NewUser.EncryptionKey.Length != 128))
			{
				SendBuff = MakeCmd(CmdCode.RegistrationInvalidData);

				return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
			}

			if (isUserExists(NewUser.UserID))
			{
				SendBuff = MakeCmd(CmdCode.RegistrationIDAlreadyTaken);
				return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
			}
			using (var DBC=new DB())
			{

				DBC.Conn.Execute("Insert into Users values (@UserID,@AuthKey,@EncryptionKey,0)", NewUser);

			}
			SendBuff = MakeCmd(CmdCode.RegistrationAccepted);
			return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
		}

	}
}