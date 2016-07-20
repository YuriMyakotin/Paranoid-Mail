using ProtoBuf;

namespace Paranoid
{
    [ProtoContract] public class User
    {
        [ProtoMember(1)] public long UserID { get; set; }
        [ProtoMember(2)] public byte[] AuthKey { get; set; }
        [ProtoMember(3)] public byte[] EncryptionKey { get; set; }
        public int UserStatus { get; set; }
    }
}
