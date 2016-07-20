
using System;
using System.Data.SQLite;
using System.Windows;
using Dapper;

namespace Paranoid
{
	static class DBCreation
	{
		private const string DBCreationStr =@"CREATE TABLE BinaryValues (ValueName string(128) NOT NULL PRIMARY KEY, ValueData blob);
CREATE TABLE IntValues (ValueName string(128) NOT NULL PRIMARY KEY, ValueData bigint NOT NULL);
CREATE TABLE Messages (FromUser bigint NOT NULL,FromServer bigint NOT NULL,MessageID bigint NOT NULL,ToUser bigint NOT NULL,ToServer bigint NOT NULL,MessageType int NOT NULL,MessageStatus int NOT NULL,RecvTime bigint NOT NULL DEFAULT 0,MessageBody blob NOT NULL,	CONSTRAINT sqlite_autoindex_Messages_1 PRIMARY KEY (FromUser, FromServer, MessageID, ToUser, ToServer));
CREATE TABLE Servers (ServerID bigint NOT NULL PRIMARY KEY,ServerFlags int NOT NULL,ServerInfoTime bigint NOT NULL,IP nvarchar(254) NOT NULL,Port int NOT NULL,CurrentPublicKey binary NOT NULL,CurrentPublicKeyExpirationTime bigint NOT NULL DEFAULT 0,NextPublicKey binary,Comments nvarchar(254),LastCallStatus int NOT NULL,FailedCalls int NOT NULL DEFAULT 0,HoldUntil bigint NOT NULL DEFAULT 0);
CREATE TABLE StringValues (ValueName string(128) NOT NULL PRIMARY KEY,ValueData string NOT NULL);";

		public static bool CreateDB()
		{
			try
			{
				SQLiteConnection.CreateFile("Paranoid.db");
				using (DB DBC = new DB())
				{
					DBC.Conn.Execute(DBCreationStr);
					DBC.Conn.Close();
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		public static bool isDBExists()
		{
			try
			{
				using (DB DBC=new DB())
				{

					DBC.Conn.Execute("Select count(*) from Servers");
					DBC.Conn.Execute("Select count(*) from IntValues");
					DBC.Conn.Execute("Select count(*) from StringValues");
					DBC.Conn.Execute("Select count(*) from BinaryValues");
					DBC.Conn.Execute("Select count(*) from Messages");
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

	}
}
