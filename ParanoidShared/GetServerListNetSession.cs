using System;

using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paranoid
{
    public class GetServerListNetSession:NetSession
    {
        public NetSessionResult GetList(long SrvID, long RequestedTime,long MySrvID)
        {

            EndResult = Connect(SrvID);

            if (EndResult == NetSessionResult.Ok)
            {
                byte[] tmp = BitConverter.GetBytes(RequestedTime);
                SendBuff = MakeCmd(CmdCode.ServersListRequest, tmp, sizeof (long), 0);

                if (!SendEncrypted())
                {
                    EndResult = NetSessionResult.NetError;
                    goto endlabel;
                }
                while (ReceiveCommand())
                {

                    switch ((CmdCode)RecvCmdCode)
                    {
                        case CmdCode.SendingDone:
                            EndResult=NetSessionResult.Ok;
                            goto endlabel;
                        case CmdCode.ServersListPart:
                            if (!Utils.UpdateServersList(this, MySrvID))
                            {
                                EndResult = NetSessionResult.InvalidData;
                                goto endlabel;
                            };
                            break;
                        default:
                            EndResult=NetSessionResult.InvalidData;
                            goto endlabel;

                    }

                }





            }



            endlabel:

            try
            {
                TcpClnt.Close();
            }
            catch { }

            return EndResult;
        }





        public bool GetListFromRootServers(long MySrvID)
        {
            NetSessionResult result;
            long RequestedTime = Utils.GetIntValue("ServerListTimestamp", 0);
            long NewTime = LongTime.Now;


            do
            {
                long SrvID = Utils.GetRandomRootServerID();
                if (SrvID == 0) return false;

                result = GetList(SrvID, RequestedTime, MySrvID);

                AfterSession(SrvID,result);

            } while (result!=NetSessionResult.Ok);

            Utils.UpdateIntValue("ServerListTimestamp",NewTime);

            return true;
        }




    }
}
