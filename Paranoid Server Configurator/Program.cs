using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;

namespace Paranoid
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				if (!DB.Init())
				{
					SelectDatabaseForm SDF = new SelectDatabaseForm();
					if (SDF.ShowDialog() == DialogResult.Cancel) return;
				}
			}
			catch(Exception Ex)
			{
				MessageBox.Show(Ex.Message);
			}

			if (!DBCreation.isTablesExists())
			{
				using (DB DBC = new DB())
				{
					try
					{
						switch (DB.DatabaseType)
						{
							case DBType.SQLite:
								DBC.Conn.Execute(DBCreation.DBCreationString_SQLite);
								break;
							case DBType.MSSQL:
								DBC.Conn.Execute(DBCreation.DBCreationString_MSSQL);
								break;
							case DBType.MySQL:
								DBC.Conn.Execute(DBCreation.DBCreationString_MySQL);
								break;
						}
					}
					catch (Exception Ex)
					{
						MessageBox.Show(Ex.Message);
						return;
					}
				}

			}




			using (DB DBC = new DB())
			{
				if (DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Servers") == 0)
				{
					if (!Utils.GetServerListFromWeb())
					{
						MessageBox.Show("Error - can't download list of servers");
						return;
					}
				}
				DBC.Conn.Execute("Update Servers set HoldUntil=0");
			}

			long ServerID = Utils.GetIntValue("ServerID", 0);
			if (ServerID == 0)
			{
				long RootServerID = Utils.GetIntValue("RegistratorID", 0);
				if (RootServerID == 0)
				{ if (!RegisterServer.NewRegistration()) return; }
				else
				 if (!RegisterServer.CheckRegistration()) return;

				ServerID = Utils.GetIntValue("ServerID", 0);
			};

			if (!Cfg.LoadCfg(ServerID))
			{

				MessageBox.Show("Error: configuration invalid. Exiting.");
				return;

			};


			Application.Run(new MainForm());
		}
	}
}
