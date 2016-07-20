using System.Collections.Generic;
using ProtoBuf;


namespace Paranoid
{
	[ProtoContract]	public class Attachment
	{
		[ProtoMember(1)] public string FileName { get; set; }
		[ProtoMember(2)] public long Size { get; set; }
		[ProtoMember(3)] public byte[] Hash {get; set; }
		[ProtoMember(4)] public FileCompression Compression { get; set; }
		[ProtoMember(5)] public List<long> FileParts { get; set; }

		public AttachmentSource Src;
		public string FullFilePath;
		public List<long> ForwardedParts;
		public bool isAllPartsReceived { get; set; } = true;

		public Attachment()
		{
			FileParts = new List<long>();
		}

		public Attachment Self => this;

		public string FileSizeStr => " ("+Utils.SizeToStr(Size)+")";

		public string FullNameStr => Src == AttachmentSource.File ? FullFilePath : "forwarded:"+FileName;
	}

}