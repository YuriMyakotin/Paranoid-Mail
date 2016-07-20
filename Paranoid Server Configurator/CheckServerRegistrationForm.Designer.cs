namespace Paranoid
{
	partial class CheckServerRegistrationForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.CancelBtn = new System.Windows.Forms.Button();
			this.CheckBtn = new System.Windows.Forms.Button();
			this.ErrorLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label1.Location = new System.Drawing.Point(130, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(369, 25);
			this.label1.TabIndex = 0;
			this.label1.Text = "Check server registration result now?";
			//
			// CancelBtn
			//
			this.CancelBtn.CausesValidation = false;
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = new System.Drawing.Point(496, 141);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = new System.Drawing.Size(120, 36);
			this.CancelBtn.TabIndex = 10;
			this.CancelBtn.Text = "Cancel";
			this.CancelBtn.UseVisualStyleBackColor = true;
			//
			// CheckBtn
			//
			this.CheckBtn.Location = new System.Drawing.Point(378, 141);
			this.CheckBtn.Name = "CheckBtn";
			this.CheckBtn.Size = new System.Drawing.Size(120, 36);
			this.CheckBtn.TabIndex = 9;
			this.CheckBtn.Text = "Check now";
			this.CheckBtn.UseVisualStyleBackColor = true;
			this.CheckBtn.Click += new System.EventHandler(this.CheckBtn_Click);
			//
			// ErrorLabel
			//
			this.ErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.ErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.ErrorLabel.Location = new System.Drawing.Point(42, 46);
			this.ErrorLabel.Name = "ErrorLabel";
			this.ErrorLabel.Size = new System.Drawing.Size(545, 79);
			this.ErrorLabel.TabIndex = 11;
			this.ErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// CheckServerRegistrationForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(628, 189);
			this.Controls.Add(this.ErrorLabel);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.CheckBtn);
			this.Controls.Add(this.label1);
			this.Name = "CheckServerRegistrationForm";
			this.Text = "Check server registration";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button CancelBtn;
		private System.Windows.Forms.Button CheckBtn;
		private System.Windows.Forms.Label ErrorLabel;
	}
}