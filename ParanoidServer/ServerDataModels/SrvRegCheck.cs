using ProtoBuf;

namespace Paranoid
{
    [ProtoContract] public class SrvRegCheck
    {
        [ProtoMember(1)] public long SrvID { get; set; }
        [ProtoMember(2)] public long RegBy { get; set; }
        [ProtoMember(3)] public long RegTime { get; set; }
    }
}
