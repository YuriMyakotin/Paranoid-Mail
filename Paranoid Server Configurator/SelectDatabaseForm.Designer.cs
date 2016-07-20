namespace Paranoid
{
    partial class SelectDatabaseForm
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
			this.SQLiteRadioButton = new System.Windows.Forms.RadioButton();
			this.MSSQLRadioButton = new System.Windows.Forms.RadioButton();
			this.MySQLRadioButton = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.ConnStringTextBox = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.ErrorTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			//
			// SQLiteRadioButton
			//
			this.SQLiteRadioButton.AutoSize = true;
			this.SQLiteRadioButton.Location = new System.Drawing.Point(6, 21);
			this.SQLiteRadioButton.Name = "SQLiteRadioButton";
			this.SQLiteRadioButton.Size = new System.Drawing.Size(72, 21);
			this.SQLiteRadioButton.TabIndex = 0;
			this.SQLiteRadioButton.Text = "SQLite";
			this.SQLiteRadioButton.UseVisualStyleBackColor = true;
			this.SQLiteRadioButton.CheckedChanged += new System.EventHandler(this.SQLiteRadioButton_CheckedChanged);
			//
			// MSSQLRadioButton
			//
			this.MSSQLRadioButton.AutoSize = true;
			this.MSSQLRadioButton.Location = new System.Drawing.Point(6, 48);
			this.MSSQLRadioButton.Name = "MSSQLRadioButton";
			this.MSSQLRadioButton.Size = new System.Drawing.Size(307, 21);
			this.MSSQLRadioButton.TabIndex = 1;
			this.MSSQLRadioButton.Text = "Microsoft SQL Server / SQL Express / Azure";
			this.MSSQLRadioButton.UseVisualStyleBackColor = true;
			this.MSSQLRadioButton.CheckedChanged += new System.EventHandler(this.MSSQLRadioButton_CheckedChanged);
			//
			// MySQLRadioButton
			//
			this.MySQLRadioButton.AutoSize = true;
			this.MySQLRadioButton.Location = new System.Drawing.Point(6, 75);
			this.MySQLRadioButton.Name = "MySQLRadioButton";
			this.MySQLRadioButton.Size = new System.Drawing.Size(206, 21);
			this.MySQLRadioButton.TabIndex = 2;
			this.MySQLRadioButton.Text = "MySQL / MariaDB / Percona";
			this.MySQLRadioButton.UseVisualStyleBackColor = true;
			this.MySQLRadioButton.CheckedChanged += new System.EventHandler(this.MySQLRadioButton_CheckedChanged);
			//
			// groupBox1
			//
			this.groupBox1.Controls.Add(this.MySQLRadioButton);
			this.groupBox1.Controls.Add(this.MSSQLRadioButton);
			this.groupBox1.Controls.Add(this.SQLiteRadioButton);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(763, 106);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Database type";
			//
			// groupBox2
			//
			this.groupBox2.Controls.Add(this.ConnStringTextBox);
			this.groupBox2.Location = new System.Drawing.Point(12, 132);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(762, 98);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Connection string";
			//
			// ConnStringTextBox
			//
			this.ConnStringTextBox.Location = new System.Drawing.Point(6, 21);
			this.ConnStringTextBox.Multiline = true;
			this.ConnStringTextBox.Name = "ConnStringTextBox";
			this.ConnStringTextBox.Size = new System.Drawing.Size(750, 61);
			this.ConnStringTextBox.TabIndex = 0;
			//
			// button1
			//
			this.button1.Location = new System.Drawing.Point(530, 394);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(120, 36);
			this.button1.TabIndex = 2;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			//
			// button2
			//
			this.button2.CausesValidation = false;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(648, 394);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(120, 36);
			this.button2.TabIndex = 3;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			//
			// ErrorTextBox
			//
			this.ErrorTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.ErrorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ErrorTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.ErrorTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.ErrorTextBox.ForeColor = System.Drawing.Color.Red;
			this.ErrorTextBox.Location = new System.Drawing.Point(15, 242);
			this.ErrorTextBox.Multiline = true;
			this.ErrorTextBox.Name = "ErrorTextBox";
			this.ErrorTextBox.ReadOnly = true;
			this.ErrorTextBox.Size = new System.Drawing.Size(752, 135);
			this.ErrorTextBox.TabIndex = 4;
			//
			// SelectDatabaseForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(787, 442);
			this.Controls.Add(this.ErrorTextBox);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SelectDatabaseForm";
			this.ShowIcon = false;
			this.Text = "Set up database connection";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton SQLiteRadioButton;
        private System.Windows.Forms.RadioButton MSSQLRadioButton;
        private System.Windows.Forms.RadioButton MySQLRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox ConnStringTextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox ErrorTextBox;
    }
}