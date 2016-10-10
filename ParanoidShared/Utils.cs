using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;
using System.IO;
using ProtoBuf;
using SevenZip.Compression.LZMA;


namespace Paranoid
{
	public enum ValueType
	{
		IntType, StringType, BinaryType
	}

	public static class LongTime
	{
		public static long Now => Utils.DateTimeToLong(DateTime.UtcNow);
		public static long Seconds(int Cnt) => (long)Cnt * 1000;
		public static long Minutes(int Cnt) => (long)Cnt * 60000;
		public static long Hours(int Cnt) => (long)Cnt * 3600000;
		public static long Days(int Cnt) => (long)Cnt * 86400000;

		public static string ToLocalTimeStr(long LongTime)
		{
			DateTime dt = (Utils.LongToDateTime(LongTime)).ToLocalTime();
			return dt.ToShortDateString()+" "+dt.ToShortTimeString();

		}
	}


	public static class Utils
	{
		private static readonly object LockObject;

		static Utils()
		{
			LockObject = new object();
		}


		public static long DateTimeToLong(DateTime dt) => (long)(dt.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);

		public static DateTime LongToDateTime(long Time)
		{
			DateTime dt = new DateTime(1970, 1, 1);
			dt = dt.AddMilliseconds(Time);
			return dt;
		}

		public static bool GetServerListFromWeb()
		{
			{
				byte[] Salt = new byte[32];
				ParanoidRNG.GetBytes(Salt);
				byte[] Data = WebInfrastructure.GetDataFromWeb("ServerList",Salt,WebNodeSupportBits.RootServer);
				if (Data == null)
				{
					return false;
				}
				byte[] SrvList = SevenZipHelper.Decompress(Data);
				if (SrvList == null) return false;
				using (MemoryStream MS = new MemoryStream(SrvList))
				{
					return UpdateServersList(MS, 0);
				}


			}
		}
		public static bool UpdateServersList(NetSession NS, long MySrvID)
		{

			try
			{
				bool Ret;
				using (MemoryStream MS = new MemoryStream(NS.RecvBuff))
				{
					Ret = UpdateServersList(MS, MySrvID);
				}
				return Ret;
			}
			catch
			{

				return false;
			}

		}


		public static bool UpdateServersList(MemoryStream S, long MySrvID)
		{
			try
			{
				List<Server> Lst = Serializer.Deserialize<List<Server>>(S);
				using (var DBC=new DB())
				{

					foreach (Server Srv in Lst)
					{
						if (Srv.ServerID != MySrvID)
							Srv.InsertIntoDB(DBC);
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}


		public static long GetIntValue(string Name, long DefaultValue)
		{
			using (DB DBC=new DB())
			{
				try
				{
					return
						DBC.Conn.QuerySingle<long>("Select ValueData from IntValues where ValueName=@ValueName",
							new { ValueName = Name });
				}
				catch
				{
					return DefaultValue;
				}
			}
		}

		public static void UpdateIntValue(string Name, long NewValue)
		{
			using (DB DBC=new DB())
			{
				switch (DB.DatabaseType)
				{
					case DBType.MSSQL:
						DBC.Conn.Execute("Delete from IntValues where ValueName=@ValueName; Insert into IntValues values(@ValueName,@Value)", new { ValueName = Name, Value = NewValue });
						break;
					case DBType.SQLite:
					case DBType.MySQL:
						DBC.Conn.Execute("Replace into IntValues values(@ValueName,@Value)", new { ValueName = Name, Value = NewValue });
						break;
				}
			}
		}

		public static string GetStringValue(string Name)
		{
			using (DB DBC=new DB())
			{

				try
				{
					return
						DBC.Conn.QuerySingle<string>("Select ValueData from StringValues where ValueName=@ValueName",
							new { ValueName = Name });
				}
				catch
				{
					return string.Empty;
				}
			}
		}

		public static void UpdateStringValue(string Name, string NewValue)
		{
			using (DB DBC=new DB())
			{
				switch (DB.DatabaseType)
				{
					case DBType.MSSQL:
						DBC.Conn.Execute("Delete from StringValues where ValueName=@ValueName; Insert into StringValues values(@ValueName,@Value)",new { ValueName = Name, Value = NewValue });
						break;
					case DBType.MySQL:
					case DBType.SQLite:
						DBC.Conn.Execute("Replace into StringValues values(@ValueName,@Value)",new { ValueName = Name, Value = NewValue });
						break;
				}
			}
		}

		public static void DeleteValue(ValueType Type, string Name)
		{
			using (DB DBC=new DB())
			{
				string Command = "Delete from ";

				switch (Type)
				{
					case ValueType.IntType:
						Command += "IntValues";
						break;
					case ValueType.StringType:
						Command += "StringValues";
						break;

					case ValueType.BinaryType:
						Command += "BinaryValues";
						break;
				}
				Command += " where ValueName=@ValueName";
				DBC.Conn.Execute(Command, new { ValueName = Name });
			    if (DB.DatabaseType == DBType.SQLite)
			    {
			        DBC.Conn.Execute("VACUUM");
			    }

			}
		}

		public static byte[] GetBinaryValue(string Name)
		{
			using (DB DBC=new DB())
			{

				try
				{
					return
						DBC.Conn.QuerySingle<byte[]>("Select ValueData from BinaryValues where ValueName=@ValueName",
							new { ValueName = Name });
				}
				catch
				{
					return null;
				}
			}
		}

		public static void UpdateBinaryValue(string Name, byte[] NewValue)
		{
			using (DB DBC=new DB())
			{
				switch (DB.DatabaseType)
				{
					case DBType.MSSQL:
						DBC.Conn.Execute("Delete from BinaryValues where ValueName=@ValueName; Insert into BinaryValues values(@ValueName,@Value)", new { ValueName = Name, Value = NewValue });
						break;
					case DBType.MySQL:
					case DBType.SQLite:
						DBC.Conn.Execute("Replace into BinaryValues values(@ValueName,@Value)", new { ValueName = Name, Value = NewValue });
						break;
				}
			}
		}


		public static long GetRandomRootServerID()
		{

			long CurrentTimeStamp =LongTime.Now;

			using (DB DBC=new DB())
			{

				var SrvList =
					DBC.Conn.Query("Select ServerID,ServerFlags from Servers where HoldUntil<=@TimeNow", new { @TimeNow = CurrentTimeStamp });
				var GCList =
					(from p in SrvList where ((p.ServerFlags & (int)ServerFlagBits.RootServer) != 0) select p.ServerID)
						.ToArray();
				if (GCList.Length == 0) return 0;

				Random Rnd = new Random();


				return GCList[Rnd.Next(GCList.Length)];


			}


		}

		public static List<long> GetRootSrvsList()
		{
			using (var DBC=new DB())
			{

				var SrvList =
				   DBC.Conn.Query("Select ServerID,ServerFlags from Servers");
				return (from p in SrvList where ((p.ServerFlags & (int)ServerFlagBits.RootServer) != 0) select (long)p.ServerID).ToList();
			}
		}

		public static T BytesToObject<T>(byte[] Data)
		{

			using (MemoryStream MS = new MemoryStream(Data))
			{
				try
				{
					return Serializer.Deserialize<T>(MS);
				}
				catch
				{
				   return default(T);
				}

			}
		}

		public static T ZippedBytesToObject<T>(byte[] Data)
		{
			try
			{
				byte[] UnzippedData = null;
				UnzippedData=SevenZipHelper.Decompress(Data);

				using (MemoryStream MS = new MemoryStream(UnzippedData))
				{
					try
					{
						return Serializer.Deserialize<T>(MS);
					}
					catch
					{
						return default(T);
					}
					finally
					{
					   if (UnzippedData!=null) for (int i = 0; i < UnzippedData.Length; i++) UnzippedData[i] = 0;
					}
				}
			}
			catch
			{
				return default(T);
			}
		}

		public static byte[] ObjectToBytes<T>(T Obj)
		{
			using (MemoryStream MS = new MemoryStream())
			{
				Serializer.Serialize<T>(MS, Obj);
				return MS.ToArray();
			}
		}

		public static bool isServerExists(long ServID)
		{
			using (DB DBC=new DB())
			{

				return DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Servers where ServerID=@SrvID", new { SrvID = ServID }) != 0;
			}
		}

		public static Server GetServerInfo(long ServID)
		{
			using (DB DBC=new DB())
			{

				return DBC.Conn.QuerySingleOrDefault<Server>("Select * from Servers where ServerID=@SrvID", new {SrvID=ServID});
			}
		}

		public static byte[] GetSrvPublicKey(Server Srv) => (Srv.CurrentPublicKeyExpirationTime <= LongTime.Now) && (Srv.CurrentPublicKeyExpirationTime != 0)
			? Srv.NextPublicKey
			: Srv.CurrentPublicKey;

		public static byte[] GetSrvPublicKey(long ServID)
		{
			Server Srv = GetServerInfo(ServID);
			return Srv == null ? null : GetSrvPublicKey(Srv);
		}


		public static long MakeMsgID()
		{
			using (DB DBC=new DB())
			{
				lock (LockObject)
				{
					long ID1 =LongTime.Now;

					long ID2 =
						DBC.Conn.QuerySingleOrDefault<long>("Select ValueData from IntValues where ValueName='MsgID'") +
						1;
					if (ID2 > ID1) ID1 = ID2;

					switch (DB.DatabaseType)
					{
						case DBType.MSSQL:
							DBC.Conn.Execute("Delete from IntValues where ValueName='MsgID'; Insert into IntValues values('MsgID',@Value)", new { Value = ID1 });
							break;
						case DBType.SQLite:
						case DBType.MySQL:
							DBC.Conn.Execute("Replace into IntValues values('MsgID',@Value)",new { Value = ID1 });
							break;
					}
					return ID1;
				}

			}
		}

		public static string SizeToStr(long Size)
		{
			string size = "0 Bytes";
			if (Size >= 1073741824)
				 size = $"{Size/1073741824.0:##.##}" + "Gb";
				else if (Size >= 1048576)
					size = $"{Size/1048576.0:##.##}" + "Mb";
				else if (Size >= 1024.0)
					size = $"{Size/1024.0:##.##}" + "Kb";
				else if (Size > 0 && Size < 1024.0)
					size = Size.ToString() + " Bytes";
			return size;

		}

	}
}
