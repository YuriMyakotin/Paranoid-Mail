using System;
using System.Linq;
using Dapper;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace Paranoid
{
	public class RoutedMsgInfo
	{
		public long FromUser { get; set; } //hops count
		public long FromServer { get; set; }
		public long MessageID { get; set; }
		public long ToServer { get; set; }
	}

	public static class CallGeneration
	{

		public static void CallGenerationThread()
		{
			Random Rnd = new Random();
			while (!Cfg.isStopped)
			{

				using (DB DBC=new DB())
				{




					List<RoutedMsgInfo> MessagesToBeRouted =
						DBC.Conn.Query<RoutedMsgInfo>(
							"Select FromUser,FromServer,MessageID,ToServer from Messages where MessageStatus=3 and MessageType=0 and FromUser=0")
							.ToList();

					if (MessagesToBeRouted.Count() != 0)
					{
						IEnumerable<dynamic> SrvList=DBC.Conn.Query("Select ServerID,ServerFlags from Servers where HoldUntil<=@TimeNow",new {TimeNow=LongTime.Now});
						long[] Relays =(from p in SrvList where ((p.ServerFlags & (int) ServerFlagBits.RelayingEnabled) != 0)
								select (long) p.ServerID).ToArray();

						foreach (RoutedMsgInfo RoutedMsg in MessagesToBeRouted)
						{
							long FromUser;
							if ((Relays.Length >= 3)&&(RoutedMsg.FromUser<6))
							{
								int R = Rnd.Next(0, 100);
								int N = (int) (RoutedMsg.FromUser)*20 + 10;
								FromUser = N >= R ? Relays[Rnd.Next(0, int.MaxValue)% Relays.Length] : RoutedMsg.ToServer;
							}
							else FromUser = RoutedMsg.ToServer;
							DBC.Conn.Execute("Update Messages set FromUser=@FromUser where FromServer=@FromServer and MessageID=@MessageID",
								new {FromUser = FromUser, FromServer = RoutedMsg.FromServer, MessageID = RoutedMsg.MessageID});

						}
					}


					//
					IEnumerable<long> CallList = DBC.Conn.Query<long>("Select distinct FromUser from Messages where MessageStatus=3 and MessageType=0");
					foreach (long SrvID in CallList)
					{
						if (SrvID == 0) continue;
						Server Srv =
						   DBC.Conn.QuerySingleOrDefault<Server>("Select * from Servers where ServerID=@SrvID", new { SrvID = SrvID });
						if ((Srv!=null)&&Srv.HoldUntil <= LongTime.Now)
						{
							Task CallTask = new Task(() => CallProc(Srv));
							CallTask.Start();
						}
					}

					CallList = DBC.Conn.Query<long>("Select distinct ToServer from Messages where MessageStatus=3 and MessageType!=0");
					foreach (long SrvID in CallList)
					{
						long ServerID = SrvID == 0 ? Utils.GetRandomRootServerID() : SrvID;
						if (ServerID == Cfg.ServerInfo.ServerID) continue;
						if (NetSessionServer.CheckBusy(ServerID)) continue;

						Server Srv =
							DBC.Conn.QuerySingleOrDefault<Server>("Select * from Servers where ServerID=@SrvID", new {SrvID = ServerID});

						if (Srv == null || Srv.HoldUntil > LongTime.Now) continue;
						Task CallTask = new Task(() => CallProc(Srv));
						CallTask.Start();
					}




				}

				Cfg.CallGenerationEvent.WaitOne(1000 * (Rnd.Next(15, 50)));
			}
		}



		private static void CallProc(Server Srv)
		{
			NetSessionServer NSS=new NetSessionServer();
			NetSessionServer.AddBusy(Srv.ServerID);
			NSS.DataCall(Srv);
			NetSessionServer.RemoveBusy(Srv.ServerID);
		}
	}
}
