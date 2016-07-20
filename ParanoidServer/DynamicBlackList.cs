using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paranoid
{

	public class BlacklistRecord
	{
		public long IpHash { get; set; }
		public long HoldUntil { get; set; }

	}
	public static class DynamicBlackList
	{
		private static readonly object LockObj=new object();
		private static readonly List<BlacklistRecord> Records=new List<BlacklistRecord>();

		public static bool CheckIP(long IpHash)
		{
			long TimeNow = LongTime.Now;
			lock (LockObj)
			{
				BlacklistRecord BR = Records.SingleOrDefault(p => p.IpHash == IpHash && p.HoldUntil >= TimeNow);
				if (BR == null) return false;
				Records.Remove(BR);
				BR.HoldUntil = TimeNow + Cfg.DynamicBlackListTime;
				Records.Add(BR);
				return true;
			}
		}


		public static void AddIP(long IpHash)
		{
			long TimeNow = LongTime.Now;
			lock (LockObj)
			{
				BlacklistRecord BR = Records.SingleOrDefault(p => p.IpHash == IpHash && p.HoldUntil >= TimeNow);
				if (BR != null) Records.Remove(BR);

				Records.Add(new BlacklistRecord
				{
					IpHash = IpHash,
					HoldUntil = TimeNow + Cfg.DynamicBlackListTime
				});
			}
		}

		public static void Cleanup()
		{
			lock (LockObj)
			{
				long TimeNow = LongTime.Now;

				foreach (BlacklistRecord BR in Records)
				{
					if (BR.HoldUntil<=TimeNow) Records.Remove(BR);
				}
			}
		}
	}
}
