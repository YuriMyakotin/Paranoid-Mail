using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data;
using System.Linq;
/*

namespace Paranoid
{
    public class ServerIncomingNetworkSession : AnswerNetworkSession
    {

        public ServerIncomingNetworkSession(TcpClient TcpClnt,Config MySrvInfo):base(TcpClnt,(ulong)MySrvInfo.ServerID,0,MySrvInfo.ServerFlags,MySrvInfo.CurrentPrivateKey)
        {

        }


        public override void ProcessRecvMessage()
        {
            base.ProcessRecvMessage();
            if (PTStatus == ParanoidTransportStatus.Disconnected) return;
             switch(SessionStatus)
             {
                 case NetSessionStatus.Started:
                     {
                         switch ((CmdCode)RecvCmdCode)
                         {
                             case CmdCode.CmdLoginRequest:
                                 {
                                     //
                                 }
                                 break;

                             case CmdCode.CmdClientRegistrationRequest:
                                 if ((MyNodeFlags&(int)TypeOfNodeFlags.ClientsAutoRegistrationDisabled)!=0)
                                 {
                                     SendCmd(CmdCode.CmdServiceNotEnabled);
                                     SendCmd(CmdCode.CmdDisconnect);
                                 }
                                 else
                                 {
                                     String CaptchaStr = MakeCaptchaString.MakeCaptchaStr();
                                     byte[] CaptchaImg = MakeRegistrationCaptcha.MakeCaptcha(CaptchaStr);
                                     byte[] ZippedData = SevenZip.Compression.LZMA.SevenZipHelper.Compress(CaptchaImg);
                                     OtherData.Add("Captcha", Encoding.ASCII.GetBytes(CaptchaStr));
                                     SendCmd(CmdCode.CmdCaptchaForRegistration, ZippedData, ZippedData.Length, 0);
                                     //

                                     //begin registration
                                     SessionStatus = NetSessionStatus.ClientRegStarted;
                                     Timeout = 300000;
                                     //begin registration
                                 }
                                 break;

                             case CmdCode.CmdServerRegistrationRequest:
                                 if ((MyNodeFlags&(int)TypeOfNodeFlags.RootServer)==0)
                                 {
                                     SendCmd(CmdCode.CmdServiceNotEnabled);
                                     SendCmd(CmdCode.CmdDisconnect);
                                 }
                                 else
                                 {
                                     String CaptchaStr = MakeCaptchaString.MakeCaptchaStr();
                                     byte[] CaptchaImg = MakeRegistrationCaptcha.MakeCaptcha(CaptchaStr);
                                     byte[] ZippedData = SevenZip.Compression.LZMA.SevenZipHelper.Compress(CaptchaImg);
                                     OtherData.Add("Captcha", Encoding.ASCII.GetBytes(CaptchaStr));
                                     SendCmd(CmdCode.CmdCaptchaForRegistration, ZippedData, ZippedData.Length, 0);
                                     //

                                     //begin registration
                                     SessionStatus = NetSessionStatus.ServerRegStarted;
                                     Timeout = 300000;
                                 }
                                 break;

                             case CmdCode.CmdServersListRequest:
                                 {
                                     ServerCommandsProcessing.SendServersList(this);
                                 }
                                 break;

                             default:
                                 EndSession(NetSessionResult.InvalidData);
                                 break;
                         }
                     }
                     break;

                 case NetSessionStatus.AuthStarted:
                     {
                         //
                     }
                     break;
                 case NetSessionStatus.AuthCompleted:
                     {

                     }
                     break;

                 case NetSessionStatus.ClientRegStarted:
                     {
                         switch ((CmdCode)RecvCmdCode)
                         {
                             case CmdCode.CmdClientRegistrationData:

                                 //
                                 break;


                             default:
                                 EndSession(NetSessionResult.InvalidData);
                                 break;
                         }
                     }
                     break;

                 case NetSessionStatus.ClientRegInProgress:
                     {

                     }
                     break;
                 case NetSessionStatus.ServerRegStarted:
                     {

                     }
                     break;
                 case NetSessionStatus.ServerRegInProgress:
                     {

                     }
                     break;
             }

 	
        }


        

    }
}
*/