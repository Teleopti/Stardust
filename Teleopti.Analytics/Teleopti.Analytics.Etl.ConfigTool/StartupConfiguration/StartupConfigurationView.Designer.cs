namespace Teleopti.Analytics.Etl.ConfigTool.StartupConfiguration
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
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(718, 495);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(112, 35);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOk
			// 
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Location = new System.Drawing.Point(597, 495);
			this.buttonOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(112, 35);
			this.buttonOk.TabIndex = 3;
			this.buttonOk.Text = "OK";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// comboBoxCulture
			// 
			this.comboBoxCulture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCulture.FormattingEnabled = true;
			this.comboBoxCulture.Location = new System.Drawing.Point(153, 386);
			this.comboBoxCulture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.comboBoxCulture.Name = "comboBoxCulture";
			this.comboBoxCulture.Size = new System.Drawing.Size(676, 28);
			this.comboBoxCulture.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 391);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 20);
			this.label1.TabIndex = 3;
			this.label1.Text = "Culture:";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label2.Location = new System.Drawing.Point(18, 14);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(813, 120);
			this.label2.TabIndex = 4;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 349);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(119, 20);
			this.label3.TabIndex = 7;
			this.label3.Text = "Interval Length:";
			// 
			// comboBoxIntervalLength
			// 
			this.comboBoxIntervalLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxIntervalLength.FormattingEnabled = true;
			this.comboBoxIntervalLength.Location = new System.Drawing.Point(153, 345);
			this.comboBoxIntervalLength.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.comboBoxIntervalLength.Name = "comboBoxIntervalLength";
			this.comboBoxIntervalLength.Size = new System.Drawing.Size(676, 28);
			this.comboBoxIntervalLength.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 432);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 20);
			this.label4.TabIndex = 9;
			this.label4.Text = "Time Zone:";
			// 
			// comboBoxTimeZone
			// 
			this.comboBoxTimeZone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTimeZone.FormattingEnabled = true;
			this.comboBoxTimeZone.Location = new System.Drawing.Point(153, 428);
			this.comboBoxTimeZone.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.comboBoxTimeZone.Name = "comboBoxTimeZone";
			this.comboBoxTimeZone.Size = new System.Drawing.Size(676, 28);
			this.comboBoxTimeZone.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(18, 134);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(813, 198);
			this.label5.TabIndex = 10;
			this.label5.Text = resources.GetString("label5.Text");
			// 
			// StartupConfigurationView
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(849, 554);
			this.ControlBox = false;
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
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "StartupConfigurationView";
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
	}
}