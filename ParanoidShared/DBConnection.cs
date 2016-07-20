using System;
using System.Data.Common;
using System.Xml.Serialization;
using System.IO;

#if MONO
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
#endif
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace Paranoid
{

	public enum DBType : int
	{
		SQLite=0,
		MSSQL=1,
		MySQL=2
	}

	[Serializable] public class DBConfigData
	{
		public DBType DatabaseType { get; set; }
		public string ConnectionString { get; set; }
	}

	public class DB:IDisposable
	{
		private static string ConnString;
		public static DBType DatabaseType;

		public static bool Init()
		{
			XmlSerializer formatter = new XmlSerializer(typeof (DBConfigData));
			try
			{
				using (FileStream fs = new FileStream(@"dbconfig.xml", FileMode.Open))
				{
					DBConfigData cfg =(DBConfigData)formatter.Deserialize(fs);
					DatabaseType = cfg.DatabaseType;
					ConnString = cfg.ConnectionString;
					return true;
				}
			}
			catch
			{
			   DatabaseType=DBType.SQLite;
				#if MONO
				ConnString= "Data Source=paranoid.db;Version=3;Pooling=True;Max Pool Size=100;Cache Size=1048576;page size=4096;FailIfMissing=True;Journal Mode=WAL;synchronous=NORMAL";
				#else
				ConnString= "DataSource=paranoid.db;Version=3;Pooling=True;Max Pool Size=100;Cache Size=1048576;page size=4096;FailIfMissing=True;Journal Mode=WAL;synchronous=NORMAL";
				#endif
				return false;
			}
		}


		public readonly DbConnection Conn;

		public DB()
		{
			switch (DatabaseType)
			{
				case DBType.SQLite:
					#if MONO
					Conn = new SqliteConnection(ConnString);
					#else
					Conn = new SQLiteConnection(ConnString);
					#endif
					
					break;
				case DBType.MSSQL:
					Conn=new SqlConnection(ConnString);
					break;
				case DBType.MySQL:
					Conn=new MySqlConnection(ConnString);
					break;
			}
			Conn.Open();
		}

		public void Dispose()
		{
			Conn.Close();
			Conn.Dispose();
		}
	}
}
