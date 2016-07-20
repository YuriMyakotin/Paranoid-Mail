using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;

using System.Linq;
using Dapper;
using Chaos.NaCl;
using static Paranoid.Utils;
using static Paranoid.LongTime;


namespace Paranoid
{

	public static class Cfg
	{
		public static readonly object LockObj=new object();
		public static volatile bool isStopped=false;
		public static byte[] CurrentPrivateKey;
		public static byte[] NextPrivateKey;
		public static Server ServerInfo;
		public static List<ListenPorts> BindingsList;

		public static long DynamicBlackListTime;
		public static int SameIpRegistrationTimeout;

		public static long LastServerConfigTime;

		public static readonly EventWaitHandle ConfigLoadedEvent = new AutoResetEvent(false);
		public static readonly EventWaitHandle MaintenanceEvent=new AutoResetEvent(false);
		public static readonly EventWaitHandle CallGenerationEvent = new AutoResetEvent(false);
		public static EventWaitHandle ExitEvent;

		public static bool LoadServerConfig()
		{
			lock (LockObj)
			{
				UpdateIntValue("isConfigChanged", 0);

				long ServerID = GetIntValue("ServerID", 0);
				if (ServerID == 0) return false;

				CurrentPrivateKey = GetBinaryValue("CurrentPrivateKey");
				if (CurrentPrivateKey?.Length != 32) return false;

				NextPrivateKey = GetBinaryValue("NextPrivateKey");

				ServerInfo = GetServerInfo(ServerID);
				if (ServerInfo == null) return false;

				DynamicBlackListTime = GetIntValue("DynamicBlackListTime", 30000);

				SameIpRegistrationTimeout = (int) GetIntValue("SameIpRegistrationTimeout", 120000);

				LastServerConfigTime = GetIntValue("ServerInfoUpdatedTime", 0);

				if (ServerInfo.ServerInfoTime > LastServerConfigTime)
				{
					//send server info update

					UpdateMyServerInfo();
					LastServerConfigTime = ServerInfo.ServerInfoTime;
					UpdateIntValue("ServerInfoUpdatedTime", LastServerConfigTime);
				}

				using (DB DBC=new DB())
				{

					BindingsList = DBC.Conn.Query<ListenPorts>("Select * from ListenPorts").ToList();
				}

				BindingsList.Add(new ListenPorts
				{
					Ip = ServerInfo.IP,
					Port = ServerInfo.Port,
					AutoRegistration =
						ServerInfo.isAutoRegistrationEnabled ? (int) AutoRegistrationModes.EnabledWithTimeout : 0
				});
			}


			ConfigLoadedEvent.Set();
			return true;

		}

		public static void CheckKeyExpiration()
		{
			long CurrentTime =Now;
			if (CurrentTime < ServerInfo.CurrentPublicKeyExpirationTime) return;
			lock (LockObj)
			{
				CurrentPrivateKey = NextPrivateKey;
				NextPrivateKey = new byte[32];
				ParanoidRNG.GetBytes(NextPrivateKey);

				ServerInfo.CurrentPublicKey = ServerInfo.NextPublicKey;
				ServerInfo.CurrentPublicKeyExpirationTime = DateTimeToLong((DateTime.Today).AddDays(90));
				ServerInfo.ServerInfoTime = CurrentTime;
				ServerInfo.NextPublicKey = Ed25519.PublicKeyFromSeed(NextPrivateKey);

				LastServerConfigTime = ServerInfo.ServerInfoTime;
				UpdateBinaryValue("CurrentPrivateKey", CurrentPrivateKey);
				UpdateBinaryValue("NextPrivateKey", NextPrivateKey);
				UpdateIntValue("ServerInfoUpdatedTime", LastServerConfigTime);
				using (DB DBC=new DB())
				{

					DBC.Conn.Execute(
							"Update Servers set ServerInfoTime=@ServerInfoTime, CurrentPublicKey=@CurrentPublicKey, CurrentPublicKeyExpirationTime=@CurrentPublicKeyExpirationTime, NextPublicKey=@NextPublicKey where ServerID=@ServerID",
							ServerInfo);
				}
				UpdateMyServerInfo();
			}
		}


	    private static void UpdateMyServerInfo()
		{
		    if (ServerInfo.isRootServer)
		    {
		        List<long> GCServers = GetRootSrvsList();
		        foreach (long SrvID in GCServers)
		        {
		            if (SrvID != ServerInfo.ServerID)
		                Message.PostMessage(0, ServerInfo.ServerID, 0, SrvID, (int) MsgType.UpdateServerInfo,
		                    ObjectToBytes<UpdateServerInfo>(new UpdateServerInfo(ServerInfo)));
		        }
		    }
		    else
		        Message.PostMessage(0, ServerInfo.ServerID, 0, 0, (int) MsgType.UpdateServerInfo,
		            ObjectToBytes<UpdateServerInfo>(new UpdateServerInfo(ServerInfo)));
		}
	}


}
