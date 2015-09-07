namespace Teleopti.Analytics.Etl.ConfigTool.Gui.StartupConfiguration
{
	partial class StartupConfigurationView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupConfigurationView));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.comboBoxCulture = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBoxIntervalLength = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.comboBoxTimeZone = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.labelMessage = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.BorderSize = 0;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Location = new System.Drawing.Point(559, 382);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// buttonOk
			// 
			this.buttonOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.FlatAppearance.BorderSize = 0;
			this.buttonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOk.ForeColor = System.Drawing.Color.White;
			this.buttonOk.Location = new System.Drawing.Point(464, 382);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(87, 27);
			this.buttonOk.TabIndex = 3;
			this.buttonOk.Text = "OK";
			this.buttonOk.UseVisualStyleBackColor = false;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// comboBoxCulture
			// 
			this.comboBoxCulture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCulture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboBoxCulture.FormattingEnabled = true;
			this.comboBoxCulture.Location = new System.Drawing.Point(119, 273);
			this.comboBoxCulture.Name = "comboBoxCulture";
			this.comboBoxCulture.Size = new System.Drawing.Size(527, 23);
			this.comboBoxCulture.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 276);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(49, 15);
			this.label1.TabIndex = 3;
			this.label1.Text = "Culture:";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label2.Location = new System.Drawing.Point(14, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(632, 90);
			this.label2.TabIndex = 4;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 245);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(89, 15);
			this.label3.TabIndex = 7;
			this.label3.Text = "Interval Length:";
			// 
			// comboBoxIntervalLength
			// 
			this.comboBoxIntervalLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxIntervalLength.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboBoxIntervalLength.FormattingEnabled = true;
			this.comboBoxIntervalLength.Location = new System.Drawing.Point(119, 241);
			this.comboBoxIntervalLength.Name = "comboBoxIntervalLength";
			this.comboBoxIntervalLength.Size = new System.Drawing.Size(527, 23);
			this.comboBoxIntervalLength.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(9, 307);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(67, 15);
			this.label4.TabIndex = 9;
			this.label4.Text = "Time Zone:";
			// 
			// comboBoxTimeZone
			// 
			this.comboBoxTimeZone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTimeZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboBoxTimeZone.FormattingEnabled = true;
			this.comboBoxTimeZone.Location = new System.Drawing.Point(119, 304);
			this.comboBoxTimeZone.Name = "comboBoxTimeZone";
			this.comboBoxTimeZone.Size = new System.Drawing.Size(527, 23);
			this.comboBoxTimeZone.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(14, 80);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(632, 158);
			this.label5.TabIndex = 10;
			this.label5.Text = resources.GetString("label5.Text");
			// 
			// labelMessage
			// 
			this.labelMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelMessage.ForeColor = System.Drawing.Color.Red;
			this.labelMessage.Location = new System.Drawing.Point(119, 336);
			this.labelMessage.Name = "labelMessage";
			this.labelMessage.Size = new System.Drawing.Size(527, 33);
			this.labelMessage.TabIndex = 11;
			// 
			// StartupConfigurationView
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(664, 425);
			this.ControlBox = false;
			this.Controls.Add(this.labelMessage);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.comboBoxTimeZone);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.comboBoxIntervalLength);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBoxCulture);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.buttonCancel);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StartupConfigurationView";
			this.Opacity = 0.9D;
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ETL Initial Configuration";
			this.Load += new System.EventHandler(this.StartupConfigurationView_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.ComboBox comboBoxCulture;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox comboBoxIntervalLength;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboBoxTimeZone;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label labelMessage;
	}
}