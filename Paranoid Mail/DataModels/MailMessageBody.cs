using System.Collections.Generic;
using ProtoBuf;


namespace Paranoid
{

	[ProtoContract]
	public class MailMessageBody
	{
		[ProtoMember(1)] public long SendTime { get; set; }
		[ProtoMember(2)] public string Subject { get; set; }
		[ProtoMember(3)] public MessageTextFormat TextFormat { get; set; }
		[ProtoMember(4)] public string MessageText { get; set; }
		[ProtoMember(5)] public long MessageFlags { get; set; }
		[ProtoMember(6)] public List<Attachment> FileAttachments { get; set; }
		public MailMessageBody()
		{
			FileAttachments = new List<Attachment>();
		}
	}

}
