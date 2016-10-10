
namespace Paranoid
{

	public enum NetSessionResult : int
	{
		Ok = 0,
		CantConnect,
		NetError,
		InvalidData,
		InvalidCredentials,
		Busy,
		InvalidServerID

	}

	public enum CmdCode : byte
	{
		SendingDone = 80,

		ServiceNotEnabled = 90,


		LoginRequest = 100,
		LoginServerSalt = 101,
		LoginCallerInfo = 102,

		LoginAccepted = 103,
		Busy = 104,

		Message=120,
		MessageHeader = 121,
		MessageAccepted = 122,
		MessageRejected = 123,
		MessageDataPart = 124,
		MessageReceived = 125,


		ServersListRequest = 180,
		ServersListPart = 181,


		SrvRegistrationRequest = 200,
		SrvRegistrationCheck=201,
		SrvRegistrationInvalidData=202,
		SrvIDAlreadyTaken=203,
		SrvRegistrationInProgress=204,
		SrvRegistrationDone=205,

		UserRegistrationRequest = 210,
		UserRegistrationTryAgainLater = 211,
		CaptchaForRegistration = 212,
		RegistrationCaptchaReply = 213,
		RegistrationBadCaptcha = 214,
		RegistrationCaptchaOk = 215,
		ClientRegistrationData = 216,
		RegistrationAccepted = 217,
		RegistrationIDAlreadyTaken = 218,
		RegistrationInvalidData = 219

	}

	public enum YesNoToAll : int
	{
		No=0,
		NoToAll=1,
		Yes=2,
		YesToAll=3
	}

	public enum UpdateType : int
	{
		Disabled=0,
		Critical=1,
		Major=2,
		Minor=3
	}

	public static class NetworkVariables
	{
		public static readonly int SocketTimeout;
		public static readonly int SocketBufferSize;
		public static readonly int MaxMessageSize;

		static NetworkVariables()
		{
			SocketTimeout = (int)Utils.GetIntValue("SocketTimeout", 180000);
			SocketBufferSize = (int)Utils.GetIntValue("SocketBufferSize", 32768);
			if ((SocketBufferSize < 8192) || (SocketBufferSize > 131072)) SocketBufferSize = 32768;
			MaxMessageSize = (int)Utils.GetIntValue("MaxMessageSize", 134217728);

		}
	}
}