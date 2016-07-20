using System;
using System.Threading;
using System.Threading.Tasks;

using Dapper;


namespace Paranoid
{


	static partial class Program
	{


		static void Main(string[] args)
		{

			DB.Init();

			try
			{
				using (DB DBC = new DB())
				{

					DBC.Conn.Execute("Select count(*) from Servers");
					DBC.Conn.Execute("Select count(*) from IntValues");
					DBC.Conn.Execute("Select count(*) from StringValues");
					DBC.Conn.Execute("Select count(*) from BinaryValues");
					DBC.Conn.Execute("Select count(*) from Messages");
					DBC.Conn.Execute("Select count(*) from ListenPorts");
					DBC.Conn.Execute("Select count(*) from NewServerRegData");
					DBC.Conn.Execute("Select count(*) from SrvRegCheck");
					DBC.Conn.Execute("Select count(*) from Registrations");
				}

			}
			catch
			{
				Console.WriteLine("Database error - server not configured");
				return;
			}

			if (!Cfg.LoadServerConfig())
			{
				Console.WriteLine("Server not configured. Please use Paranoid Server Configurator tool");
				return;
			}
			if (EventWaitHandle.TryOpenExisting(((ulong) Cfg.ServerInfo.ServerID).ToString() + "_ExitEvent",
				out Cfg.ExitEvent))
			{
				Console.WriteLine("Another server process with same ServerID already running, exiting");
				Cfg.ExitEvent.Close();
				return;
			}


			Cfg.ExitEvent= new EventWaitHandle(false, EventResetMode.ManualReset, ((ulong)Cfg.ServerInfo.ServerID).ToString()+"_ExitEvent");


			using (DB DBC=new DB())
			{

				DBC.Conn.Execute("Update Servers set HoldUntil=0");
			}


			Task MaintenanceTask = new Task(Maintenance.MaintenanceThread);
			MaintenanceTask.Start();
			Task ListenPortsTask = new Task(ListenThread);
			ListenPortsTask.Start();
			Task CallGeneraionTask = new Task(CallGeneration.CallGenerationThread);
			CallGeneraionTask.Start();


			Cfg.ExitEvent.WaitOne();

		}



	}
}
