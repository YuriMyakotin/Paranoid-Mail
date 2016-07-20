using ProtoBuf;

namespace Paranoid {
	[ProtoContract] public class AddContactRequestData
	{
		[ProtoMember(1)] public byte[] MessageToContact { get; set; }
		[ProtoMember(2)] public byte[] PublicKey { get; set; }
		[ProtoMember(3)] public byte[] SharedKey { get; set; }
		[ProtoMember(4)] public byte[] AuthPubKey { get; set; }
	}

}
