using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Chaos.NaCl;
using HashLib.Crypto.SHA3;

namespace Paranoid
{
	public class WebNode
	{
		public readonly string URL;
		public readonly byte[] PublicKey;
		public readonly int Flags; //for future use

		public WebNode(string URL, byte[] PubKey, int Flags)
		{
			this.URL = URL;
			this.Flags = Flags;
			PublicKey = PubKey;
		}
	}
	public static class WebInfrastructure
	{
		private static readonly List<WebNode> WebNodes=new List<WebNode>();

		private static byte[] WebRequest(WebNode WN, string ControllerName, byte[] RequestData)
		{
			string FullURL=WN.URL+@"/"+ControllerName+@"/?Data="+ Uri.EscapeDataString(Convert.ToBase64String(RequestData));

			byte[] DataFromWeb;

			WebClient WC = new WebClient();

			try
			{
				DataFromWeb = WC.DownloadData(FullURL);
			}
			catch
			{

				return null;
			}
			byte[] FileBytes = new byte[DataFromWeb.Length - 64];
			byte[] Sig = new byte[64];
			Buffer.BlockCopy(DataFromWeb, 0, Sig, 0, 64);
			Buffer.BlockCopy(DataFromWeb, 64, FileBytes, 0, DataFromWeb.Length - 64);
			Blake512 Bl=new Blake512();
			Bl.TransformBytes(RequestData);
			Bl.TransformBytes(FileBytes);
			Bl.TransformBytes(RequestData);
			byte[] Hash = (Bl.TransformFinal()).GetBytes();
			Bl.Initialize();

			return Ed25519.Verify(Sig, Hash, WN.PublicKey) ? FileBytes : null;
		}

		public static byte[] GetDataFromWeb(string ControllerName,byte[] RequestData)
		{

			foreach (WebNode WN in WebNodes)
			{
				byte[] RetVal = WebRequest(WN, ControllerName,RequestData);
				if (RetVal != null) return RetVal;
			}
			return null;
		}

		static WebInfrastructure()
		{
			WebNodes.Add(new WebNode(@"https://paranoid.ym-com.net",Convert.FromBase64String(@"I/YBDFGsjbx2dlDCJi2wJWwGIeKBUkDmrhCA2HjsR04="),0));
		}

	}
}
