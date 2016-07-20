namespace Paranoid
{
	partial class RegisterNewServerForm
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
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.RegistrationCheckBox = new System.Windows.Forms.CheckBox();
			this.PortTextBox = new System.Windows.Forms.TextBox();
			this.RelayCheckBox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.IpTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.DescriptionTextBox = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.IDLabel = new System.Windows.Forms.Label();
			this.ServerNameTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.RegisterButton = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.RandomDataTextBox = new System.Windows.Forms.TextBox();
			this.CaptchaGroupBox = new System.Windows.Forms.GroupBox();
			this.CaptchaCancelButton = new System.Windows.Forms.Button();
			this.CaptchaOkButton = new System.Windows.Forms.Button();
			this.CaptchaAnswerTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.CaptchaPictureBox = new System.Windows.Forms.PictureBox();
			this.ErrorLabel = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.CaptchaGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CaptchaPictureBox)).BeginInit();
			this.SuspendLayout();
			//
			// groupBox2
			//
			this.groupBox2.Controls.Add(this.RegistrationCheckBox);
			this.groupBox2.Controls.Add(this.PortTextBox);
			this.groupBox2.Controls.Add(this.RelayCheckBox);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.IpTextBox);
			this.groupBox2.Location = new System.Drawing.Point(12, 152);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(841, 83);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Server information";
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
			// groupBox1
			//
			this.groupBox1.Controls.Add(this.DescriptionTextBox);
			this.groupBox1.Location = new System.Drawing.Point(12, 95);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(841, 51);
			this.groupBox1.TabIndex = 4;
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
			// groupBox3
			//
			this.groupBox3.Controls.Add(this.IDLabel);
			this.groupBox3.Controls.Add(this.ServerNameTextBox);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Location = new System.Drawing.Point(12, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(841, 77);
			this.groupBox3.TabIndex = 6;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Server ID";
			//
			// IDLabel
			//
			this.IDLabel.AutoSize = true;
			this.IDLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.IDLabel.Location = new System.Drawing.Point(102, 46);
			this.IDLabel.Margin = new System.Windows.Forms.Padding(0);
			this.IDLabel.MinimumSize = new System.Drawing.Size(400, 0);
			this.IDLabel.Name = "IDLabel";
			this.IDLabel.Size = new System.Drawing.Size(400, 25);
			this.IDLabel.TabIndex = 8;
			this.IDLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.IDLabel.UseMnemonic = false;
			//
			// ServerNameTextBox
			//
			this.ServerNameTextBox.Location = new System.Drawing.Point(105, 21);
			this.ServerNameTextBox.Name = "ServerNameTextBox";
			this.ServerNameTextBox.Size = new System.Drawing.Size(719, 22);
			this.ServerNameTextBox.TabIndex = 1;
			this.ServerNameTextBox.TextChanged += new System.EventHandler(this.ServerNameTextBox_TextChanged);
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label4.Location = new System.Drawing.Point(7, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 25);
			this.label4.TabIndex = 7;
			this.label4.Text = "ID:";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(93, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server name:";
			//
			// button2
			//
			this.button2.CausesValidation = false;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(733, 535);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(120, 36);
			this.button2.TabIndex = 8;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			//
			// RegisterButton
			//
			this.RegisterButton.Enabled = false;
			this.RegisterButton.Location = new System.Drawing.Point(615, 535);
			this.RegisterButton.Name = "RegisterButton";
			this.RegisterButton.Size = new System.Drawing.Size(120, 36);
			this.RegisterButton.TabIndex = 7;
			this.RegisterButton.Text = "Register";
			this.RegisterButton.UseVisualStyleBackColor = true;
			this.RegisterButton.Click += new System.EventHandler(this.RegisterButton_Click);
			//
			// groupBox4
			//
			this.groupBox4.Controls.Add(this.RandomDataTextBox);
			this.groupBox4.Location = new System.Drawing.Point(12, 241);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(841, 100);
			this.groupBox4.TabIndex = 9;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Random data for key generation (optional)";
			//
			// RandomDataTextBox
			//
			this.RandomDataTextBox.Location = new System.Drawing.Point(12, 21);
			this.RandomDataTextBox.Multiline = true;
			this.RandomDataTextBox.Name = "RandomDataTextBox";
			this.RandomDataTextBox.Size = new System.Drawing.Size(811, 63);
			this.RandomDataTextBox.TabIndex = 0;
			//
			// CaptchaGroupBox
			//
			this.CaptchaGroupBox.Controls.Add(this.CaptchaCancelButton);
			this.CaptchaGroupBox.Controls.Add(this.CaptchaOkButton);
			this.CaptchaGroupBox.Controls.Add(this.CaptchaAnswerTextBox);
			this.CaptchaGroupBox.Controls.Add(this.label5);
			this.CaptchaGroupBox.Controls.Add(this.CaptchaPictureBox);
			this.CaptchaGroupBox.Location = new System.Drawing.Point(12, 352);
			this.CaptchaGroupBox.Name = "CaptchaGroupBox";
			this.CaptchaGroupBox.Size = new System.Drawing.Size(840, 162);
			this.CaptchaGroupBox.TabIndex = 10;
			this.CaptchaGroupBox.TabStop = false;
			this.CaptchaGroupBox.Text = "Captcha";
			this.CaptchaGroupBox.Visible = false;
			//
			// CaptchaCancelButton
			//
			this.CaptchaCancelButton.Location = new System.Drawing.Point(610, 126);
			this.CaptchaCancelButton.Name = "CaptchaCancelButton";
			this.CaptchaCancelButton.Size = new System.Drawing.Size(75, 25);
			this.CaptchaCancelButton.TabIndex = 4;
			this.CaptchaCancelButton.Text = "Cancel";
			this.CaptchaCancelButton.UseVisualStyleBackColor = true;
			this.CaptchaCancelButton.Click += new System.EventHandler(this.CaptchaCancelButton_Click);
			//
			// CaptchaOkButton
			//
			this.CaptchaOkButton.Location = new System.Drawing.Point(538, 126);
			this.CaptchaOkButton.Name = "CaptchaOkButton";
			this.CaptchaOkButton.Size = new System.Drawing.Size(75, 25);
			this.CaptchaOkButton.TabIndex = 3;
			this.CaptchaOkButton.Text = "OK";
			this.CaptchaOkButton.UseVisualStyleBackColor = true;
			this.CaptchaOkButton.Click += new System.EventHandler(this.CaptchaOkButton_Click);
			//
			// CaptchaAnswerTextBox
			//
			this.CaptchaAnswerTextBox.Location = new System.Drawing.Point(346, 126);
			this.CaptchaAnswerTextBox.Name = "CaptchaAnswerTextBox";
			this.CaptchaAnswerTextBox.Size = new System.Drawing.Size(177, 22);
			this.CaptchaAnswerTextBox.TabIndex = 2;
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(155, 129);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(185, 17);
			this.label5.TabIndex = 1;
			this.label5.Text = "Please enter numeric result:";
			//
			// CaptchaPictureBox
			//
			this.CaptchaPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CaptchaPictureBox.Location = new System.Drawing.Point(100, 21);
			this.CaptchaPictureBox.Name = "CaptchaPictureBox";
			this.CaptchaPictureBox.Size = new System.Drawing.Size(640, 96);
			this.CaptchaPictureBox.TabIndex = 0;
			this.CaptchaPictureBox.TabStop = false;
			//
			// ErrorLabel
			//
			this.ErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.ErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.ErrorLabel.Location = new System.Drawing.Point(12, 352);
			this.ErrorLabel.Name = "ErrorLabel";
			this.ErrorLabel.Size = new System.Drawing.Size(841, 170);
			this.ErrorLabel.TabIndex = 5;
			this.ErrorLabel.Text = "Test";
			this.ErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ErrorLabel.Visible = false;
			//
			// RegisterNewServerForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(869, 583);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.RegisterButton);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.CaptchaGroupBox);
			this.Controls.Add(this.ErrorLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RegisterNewServerForm";
			this.ShowIcon = false;
			this.Text = "New server registration";
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.CaptchaGroupBox.ResumeLayout(false);
			this.CaptchaGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CaptchaPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox RegistrationCheckBox;
		private System.Windows.Forms.TextBox PortTextBox;
		private System.Windows.Forms.CheckBox RelayCheckBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox IpTextBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox DescriptionTextBox;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox ServerNameTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label IDLabel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button RegisterButton;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.TextBox RandomDataTextBox;
		private System.Windows.Forms.GroupBox CaptchaGroupBox;
		private System.Windows.Forms.PictureBox CaptchaPictureBox;
		private System.Windows.Forms.TextBox CaptchaAnswerTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button CaptchaCancelButton;
		private System.Windows.Forms.Button CaptchaOkButton;
		private System.Windows.Forms.Label ErrorLabel;
	}
}