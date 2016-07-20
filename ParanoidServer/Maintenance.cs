using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper;

using static Paranoid.Utils;
using static Paranoid.LongTime;

namespace Paranoid
{
	public static class Maintenance
	{
		public static void MaintenanceThread()
		{
			Random Rnd=new Random();
			while (!Cfg.isStopped)
			{

				Cfg.CheckKeyExpiration();

				if (GetIntValue("isConfigChanged", 0) != 0)
				{
					if (!Cfg.LoadServerConfig())
					{
						Cfg.isStopped = true;
						Cfg.ExitEvent.Set();
						return;
					}
				}
				//

				if (!NetSessionServer.isAnyBusyNow())
				{
					DynamicBlackList.Cleanup();
					using (DB DBC=new DB())
					{

						DBC.Conn.Execute("Delete from Messages where MessageStatus=5");
						DBC.Conn.Execute("Update Messages set MessageStatus=3 where MessageType=0 and MessageStatus=6");
						DBC.Conn.Execute("Update Messages set FromUser=0 where MessageType=0 and MessageStatus=3");
						DBC.Conn.Execute("Delete from Registrations where LastTime<@TimeNow", new {TimeNow = Now});
						if (DB.DatabaseType==DBType.SQLite) DBC.Conn.Execute("Vacuum");
					}

					UpdateType AutoUpdateMode = (UpdateType) GetIntValue("AutoUpdateMode", 2);
					if (AutoUpdateMode != UpdateType.Disabled)
					{
						long LastUpdateCheckTime=GetIntValue("LastUpdateCheckTime",0);
						if (LastUpdateCheckTime + Days(1) <= Now)
						{
							UpdateIntValue("LastUpdateCheckTime",Now+Days(1));
							Version CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
							long Build = CurrentVersion.Build*uint.MaxValue + CurrentVersion.Revision;
							#if MONO
							if (WebUpdate.UpdateApp(18614730117385627, Build, AutoUpdateMode))
							#else
							if (WebUpdate.UpdateApp(4542025256693768561, Build, AutoUpdateMode))
							#endif
							{
								Cfg.ExitEvent.Set();
								return;
							}
						}
					}

					GC.Collect();



				}

				if (Cfg.ServerInfo.isRootServer)
				{
					//maintain root server list
					using (DB DBC=new DB())
					{
						long TimeNow = Now;

						IEnumerable<Server> SrvList =
							DBC.Conn.Query<Server>(
								"Select * from Servers where CurrentPublicKeyExpirationTime<=@Timestamp",
								new {Timestamp = TimeNow});
						foreach (Server Srv in SrvList)
						{
							if (Srv.ServerID != Cfg.ServerInfo.ServerID)
								DBC.Conn.Execute(
									"Update Servers set ServerInfoTime=@TimeStamp,CurrentPublicKey=@Key,CurrentPublicKeyExpirationTime=0,NextPublicKey=NULL where ServerID=@SrvID",
									new {SrvID = Srv.ServerID, Key = Srv.NextPublicKey, TimeStamp = TimeNow});
						}
					}
				}
				else
				{
					//update server list from any root server
					long SrvListAge = GetIntValue("ServerListTimestamp", 0);
					if (Now - SrvListAge >= Hours(6)) // check if need update serverlist
					{
						GetServerListNetSession RSL = new GetServerListNetSession();
						RSL.GetListFromRootServers(Cfg.ServerInfo.ServerID);
					}
				}

				Cfg.MaintenanceEvent.WaitOne(1000*(Rnd.Next(250, 450)));

			}
		}
	}
}
