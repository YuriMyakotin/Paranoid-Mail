using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;

namespace Paranoid
{
	public static class Checks
	{
		public static bool CheckPort(string PortName)
		{
			int NewPort;
			if (!int.TryParse(PortName, out NewPort)) return false;
			return (NewPort > 0) && (NewPort < 65536);
		}

		public static bool CheckIP(string IpStr)
		{
			if (IpStr.Length < 1) return false;
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				IPAddress[] Addr = Dns.GetHostAddresses(IpStr);
				return Addr.Length != 0;
			}
			catch
			{
				return false;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

	}
}
