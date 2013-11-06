namespace Teleopti.Ccc.Win.Main
{
	partial class LogonView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogonView));
			this.pnlContent = new System.Windows.Forms.Panel();
			this.labelStatusText = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// pnlContent
			// 
			this.pnlContent.BackColor = System.Drawing.Color.Transparent;
			this.pnlContent.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlContent.Location = new System.Drawing.Point(1, 1);
			this.pnlContent.Margin = new System.Windows.Forms.Padding(0);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Size = new System.Drawing.Size(481, 295);
			this.pnlContent.TabIndex = 0;
			// 
			// labelStatusText
			// 
			this.labelStatusText.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelStatusText.ForeColor = System.Drawing.Color.Orange;
			this.labelStatusText.Location = new System.Drawing.Point(2, 315);
			this.labelStatusText.Name = "labelStatusText";
			this.labelStatusText.Size = new System.Drawing.Size(481, 25);
			this.labelStatusText.TabIndex = 42;
			this.labelStatusText.Text = "xxSearching for data sources...";
			this.labelStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(5, 300);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(474, 15);
			this.label1.TabIndex = 43;
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// LogonView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.LightBlue;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(483, 345);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.labelStatusText);
			this.Controls.Add(this.pnlContent);
			this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "LogonView";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LogonView";
			this.Shown += new System.EventHandler(this.logonViewShown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pnlContent;
		private System.Windows.Forms.Label labelStatusText;
		private System.Windows.Forms.Label label1;
	}
}