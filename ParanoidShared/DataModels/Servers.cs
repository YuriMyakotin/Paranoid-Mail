

using System.Data.Common;
using Dapper;
using ProtoBuf;
namespace Paranoid
{
    public enum ServerFlagBits : int
    {
        RootServer = 1,
        RelayingEnabled = 2,
        UsersRegistrationEnabled = 4
    }
    [ProtoContract] public class Server
    {
        [ProtoMember(1)] public long ServerID { get; set; }
        [ProtoMember(2)] public int ServerFlags { get; set; }
        [ProtoMember(3)] public long ServerInfoTime { get; set; }
        [ProtoMember(4)] public string IP { get; set; }
        [ProtoMember(5)] public int Port { get; set; }
        [ProtoMember(6)] public byte[] CurrentPublicKey { get; set; }
        [ProtoMember(7)] public long CurrentPublicKeyExpirationTime { get; set; }
        [ProtoMember(8)] public byte[] NextPublicKey { get; set; }
        [ProtoMember(9)] public string Comments { get; set; }
        public int LastCallStatus { get; set; }
        public int FailedCalls { get; set; }
        public long HoldUntil { get; set; }
        public string ServerNameStr => Comments.Length > 0 ? Comments : ((ulong) ServerID).ToString();
        public string IpAdressStr => IP + ":" + Port.ToString();

        public bool isRootServer => (ServerFlags & (int) ServerFlagBits.RootServer) != 0;

        public bool isAutoRegistrationEnabled => (ServerFlags & (int)ServerFlagBits.UsersRegistrationEnabled) != 0;

        public bool isRelayingSupported => (ServerFlags & (int)ServerFlagBits.RelayingEnabled) != 0;

        public void InsertIntoDB(DB DBC)
        {
            switch (DB.DatabaseType)
            {
                case DBType.MSSQL:
                    DBC.Conn.Execute("Delete from Servers where ServerID=@ServerID; Insert into Servers Values(@ServerID,@ServerFlags,@ServerInfoTime,@Ip,@Port,@CurrentPublicKey,@CurrentPublicKeyExpirationTime,@NextPublicKey,@Comments,0,0,0)", this);
                    break;
                case DBType.SQLite:
                case DBType.MySQL:
                    DBC.Conn.Execute("Replace into Servers Values(@ServerID,@ServerFlags,@ServerInfoTime,@Ip,@Port,@CurrentPublicKey,@CurrentPublicKeyExpirationTime,@NextPublicKey,@Comments,0,0,0)", this);
                    break;
            }

        }

        public void InsertIntoDB()
        {
            using (var DBC=new DB())
            {

                InsertIntoDB(DBC);
            }
        }

    }
}
