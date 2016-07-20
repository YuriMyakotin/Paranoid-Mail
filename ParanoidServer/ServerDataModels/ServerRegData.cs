namespace Paranoid
{
    public class ServerRegData
    {
        public long RequestID { get; set; }
        public long RegTime { get; set; }
        public long NewSrvID { get; set; }
        public int Flags { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public byte[] PKey { get; set; }
        public long KeyExpTime { get; set; }
        public byte[] NextPKey { get; set; }
        public string Comments { get; set; }
        public int AwaitRepliesCount { get; set; }

        public ServerRegData()
        {

        }
        public ServerRegData(Server Srv,long Time)
        {
            RequestID = Utils.MakeMsgID();
            RegTime = Time;
            NewSrvID = Srv.ServerID;
            Flags = Srv.ServerFlags;
            IP = Srv.IP;
            Port = Srv.Port;
            PKey = Srv.CurrentPublicKey;
            KeyExpTime = Srv.CurrentPublicKeyExpirationTime;
            NextPKey = Srv.NextPublicKey;
            Comments = Srv.Comments;
        }

    }
}
