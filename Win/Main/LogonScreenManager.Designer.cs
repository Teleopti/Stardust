namespace Teleopti.Ccc.Win.Main
{
	partial class LogonScreenManager
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
			this.pnlContent = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// pnlContent
			// 
			this.pnlContent.Location = new System.Drawing.Point(1, 1);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Size = new System.Drawing.Size(488, 334);
			this.pnlContent.TabIndex = 0;
			// 
			// LogonScreenManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(490, 337);
			this.Controls.Add(this.pnlContent);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "LogonScreenManager";
			this.Text = "LogonScreenManager";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pnlContent;
	}
}