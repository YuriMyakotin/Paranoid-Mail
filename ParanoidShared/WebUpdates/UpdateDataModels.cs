using ProtoBuf;

namespace Paranoid
{
	[ProtoContract] public class CheckUpdateDataModel
	{
		[ProtoMember(1)] public long ProductID { get; set; }
		[ProtoMember(2)] public long CurrentBuild { get; set; }
		[ProtoMember(3)] public int CheckType { get; set; }
		[ProtoMember(4)] public byte[] SaltBytes { get; set; }
	}


	[ProtoContract] public class UpdateInfoDataModel
	{
		[ProtoMember(1)] public long BuildNumber { get; set; }
		[ProtoMember(2)] public string VersionName { get; set; }
		[ProtoMember(3)] public UpdateType BuildType { get; set; }
		[ProtoMember(4)] public string BuildDescription { get; set; }


	}
}