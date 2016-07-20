using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Paranoid.Utils;
using Dapper;


namespace Paranoid
{
    public static class Cfg
    {
        public static byte[] CurrentPrivateKey;
        public static byte[] NextPrivateKey;
        public static Server ServerInfo;
        public static IEnumerable<ListenPorts> BindingsList;
        public static int RegistrationTimeout;

        public static List<NamedValue> RegistrationModes;

        public static bool LoadCfg(long ServerID)
        {
            CurrentPrivateKey = GetBinaryValue("CurrentPrivateKey");
            if (CurrentPrivateKey?.Length != 32) return false;

            NextPrivateKey = GetBinaryValue("NextPrivateKey");

            ServerInfo = GetServerInfo(ServerID);
            if (ServerInfo == null) return false;

            RegistrationTimeout = (int)GetIntValue("SameIpRegistrationTimeout", 120000);

            using (var DBC=new DB())
            {

                BindingsList = DBC.Conn.Query<ListenPorts>("Select * from ListenPorts");
            }

            RegistrationModes = new List<NamedValue>
            {
                new NamedValue(0, "Disabled"),
                new NamedValue(1, "Enabled with timeout"),
                new NamedValue(2, "Enabled without timeout")
            };

            return true;
        }
    }
}
