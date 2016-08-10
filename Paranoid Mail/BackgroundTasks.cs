using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Paranoid
{
    public static class BackgroundTasks
    {
        private static readonly List<long> BusyList = new List<long>();
        private static readonly object BusyListLock = new object();

        public static volatile bool isExit=false;


        public static void AddBusy(long ContactID)
        {
            lock (BusyListLock)
            {
                BusyList.Add(ContactID);
            }
        }

        public static bool CheckBusy(long ContactID)
        {
            int idx;
            lock (BusyListLock)
            {
                idx = BusyList.IndexOf(ContactID);
            }
            return idx != -1;
        }

        public static void RemoveBusy(long ContactID)
        {
            lock (BusyListLock)
            {
                BusyList.Remove(ContactID);
            }
        }


        public static void PeriodicTask()
        {
            Random Rnd = new Random();
            while (!isExit)
            {
                CheckSrvListUpdate();
                SendReceiveAll();
                int WaitTime = (int)Utils.GetIntValue("SendReceiveInterval", 180);
                Thread.Sleep(Rnd.Next(WaitTime*800,WaitTime*1200));
            }

        }

        private static void CheckSrvListUpdate()
        {
            long SrvListAge = Utils.GetIntValue("ServerListTimestamp", 0);
            if (LongTime.Now - SrvListAge >= LongTime.Hours(16)) // check if need update serverlist
            {
                long NewTime = LongTime.Now;
                using (GetServerListNetSession NS = new GetServerListNetSession())
                {
                    List<long> SrvIDList = (from p in CryptoData.Accounts select p.ServerID).Distinct().ToList();
                    while (SrvIDList.Count > 0)
                    {
                        long SrvID = SrvIDList.ElementAt(NS.Rnd.Next(0, SrvIDList.Count));
                        if (NS.GetList(SrvID, SrvListAge, 0) == NetSessionResult.Ok)
                        {
                            Utils.UpdateIntValue("ServerListTimestamp", NewTime);
                            return;
                        }
                        SrvIDList.Remove(SrvID);
                    }

                    if (Utils.GetServerListFromWeb())
                    {
                        Utils.UpdateIntValue("ServerListTimestamp", NewTime);
                    }

                }
            }
        }

        public static void SendReceiveAll()
        {
            foreach (Account Acc in CryptoData.Accounts)
            {
                SendReceiveAccount(Acc);
            }
        }

        public static void SendReceiveAccount(Account Acc)
        {
            if (CheckBusy(Acc.AccountID)) return;
            Task CallTask = new Task(() => CallAccountTask(Acc));
            CallTask.Start();
        }

        private static void CallAccountTask(Account Acc)
        {
            AddBusy(Acc.AccountID);
            using (NetSessionClient NSC = new NetSessionClient())
            {
                Acc.LastCallStatus=NetSessionResult.Busy;
                Acc.LastCallStatus = NSC.CallServer(Acc);
            }
            RemoveBusy(Acc.AccountID);
        }

    }
}
