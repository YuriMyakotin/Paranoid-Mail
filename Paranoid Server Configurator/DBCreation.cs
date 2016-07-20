using System.Data.Common;
using Dapper;

namespace Paranoid
{
	static class DBCreation
	{
		public const string DBCreationString_SQLite =
			@"CREATE TABLE BinaryValues (ValueName string(128) NOT NULL PRIMARY KEY, ValueData blob);
			CREATE TABLE IntValues (ValueName string(128) NOT NULL PRIMARY KEY, ValueData bigint NOT NULL);
			CREATE TABLE ListenPorts (Ip string(254) NOT NULL, Port int NOT NULL, AutoRegistration int NOT NULL DEFAULT 0, PrivatePortPassword string(128), CONSTRAINT sqlite_autoindex_ListenPorts_1 PRIMARY KEY(Ip, Port));
			CREATE TABLE Messages (FromUser bigint NOT NULL, FromServer bigint NOT NULL, MessageID bigint NOT NULL, ToUser bigint NOT NULL, ToServer bigint NOT NULL, MessageType int NOT NULL, MessageStatus int NOT NULL, RecvTime bigint NOT NULL DEFAULT 0, MessageBody blob NOT NULL, CONSTRAINT sqlite_autoindex_Messages_1 PRIMARY KEY(FromUser, FromServer, MessageID, ToUser, ToServer));
			CREATE TABLE NewServerRegData (RequestID bigint NOT NULL PRIMARY KEY, RegTime bigint NOT NULL, NewSrvID bigint NOT NULL, Flags int NOT NULL, IP string(254) NOT NULL,  Port int NOT NULL, PKey blob NOT NULL, KeyExpTime bigint NOT NULL DEFAULT 0, NextPKey blob, Comments string(254), AwaitRepliesCount int NOT NULL DEFAULT 0);
			CREATE TABLE Registrations (IpHash bigint NOT NULL PRIMARY KEY, LastTime bigint NOT NULL);
			CREATE TABLE Servers (ServerID bigint NOT NULL PRIMARY KEY, ServerFlags int NOT NULL, ServerInfoTime bigint NOT NULL, IP string(254) NOT NULL, Port int NOT NULL, CurrentPublicKey blob NOT NULL, CurrentPublicKeyExpirationTime bigint NOT NULL DEFAULT 0, NextPublicKey blob, Comments string(254), LastCallStatus int NOT NULL, FailedCalls int NOT NULL DEFAULT 0, HoldUntil bigint NOT NULL DEFAULT 0);
			CREATE TABLE SrvRegCheck (SrvID bigint NOT NULL PRIMARY KEY, RegBy bigint NOT NULL, RegTime bigint NOT NULL);
			CREATE TABLE StringValues (ValueName  string(128) NOT NULL PRIMARY KEY, ValueData string NOT NULL);
			CREATE TABLE Users (UserID bigint NOT NULL PRIMARY KEY, AuthKey blob NOT NULL, EncryptionKey blob NOT NULL, UserStatus int NOT NULL DEFAULT 0);";

		public const string DBCreationString_MSSQL =
		   @"CREATE TABLE BinaryValues (ValueName nvarchar(128) NOT NULL PRIMARY KEY, ValueData varbinary(MAX));
			CREATE TABLE IntValues (ValueName nvarchar(128) NOT NULL PRIMARY KEY, ValueData bigint NOT NULL);
			CREATE TABLE ListenPorts (Ip nvarchar(254) NOT NULL, Port int NOT NULL, AutoRegistration int NOT NULL DEFAULT 0, PrivatePortPassword nvarchar(128), CONSTRAINT sqlite_autoindex_ListenPorts_1 PRIMARY KEY(Ip, Port));
			CREATE TABLE Messages (FromUser bigint NOT NULL, FromServer bigint NOT NULL, MessageID bigint NOT NULL, ToUser bigint NOT NULL, ToServer bigint NOT NULL, MessageType int NOT NULL, MessageStatus int NOT NULL, RecvTime bigint NOT NULL DEFAULT 0, MessageBody varbinary(MAX) NOT NULL, CONSTRAINT sqlite_autoindex_Messages_1 PRIMARY KEY(FromUser, FromServer, MessageID, ToUser, ToServer));
			CREATE TABLE NewServerRegData (RequestID bigint NOT NULL PRIMARY KEY, RegTime bigint NOT NULL, NewSrvID bigint NOT NULL, Flags int NOT NULL, IP nvarchar(254) NOT NULL,  Port int NOT NULL, PKey varbinary(1024) NOT NULL, KeyExpTime bigint NOT NULL DEFAULT 0, NextPKey varbinary(1024), Comments nvarchar(254), AwaitRepliesCount int NOT NULL DEFAULT 0);
			CREATE TABLE Registrations (IpHash bigint NOT NULL PRIMARY KEY, LastTime bigint NOT NULL);
			CREATE TABLE Servers (ServerID bigint NOT NULL PRIMARY KEY, ServerFlags int NOT NULL, ServerInfoTime bigint NOT NULL, IP nvarchar(254) NOT NULL, Port int NOT NULL, CurrentPublicKey varbinary(1024) NOT NULL, CurrentPublicKeyExpirationTime bigint NOT NULL DEFAULT 0, NextPublicKey varbinary(1024), Comments nvarchar(254), LastCallStatus int NOT NULL, FailedCalls int NOT NULL DEFAULT 0, HoldUntil bigint NOT NULL DEFAULT 0);
			CREATE TABLE SrvRegCheck (SrvID bigint NOT NULL PRIMARY KEY, RegBy bigint NOT NULL, RegTime bigint NOT NULL);
			CREATE TABLE StringValues (ValueName  nvarchar(128) NOT NULL PRIMARY KEY, ValueData nvarchar(1024) NOT NULL);
			CREATE TABLE Users (UserID bigint NOT NULL PRIMARY KEY, AuthKey varbinary(1024) NOT NULL, EncryptionKey varbinary(1024) NOT NULL, UserStatus int NOT NULL DEFAULT 0);";


		public const string DBCreationString_MySQL =
			@"CREATE TABLE BinaryValues (ValueName varchar(128) NOT NULL PRIMARY KEY, ValueData blob);
			CREATE TABLE IntValues (ValueName varchar(128) NOT NULL PRIMARY KEY, ValueData bigint NOT NULL);
			CREATE TABLE ListenPorts (Ip varchar(254) NOT NULL, Port int NOT NULL, AutoRegistration int NOT NULL DEFAULT 0, PrivatePortPassword varchar(128), CONSTRAINT sqlite_autoindex_ListenPorts_1 PRIMARY KEY(Ip, Port));
			CREATE TABLE Messages (FromUser bigint NOT NULL, FromServer bigint NOT NULL, MessageID bigint NOT NULL, ToUser bigint NOT NULL, ToServer bigint NOT NULL, MessageType int NOT NULL, MessageStatus int NOT NULL, RecvTime bigint NOT NULL DEFAULT 0, MessageBody longblob NOT NULL, CONSTRAINT sqlite_autoindex_Messages_1 PRIMARY KEY(FromUser, FromServer, MessageID, ToUser, ToServer));
			CREATE TABLE NewServerRegData (RequestID bigint NOT NULL PRIMARY KEY, RegTime bigint NOT NULL, NewSrvID bigint NOT NULL, Flags int NOT NULL, IP varchar(254) NOT NULL,  Port int NOT NULL, PKey blob NOT NULL, KeyExpTime bigint NOT NULL DEFAULT 0, NextPKey blob, Comments varchar(254), AwaitRepliesCount int NOT NULL DEFAULT 0);
			CREATE TABLE Registrations (IpHash bigint NOT NULL PRIMARY KEY, LastTime bigint NOT NULL);
			CREATE TABLE Servers (ServerID bigint NOT NULL PRIMARY KEY, ServerFlags int NOT NULL, ServerInfoTime bigint NOT NULL, IP varchar(254) NOT NULL, Port int NOT NULL, CurrentPublicKey blob NOT NULL, CurrentPublicKeyExpirationTime bigint NOT NULL DEFAULT 0, NextPublicKey blob, Comments varchar(254), LastCallStatus int NOT NULL, FailedCalls int NOT NULL DEFAULT 0, HoldUntil bigint NOT NULL DEFAULT 0);
			CREATE TABLE SrvRegCheck (SrvID bigint NOT NULL PRIMARY KEY, RegBy bigint NOT NULL, RegTime bigint NOT NULL);
			CREATE TABLE StringValues (ValueName  varchar(128) NOT NULL PRIMARY KEY, ValueData varchar(1024) NOT NULL);
			CREATE TABLE Users (UserID bigint NOT NULL PRIMARY KEY, AuthKey blob NOT NULL, EncryptionKey blob NOT NULL, UserStatus int NOT NULL DEFAULT 0)";


		public static bool isTablesExists()
		{
			try
			{
				using (var DBC = new DB())
				{

					DBC.Conn.Execute("Select count(*) from Servers");
					DBC.Conn.Execute("Select count(*) from IntValues");
					DBC.Conn.Execute("Select count(*) from StringValues");
					DBC.Conn.Execute("Select count(*) from BinaryValues");
					DBC.Conn.Execute("Select count(*) from Messages");
					DBC.Conn.Execute("Select count(*) from ListenPorts");
					DBC.Conn.Execute("Select count(*) from NewServerRegData");
					DBC.Conn.Execute("Select count(*) from SrvRegCheck");
					DBC.Conn.Execute("Select count(*) from Registrations");
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
