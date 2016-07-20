using System;

namespace Paranoid
{
	public partial class NetSessionServer : NetSessionExtended
	{
		private NetSessionResult CaptchaChecking(out bool isOk)
		{
			int CaptchaValue;
			isOk = false;
			Sock.SendTimeout = NetworkVariables.TimeoutInteractive;
			Sock.ReceiveTimeout = NetworkVariables.TimeoutInteractive;

			{
				int A = Rnd.Next(0, 100);
				int B = Rnd.Next(0, 100);
				int C = Rnd.Next(-100, 100);
				byte[] CompressedImg =
					SevenZip.Compression.LZMA.SevenZipHelper.Compress(
						MakeCaptchaImage.MakeCaptcha(MakeCaptchaString.CaptchaStr(A, B, (C >= 0))));
				if (C < 0) CaptchaValue = A - B;
				else CaptchaValue = A + B;
				SendBuff = MakeCmd(CmdCode.CaptchaForRegistration, CompressedImg, CompressedImg.Length, 0);
			}

			if (!SendEncrypted()) return NetSessionResult.NetError;

			if (!ReceiveCommand()) return EndResult;
			if ((RecvCmdCode != (byte)CmdCode.RegistrationCaptchaReply) || (RecvCmdSize != sizeof(int)))
				return NetSessionResult.InvalidData;

			int ReceivedCapchaValue = BitConverter.ToInt32(RecvBuff, 0);
			if (ReceivedCapchaValue != CaptchaValue)
			{
				SendBuff = MakeCmd(CmdCode.RegistrationBadCaptcha);
				if (!SendEncrypted()) return NetSessionResult.NetError;
				return NetSessionResult.Ok;
			}
			SendBuff = MakeCmd(CmdCode.RegistrationCaptchaOk);
			if (!SendEncrypted()) return NetSessionResult.NetError;
			isOk = true;
			return NetSessionResult.Ok;
		}
	}
}