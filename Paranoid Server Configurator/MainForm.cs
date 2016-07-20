using System;
using Dapper;

using System.Drawing;
using System.Net;
using System.Windows.Forms;
using System.Text;
using Chaos.NaCl;


namespace Paranoid
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			IDLabel.Text = ((ulong) Cfg.ServerInfo.ServerID).ToString();
			DescriptionTextBox.Text = Cfg.ServerInfo.Comments;
			IpTextBox.Text = Cfg.ServerInfo.IP;
			PortTextBox.Text = Cfg.ServerInfo.Port.ToString();
			RegistrationCheckBox.Checked = Cfg.ServerInfo.isAutoRegistrationEnabled;
			RelayCheckBox.Checked = Cfg.ServerInfo.isRelayingSupported;

			RegTimeout.Value = (int)(Cfg.RegistrationTimeout/1000);

			autoRegistrationDataGridViewTextBoxColumn.DataSource = Cfg.RegistrationModes;

			BindingSource source = new BindingSource(Cfg.BindingsList, null);

			BindingsDataGridView.DataSource = source;

			DateTime dt = Utils.LongToDateTime(Cfg.ServerInfo.CurrentPublicKeyExpirationTime).ToLocalTime();
			KeyExpirationDate.Text= dt.ToShortDateString();
			if ((dt - DateTime.Now).Days < 2) NewKeyButton.Enabled = false;



		}

		private void SaveButton_Click(object sender, System.EventArgs e)
		{


			if (!Checks.CheckPort(PortTextBox.Text))
			{
				PortTextBox.BackColor = Color.Red;
				Cursor.Current = Cursors.Default;
				return;
			}
			PortTextBox.BackColor = Color.White;
			if (!Checks.CheckIP(IpTextBox.Text))
			{
				IpTextBox.BackColor = Color.Red;
				Cursor.Current = Cursors.Default;
				return;
			}
			IpTextBox.BackColor = Color.White;

			Cfg.ServerInfo.IP = IpTextBox.Text;
			Cfg.ServerInfo.Port = int.Parse(PortTextBox.Text);
			Cfg.ServerInfo.Comments = DescriptionTextBox.Text;
			Cfg.ServerInfo.ServerInfoTime = LongTime.Now;

			if (RelayCheckBox.Checked)
				Cfg.ServerInfo.ServerFlags |= (int)ServerFlagBits.RelayingEnabled;
			else Cfg.ServerInfo.ServerFlags &= (~(int)ServerFlagBits.RelayingEnabled);

			if (RegistrationCheckBox.Checked)
				Cfg.ServerInfo.ServerFlags |= (int)ServerFlagBits.UsersRegistrationEnabled;
			else Cfg.ServerInfo.ServerFlags &= (~(int)ServerFlagBits.UsersRegistrationEnabled);

			Utils.UpdateIntValue("SameIpRegistrationTimeout",(long)RegTimeout.Value*1000);

			using (DB DBC=new DB())
			{

				DBC.Conn.Execute("Delete from ListenPorts");
				foreach (ListenPorts LP in Cfg.BindingsList)
				{
					DBC.Conn.Execute("Insert into ListenPorts values(@Ip,@Port,@AutoRegistration,@PrivatePortPassword)", LP);
				}
			}

			Cfg.ServerInfo.InsertIntoDB();
			Utils.UpdateIntValue("isConfigChanged",1);

		}



		private void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void NewKeyButton_Click(object sender, EventArgs e)
		{
			NewKey KeyDialog = new NewKey
			{
				ExpirationDatePicker =
				{
					MaxDate = Utils.LongToDateTime(Cfg.ServerInfo.CurrentPublicKeyExpirationTime).ToLocalTime(),
					MinDate = DateTime.Now.Date.AddDays(2)
				}
			};
			if (KeyDialog.ShowDialog() != DialogResult.OK) return;

			Cfg.NextPrivateKey=new byte[32];
			ParanoidRNG.GetBytes(Cfg.NextPrivateKey,0,32,Encoding.UTF8.GetBytes(KeyDialog.RandomDataTextBox.Text));

			Cfg.ServerInfo.CurrentPublicKeyExpirationTime = Utils.DateTimeToLong(KeyDialog.ExpirationDatePicker.Value.ToUniversalTime());
			Cfg.ServerInfo.ServerInfoTime = LongTime.Now;
			Cfg.ServerInfo.NextPublicKey = Ed25519.PublicKeyFromSeed(Cfg.NextPrivateKey);
			Utils.UpdateBinaryValue("NextPrivateKey",Cfg.NextPrivateKey);
			Cfg.ServerInfo.InsertIntoDB();
			Utils.UpdateIntValue("isConfigChanged", 1);
			DateTime dt = Utils.LongToDateTime(Cfg.ServerInfo.CurrentPublicKeyExpirationTime).ToLocalTime();
			KeyExpirationDate.Text = dt.ToShortDateString();
			if ((dt - DateTime.Now).Days < 2) NewKeyButton.Enabled = false;
		}

		/*private void BindingsDataGridView_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			DataGridViewRow row =BindingsDataGridView.Rows[e.RowIndex];
			if (!CheckPort(row.Cells[1]..Value.ToString()))
			{
				e.Cancel = true;
				row.Cells[1].ErrorText = "Port value must be between 1 and 65535";
				return;
			}

		}*/

		private void BindingsDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
		   BindingsDataGridView.Rows[e.RowIndex].ErrorText = "";
			switch (e.ColumnIndex)
			{
				case 0:
					if (!Checks.CheckIP(e.FormattedValue.ToString()))
					{
						e.Cancel = true;
						BindingsDataGridView.Rows[e.RowIndex].ErrorText = "Invalid IP Address";
					}
					return;
				case 1:
					if (!Checks.CheckPort(e.FormattedValue.ToString()))
					{
						e.Cancel = true;
						BindingsDataGridView.Rows[e.RowIndex].ErrorText = "Port value must be between 1 and 65535";
					}
					return;
				default:
					return;
			}
		}


	}
}
