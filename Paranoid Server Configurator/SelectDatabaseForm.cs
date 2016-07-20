using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
#if MONO
using Mono.Data.Sqlite;
#else
using System.Data.SQLite;
#endif
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace Paranoid
{
	public partial class SelectDatabaseForm : Form
	{
		public DBConfigData DBConfig=new DBConfigData();
		public SelectDatabaseForm()
		{
			InitializeComponent();
			SQLiteRadioButton.Checked = true;
		}

		private void button1_Click(object sender, EventArgs e)
		{

			DBConfig.ConnectionString = ConnStringTextBox.Text;
			try
			{
				DbConnection Conn = null;
				switch (DBConfig.DatabaseType)
				{
					case DBType.SQLite:
					#if MONO
					SqliteConnectionStringBuilder Builder=new SqliteConnectionStringBuilder(DBConfig.ConnectionString);
					string FileName = Builder.DataSource;
					if (!File.Exists(FileName))
					{
						SqliteConnection.CreateFile(FileName);
					}

					Conn=new SqliteConnection(DBConfig.ConnectionString);
				
					
					#else
						SQLiteConnectionStringBuilder Builder = new SQLiteConnectionStringBuilder(DBConfig.ConnectionString);
						string FileName = (string)Builder["DataSource"];
						if (!File.Exists(FileName))
						{
							SQLiteConnection.CreateFile(FileName);
						}

						Conn = new SQLiteConnection(DBConfig.ConnectionString);
					#endif


						break;
					case DBType.MSSQL:
						Conn = new SqlConnection(DBConfig.ConnectionString);
						break;
					case DBType.MySQL:
						Conn = new MySqlConnection(DBConfig.ConnectionString);
						break;
				}
				Conn.Open();
				Conn.Close();
				Conn.Dispose();
				XmlSerializer formatter = new XmlSerializer(typeof(DBConfigData));
				using (FileStream fs = new FileStream(@"dbconfig.xml", FileMode.OpenOrCreate))
				{
					formatter.Serialize(fs,DBConfig);
					fs.Flush();
				}
			}
			catch (Exception Ex)
			{
				ErrorTextBox.Text = Ex.Message;
				return;
			}

			Close();
			DialogResult=DialogResult.OK;
		}

		private void SQLiteRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (SQLiteRadioButton.Checked)
			{
				DBConfig.DatabaseType=DBType.SQLite;
				#if MONO
				ConnStringTextBox.Text ="Data Source=paranoid.db;Version=3;Pooling=True;Max Pool Size=100;Cache Size=1048576;page size=4096;FailIfMissing=True;Journal Mode=WAL;synchronous=NORMAL";
				#else
				ConnStringTextBox.Text ="DataSource=paranoid.db;Version=3;Pooling=True;Max Pool Size=100;Cache Size=1048576;page size=4096;FailIfMissing=True;Journal Mode=WAL;synchronous=NORMAL";
				#endif

			}
		}

		private void MSSQLRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (MSSQLRadioButton.Checked)
			{
				DBConfig.DatabaseType =DBType.MSSQL;
				ConnStringTextBox.Text = "Data Source= ;Initial Catalog= ;User ID= ; Password=; ";
			}
		}

		private void MySQLRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (MySQLRadioButton.Checked)
			{
				DBConfig.DatabaseType =DBType.MySQL;
				ConnStringTextBox.Text =
					"Server= ;Port=3306;Database= ;Uid= ;Pwd= ;CharSet=utf8;";
			}
		}
	}
}
