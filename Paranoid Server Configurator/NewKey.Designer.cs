namespace Paranoid
{
    partial class NewKey
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.ExpirationDatePicker = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.RandomDataTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // button1
            //
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(296, 182);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 24);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            //
            // button2
            //
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(373, 182);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(80, 24);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            //
            // ExpirationDatePicker
            //
            this.ExpirationDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.ExpirationDatePicker.Location = new System.Drawing.Point(262, 7);
            this.ExpirationDatePicker.Name = "ExpirationDatePicker";
            this.ExpirationDatePicker.Size = new System.Drawing.Size(191, 22);
            this.ExpirationDatePicker.TabIndex = 2;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(205, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Set current key expiration date:";
            //
            // RandomDataTextBox
            //
            this.RandomDataTextBox.Location = new System.Drawing.Point(17, 65);
            this.RandomDataTextBox.Multiline = true;
            this.RandomDataTextBox.Name = "RandomDataTextBox";
            this.RandomDataTextBox.Size = new System.Drawing.Size(436, 111);
            this.RandomDataTextBox.TabIndex = 4;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(280, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Random data for key generation (optional):";
            //
            // NewKey
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(472, 215);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RandomDataTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ExpirationDatePicker);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NewKey";
            this.Text = "New key generation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.DateTimePicker ExpirationDatePicker;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox RandomDataTextBox;
        private System.Windows.Forms.Label label2;
    }
}