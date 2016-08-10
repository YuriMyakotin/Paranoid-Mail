namespace Paranoid
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.IDLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DescriptionTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.RegTimeout = new System.Windows.Forms.NumericUpDown();
            this.TimeoutLabel = new System.Windows.Forms.Label();
            this.RegistrationCheckBox = new System.Windows.Forms.CheckBox();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.RelayCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.IpTextBox = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.NewKeyButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.KeyExpirationDate = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.BindingsDataGridView = new System.Windows.Forms.DataGridView();
            this.ipDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.portDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.autoRegistrationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.namedValueBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.privatePortPasswordDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.listenPortsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RegTimeout)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.namedValueBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.listenPortsBindingSource)).BeginInit();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(18, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 36);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID:";
            //
            // IDLabel
            //
            this.IDLabel.AutoSize = true;
            this.IDLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.IDLabel.Location = new System.Drawing.Point(240, 9);
            this.IDLabel.MinimumSize = new System.Drawing.Size(400, 0);
            this.IDLabel.Name = "IDLabel";
            this.IDLabel.Size = new System.Drawing.Size(400, 36);
            this.IDLabel.TabIndex = 1;
            this.IDLabel.UseMnemonic = false;
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.DescriptionTextBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 63);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(842, 51);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server description (optional)";
            //
            // DescriptionTextBox
            //
            this.DescriptionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.DescriptionTextBox.Location = new System.Drawing.Point(12, 21);
            this.DescriptionTextBox.Name = "DescriptionTextBox";
            this.DescriptionTextBox.Size = new System.Drawing.Size(813, 22);
            this.DescriptionTextBox.TabIndex = 0;
            //
            // groupBox2
            //
            this.groupBox2.Controls.Add(this.RegTimeout);
            this.groupBox2.Controls.Add(this.TimeoutLabel);
            this.groupBox2.Controls.Add(this.RegistrationCheckBox);
            this.groupBox2.Controls.Add(this.PortTextBox);
            this.groupBox2.Controls.Add(this.RelayCheckBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.IpTextBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 120);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(841, 89);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Server information";
            //
            // RegTimeout
            //
            this.RegTimeout.Location = new System.Drawing.Point(431, 52);
            this.RegTimeout.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.RegTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RegTimeout.Name = "RegTimeout";
            this.RegTimeout.Size = new System.Drawing.Size(197, 22);
            this.RegTimeout.TabIndex = 5;
            this.RegTimeout.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            //
            // TimeoutLabel
            //
            this.TimeoutLabel.AutoSize = true;
            this.TimeoutLabel.Location = new System.Drawing.Point(9, 54);
            this.TimeoutLabel.Name = "TimeoutLabel";
            this.TimeoutLabel.Size = new System.Drawing.Size(416, 17);
            this.TimeoutLabel.TabIndex = 4;
            this.TimeoutLabel.Text = "Timeout between new user registrations from same IP (seconds):";
            //
            // RegistrationCheckBox
            //
            this.RegistrationCheckBox.AutoSize = true;
            this.RegistrationCheckBox.Location = new System.Drawing.Point(643, 53);
            this.RegistrationCheckBox.Name = "RegistrationCheckBox";
            this.RegistrationCheckBox.Size = new System.Drawing.Size(189, 21);
            this.RegistrationCheckBox.TabIndex = 1;
            this.RegistrationCheckBox.Text = "Auto registration enabled";
            this.RegistrationCheckBox.UseVisualStyleBackColor = true;
            //
            // PortTextBox
            //
            this.PortTextBox.Location = new System.Drawing.Point(558, 21);
            this.PortTextBox.MaxLength = 8;
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(70, 22);
            this.PortTextBox.TabIndex = 3;
            //
            // RelayCheckBox
            //
            this.RelayCheckBox.AutoSize = true;
            this.RelayCheckBox.Location = new System.Drawing.Point(643, 23);
            this.RelayCheckBox.Name = "RelayCheckBox";
            this.RelayCheckBox.Size = new System.Drawing.Size(140, 21);
            this.RelayCheckBox.TabIndex = 0;
            this.RelayCheckBox.Text = "Relaying enabled";
            this.RelayCheckBox.UseVisualStyleBackColor = true;
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(514, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Port:";
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Public IP:";
            //
            // IpTextBox
            //
            this.IpTextBox.Location = new System.Drawing.Point(75, 21);
            this.IpTextBox.MaxLength = 255;
            this.IpTextBox.Name = "IpTextBox";
            this.IpTextBox.Size = new System.Drawing.Size(433, 22);
            this.IpTextBox.TabIndex = 0;
            //
            // SaveButton
            //
            this.SaveButton.Location = new System.Drawing.Point(635, 517);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(112, 36);
            this.SaveButton.TabIndex = 5;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            //
            // CloseButton
            //
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(743, 517);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(112, 36);
            this.CloseButton.TabIndex = 6;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CancelButton_Click);
            //
            // groupBox4
            //
            this.groupBox4.Controls.Add(this.NewKeyButton);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.KeyExpirationDate);
            this.groupBox4.Location = new System.Drawing.Point(12, 452);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(842, 59);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Current server key";
            //
            // NewKeyButton
            //
            this.NewKeyButton.Location = new System.Drawing.Point(405, 18);
            this.NewKeyButton.Name = "NewKeyButton";
            this.NewKeyButton.Size = new System.Drawing.Size(420, 35);
            this.NewKeyButton.TabIndex = 2;
            this.NewKeyButton.Text = "Force key expiration and generate new key...";
            this.NewKeyButton.UseVisualStyleBackColor = true;
            this.NewKeyButton.Click += new System.EventHandler(this.NewKeyButton_Click);
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(8, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 20);
            this.label4.TabIndex = 1;
            this.label4.Text = "Key expires:";
            //
            // KeyExpirationDate
            //
            this.KeyExpirationDate.AutoSize = true;
            this.KeyExpirationDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.KeyExpirationDate.Location = new System.Drawing.Point(125, 24);
            this.KeyExpirationDate.Name = "KeyExpirationDate";
            this.KeyExpirationDate.Size = new System.Drawing.Size(0, 20);
            this.KeyExpirationDate.TabIndex = 0;
            //
            // groupBox5
            //
            this.groupBox5.Controls.Add(this.BindingsDataGridView);
            this.groupBox5.Location = new System.Drawing.Point(12, 215);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(841, 239);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Additional bindings";
            //
            // BindingsDataGridView
            //
            this.BindingsDataGridView.AutoGenerateColumns = false;
            this.BindingsDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.BindingsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.BindingsDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.BindingsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.BindingsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BindingsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ipDataGridViewTextBoxColumn,
            this.portDataGridViewTextBoxColumn,
            this.autoRegistrationDataGridViewTextBoxColumn,
            this.privatePortPasswordDataGridViewTextBoxColumn});
            this.BindingsDataGridView.DataSource = this.listenPortsBindingSource;
            this.BindingsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BindingsDataGridView.Location = new System.Drawing.Point(3, 18);
            this.BindingsDataGridView.Name = "BindingsDataGridView";
            this.BindingsDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.BindingsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.BindingsDataGridView.RowTemplate.Height = 24;
            this.BindingsDataGridView.Size = new System.Drawing.Size(835, 218);
            this.BindingsDataGridView.TabIndex = 0;
            this.BindingsDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.BindingsDataGridView_CellValidating);
            //
            // ipDataGridViewTextBoxColumn
            //
            this.ipDataGridViewTextBoxColumn.DataPropertyName = "Ip";
            this.ipDataGridViewTextBoxColumn.HeaderText = "Ip";
            this.ipDataGridViewTextBoxColumn.MaxInputLength = 254;
            this.ipDataGridViewTextBoxColumn.Name = "ipDataGridViewTextBoxColumn";
            this.ipDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ipDataGridViewTextBoxColumn.Width = 340;
            //
            // portDataGridViewTextBoxColumn
            //
            this.portDataGridViewTextBoxColumn.DataPropertyName = "Port";
            this.portDataGridViewTextBoxColumn.HeaderText = "Port";
            this.portDataGridViewTextBoxColumn.MaxInputLength = 8;
            this.portDataGridViewTextBoxColumn.Name = "portDataGridViewTextBoxColumn";
            this.portDataGridViewTextBoxColumn.Width = 80;
            //
            // autoRegistrationDataGridViewTextBoxColumn
            //
            this.autoRegistrationDataGridViewTextBoxColumn.DataPropertyName = "AutoRegistration";
            this.autoRegistrationDataGridViewTextBoxColumn.DataSource = this.namedValueBindingSource;
            this.autoRegistrationDataGridViewTextBoxColumn.DisplayMember = "ValueName";
            this.autoRegistrationDataGridViewTextBoxColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.autoRegistrationDataGridViewTextBoxColumn.HeaderText = "New users registration";
            this.autoRegistrationDataGridViewTextBoxColumn.Name = "autoRegistrationDataGridViewTextBoxColumn";
            this.autoRegistrationDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.autoRegistrationDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.autoRegistrationDataGridViewTextBoxColumn.ValueMember = "Value";
            this.autoRegistrationDataGridViewTextBoxColumn.Width = 180;
            //
            // namedValueBindingSource
            //
            this.namedValueBindingSource.DataSource = typeof(Paranoid.NamedValue);
            //
            // privatePortPasswordDataGridViewTextBoxColumn
            //
            this.privatePortPasswordDataGridViewTextBoxColumn.DataPropertyName = "PrivatePortPassword";
            this.privatePortPasswordDataGridViewTextBoxColumn.HeaderText = "Password for private port (optional)";
            this.privatePortPasswordDataGridViewTextBoxColumn.MaxInputLength = 127;
            this.privatePortPasswordDataGridViewTextBoxColumn.Name = "privatePortPasswordDataGridViewTextBoxColumn";
            this.privatePortPasswordDataGridViewTextBoxColumn.Width = 190;
            //
            // listenPortsBindingSource
            //
            this.listenPortsBindingSource.DataSource = typeof(Paranoid.ListenPorts);
            //
            // MainForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(868, 565);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.IDLabel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "ParanoidServer Configuration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RegTimeout)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BindingsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.namedValueBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.listenPortsBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label IDLabel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox DescriptionTextBox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox PortTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox IpTextBox;
		private System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.CheckBox RegistrationCheckBox;
		private System.Windows.Forms.CheckBox RelayCheckBox;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label KeyExpirationDate;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button NewKeyButton;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.NumericUpDown RegTimeout;
		private System.Windows.Forms.Label TimeoutLabel;
		private System.Windows.Forms.DataGridView BindingsDataGridView;
		private System.Windows.Forms.BindingSource namedValueBindingSource;
		private System.Windows.Forms.BindingSource listenPortsBindingSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn ipDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn portDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn autoRegistrationDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn privatePortPasswordDataGridViewTextBoxColumn;
	}
}

