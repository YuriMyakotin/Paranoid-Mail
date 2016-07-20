using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Paranoid
{
	struct IpListenPort
	{
		public TcpListener L;
		public AutoRegistrationModes RegMode;
		public string PortPassword;
	}

	static partial class Program
	{
		public static List<TcpListener> Listeners=new List<TcpListener>();
		public static void ListenThread()
		{
			while (!Cfg.isStopped)
			{
				Cfg.ConfigLoadedEvent.WaitOne();
				lock (Cfg.LockObj)
				{
					foreach (var L in Listeners)
					{
						try
						{
							L.Stop();

						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
						}
					}
					Listeners.Clear();
				}


				lock (Cfg.LockObj)
				{
					foreach (ListenPorts Binding in Cfg.BindingsList)
					{
						try
						{
							IPAddress[] Addr = Dns.GetHostAddresses(Binding.Ip);
							foreach (IPAddress IpAddr in Addr)
							{
								IpListenPort ILP = new IpListenPort
								{
									L = new TcpListener(new IPEndPoint(IpAddr, Binding.Port)),
									RegMode = (AutoRegistrationModes) Binding.AutoRegistration,
									PortPassword = Binding.PrivatePortPassword
								};

								ILP.L.Start();
								ILP.L.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), ILP);
								Console.WriteLine("Listen on " + IpAddr.ToString() + ":" + Binding.Port.ToString());
								Listeners.Add(ILP.L);
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
						}
					}
				}
				if (Listeners.Count == 0)
				{
					Console.WriteLine("Error: no listen ports");
					Cfg.isStopped = true;
					Cfg.ExitEvent.Set();
					return;
				}



			}

		}

		private static void AcceptTcpClientCallback(IAsyncResult ar)
		{
			try
			{
				IpListenPort ILP = (IpListenPort) ar.AsyncState;

				TcpClient client = ILP.L.EndAcceptTcpClient(ar);

				Task SessionTask = new Task(() => AnswerSessionThreadProc(client,ILP));
				SessionTask.Start();

				ILP.L.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), ILP);
			}
			catch
			{
				;
			}
		}

		private static void AnswerSessionThreadProc(TcpClient TcpConn,IpListenPort ILP)
		{
			using (NetSessionServer Srv = new NetSessionServer())
			{
				Srv.TcpClnt = TcpConn;
				Srv.RegistrationMode = ILP.RegMode;
				Srv.PortPassword = ILP.PortPassword;
				Srv.Answer();
			}
		}
	}
}