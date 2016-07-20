using ProtoBuf;

namespace Paranoid
{
    [ProtoContract] public class UpdateServerInfo
    {
        [ProtoMember(1)] public long ServerID { get; set; }
        [ProtoMember(2)] public int ServerFlags { get; set; }
        [ProtoMember(3)] public string IP { get; set; }
        [ProtoMember(4)] public int Port { get; set; }
        [ProtoMember(5)] public long CurrentPublicKeyExpirationTime { get; set; }
        [ProtoMember(6)] public byte[] NextPublicKey { get; set; }
        [ProtoMember(7)] public string Comments { get; set; }

        public UpdateServerInfo()
        {
            
        }
        public UpdateServerInfo(Server Srv)
        {
            ServerID = Srv.ServerID;
            ServerFlags = Srv.ServerFlags;
            IP = Srv.IP;
            Port = Srv.Port;
            CurrentPublicKeyExpirationTime = Srv.CurrentPublicKeyExpirationTime;
            NextPublicKey = Srv.NextPublicKey;
            Comments = Srv.Comments;
        }

    }
}
