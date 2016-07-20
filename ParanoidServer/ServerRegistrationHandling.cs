using System;
using System.Collections.Generic;

using System.Linq;
using System.Net;
using Dapper;
using static Paranoid.Utils;
using static Paranoid.LongTime;

namespace Paranoid
{

	public partial class NetSessionServer : NetSessionExtended
	{
		private NetSessionResult RegisterServer()
		{
			if (!Cfg.ServerInfo.isRootServer)
			{
				SendBuff = MakeCmd(CmdCode.ServiceNotEnabled);

				return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
			}

			Server NewSrv = BytesToObject<Server>(RecvBuff);
			if (NewSrv == null) return NetSessionResult.InvalidData;

			bool isCapchaOk;

			if (CaptchaChecking(out isCapchaOk) != NetSessionResult.Ok) return NetSessionResult.NetError;
			if (!isCapchaOk) return NetSessionResult.Ok;


			long TimeNow =Now;
			if (!ValidateServerInfo(NewSrv,TimeNow))
			{
				SendBuff = MakeCmd(CmdCode.SrvRegistrationInvalidData);
				return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
			}

			using (DB DBC=new DB())
			{

				if ((DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Servers where ServerID=@SrvID", new {SrvID = NewSrv.ServerID})!=0)
					||(DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from NewServerRegData where NewSrvID=@SrvID", new { SrvID = NewSrv.ServerID })!= 0)
					||(DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from SrvRegCheck where SrvID=@SrvID", new { SrvID = NewSrv.ServerID }) != 0))
				{
					SendBuff = MakeCmd(CmdCode.SrvIDAlreadyTaken);
					return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
				}

				List<long> RootServers = (from p in DBC.Conn.Query("Select ServerID,ServerFlags from Servers") where ((p.ServerFlags & (int)ServerFlagBits.RootServer) != 0) select (long)p.ServerID).ToList();
				if (RootServers.Count ==1)
				{
					NewSrv.InsertIntoDB(DBC);
					SendBuff = MakeCmd(CmdCode.SrvRegistrationDone);
					return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
				}

				ServerRegData RegData = new ServerRegData(NewSrv, TimeNow) {AwaitRepliesCount = RootServers.Count - 1};

				DBC.Conn.Execute("Insert Into NewServerRegData values(@RequestID,@RegTime,@NewSrvID,@Flags,@IP,@Port,@Pkey,@KeyExpTime,@NextPKey,@Comments,@AwaitRepliesCount)", RegData);
				SrvRegCheck SRCH = new SrvRegCheck
				{
					SrvID = NewSrv.ServerID,
					RegBy = Cfg.ServerInfo.ServerID,
					RegTime = TimeNow
				};


				byte[] Msg = ObjectToBytes<SrvRegCheck>(SRCH);
				foreach (long SrvID in RootServers)
				{
					if (SrvID!= Cfg.ServerInfo.ServerID) Message.PostMessage(0, Cfg.ServerInfo.ServerID,0,SrvID,(int)MsgType.ServerRegistrationCheck,Msg);
				}

				SendBuff = MakeCmd(CmdCode.SrvRegistrationInProgress,RegData.RequestID);
				return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
			}


		}

		private static bool ValidateServerInfo(Server Srv,long TimeNow)
		{


			if ((Srv.ServerID >= 0) && (Srv.ServerID <= 10000)) return false;

			if ((Srv.CurrentPublicKey.Length != 32) || (Srv.NextPublicKey.Length != 32) ||
				(Srv.CurrentPublicKeyExpirationTime <= TimeNow + Days(1)))
				return false;

			if (Srv.isRootServer) return false;

			if ((Srv.Port <= 0) || (Srv.Port > 65535)) return false;
			try
			{
				IPAddress[] Addr = Dns.GetHostAddresses(Srv.IP);
				if (Addr.Length == 0) return false;
			}
			catch
			{
				return false;
			}

			Srv.ServerInfoTime = TimeNow;

			return true;
		}

		private NetSessionResult ServerRegCheck()
		{
			if (!Cfg.ServerInfo.isRootServer)
			{
				SendBuff = MakeCmd(CmdCode.ServiceNotEnabled);

				return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
			}
			if (RecvCmdSize!=16) return NetSessionResult.InvalidData;
			long SrvID = BitConverter.ToInt64(RecvBuff, 0);
			long RegID = BitConverter.ToInt64(RecvBuff, 8);
			using (DB DBC=new DB())
			{

				long TimeNow =Now;
				ServerRegData RegData =
					DBC.Conn.QuerySingleOrDefault<ServerRegData>("Select * from NewServerRegData where RequestID=@RequestID AND NewSrvID=@NewSrvID",new {RequestID=RegID,NewSrvID=SrvID});
				if (RegData==null) return NetSessionResult.InvalidData;

				if (RegData.AwaitRepliesCount > 0) //
				{//still in progress
					SendBuff = MakeCmd(CmdCode.SrvRegistrationInProgress);
					return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
				}

				DBC.Conn.Execute(
					"Delete from NewServerRegData where RequestID=@RequestID AND NewSrvID=@NewSrvID", new {RequestID = RegID, NewSrvID = SrvID});

				if (RegData.AwaitRepliesCount < 0)
				{// denied
					SendBuff = MakeCmd(CmdCode.SrvIDAlreadyTaken);
					return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
				}

				//allowed
				Server NewSrv = new Server()
				{
					ServerID = RegData.NewSrvID,
					ServerFlags = RegData.Flags,
					ServerInfoTime = TimeNow,
					CurrentPublicKey = RegData.PKey,
					Comments = RegData.Comments,
					CurrentPublicKeyExpirationTime = RegData.KeyExpTime,
					IP = RegData.IP,
					Port = RegData.Port,
					NextPublicKey = RegData.NextPKey
				};
				NewSrv.InsertIntoDB(DBC);
				DBC.Conn.Execute("Delete from SrvRegCheck where SrvID=@ServerID", new { ServerID = SrvID });
				List<long> RootServers = (from p in DBC.Conn.Query("Select ServerID,ServerFlags from Servers") where ((p.ServerFlags & (int)ServerFlagBits.RootServer) != 0) select (long)p.ServerID).ToList();
				byte[] Data = ObjectToBytes<Server>(NewSrv);
				foreach (long RootSrvID in RootServers)
				{
					if (RootSrvID != Cfg.ServerInfo.ServerID) Message.PostMessage(0, Cfg.ServerInfo.ServerID, 0, RootSrvID, (int)MsgType.ServerRegistered, Data);
				}


				SendBuff = MakeCmd(CmdCode.SrvRegistrationDone);
				return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Ok;
			}
		}

		private static bool CheckRegistration(SrvRegCheck RegCheck)
		{
			using (var DBC=new DB())
			{


				if (DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Servers where ServerID=@SrvID", RegCheck)!= 0) return false;

				int RegTime =
					DBC.Conn.QueryFirstOrDefault<int>("Select RegTime from NewServerRegData where NewSrvID=@SrvID", RegCheck);

				if (RegTime != 0)
				{
					if (RegTime > RegCheck.RegTime)
						DBC.Conn.Execute("Update NewServerRegData set AwaitRepliesCount=-1 where NewSrvID=@SrvID", RegCheck);
					else
						return false;
				}


				SrvRegCheck ExistingCheck=DBC.Conn.QuerySingleOrDefault<SrvRegCheck>("Select * from SrvRegCheck where SrvID=@SrvID", RegCheck);

				if (ExistingCheck==null)
				{
					DBC.Conn.Execute("Insert into SrvRegCheck values(@SrvID,@RegBy,@RegTime)", RegCheck);
					return true;
				}
				if (ExistingCheck.RegTime <= RegCheck.RegTime) return false;

				DBC.Conn.Execute("Update SrvRegCheck set RegBy=@RegBy, RegTime=@RegTime where SrvID=@SrvID", RegCheck);
				Message.PostMessage(0, Cfg.ServerInfo.ServerID, 0, ExistingCheck.RegBy, (int)MsgType.ServerRegistrationReplyNo, BitConverter.GetBytes(RegCheck.SrvID));
				return true;
			}


		}
	}
}