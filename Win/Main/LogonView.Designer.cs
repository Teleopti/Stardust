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
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.pnlContent = new System.Windows.Forms.Panel();
			this.autoLabel2 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel3 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.elementHost2 = new System.Windows.Forms.Integration.ElementHost();
			this.progress1 = new Teleopti.Ccc.WpfControls.Controls.Progress();
			this.SuspendLayout();
			// 
			// autoLabel1
			// 
			this.autoLabel1.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.autoLabel1.ForeColor = System.Drawing.Color.White;
			this.autoLabel1.Location = new System.Drawing.Point(19, 8);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(253, 47);
			this.autoLabel1.TabIndex = 0;
			this.autoLabel1.Text = "Teleopti WFM";
			// 
			// pnlContent
			// 
			this.pnlContent.Location = new System.Drawing.Point(4, 75);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Size = new System.Drawing.Size(475, 218);
			this.pnlContent.TabIndex = 1;
			this.pnlContent.Visible = false;
			// 
			// autoLabel2
			// 
			this.autoLabel2.ForeColor = System.Drawing.Color.White;
			this.autoLabel2.Location = new System.Drawing.Point(30, 53);
			this.autoLabel2.Name = "autoLabel2";
			this.autoLabel2.Size = new System.Drawing.Size(77, 19);
			this.autoLabel2.TabIndex = 2;
			this.autoLabel2.Text = "autoLabel2";
			// 
			// autoLabel3
			// 
			this.autoLabel3.AutoSize = false;
			this.autoLabel3.Font = new System.Drawing.Font("Segoe UI", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.autoLabel3.ForeColor = System.Drawing.Color.White;
			this.autoLabel3.Location = new System.Drawing.Point(24, 124);
			this.autoLabel3.Name = "autoLabel3";
			this.autoLabel3.Size = new System.Drawing.Size(438, 55);
			this.autoLabel3.TabIndex = 3;
			this.autoLabel3.Text = "Starting...";
			this.autoLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// elementHost2
			// 
			this.elementHost2.AutoSize = true;
			this.elementHost2.Location = new System.Drawing.Point(12, 161);
			this.elementHost2.Name = "elementHost2";
			this.elementHost2.Size = new System.Drawing.Size(455, 57);
			this.elementHost2.TabIndex = 0;
			this.elementHost2.Text = "elementHost2";
			this.elementHost2.Visible = false;
			this.elementHost2.Child = this.progress1;
			// 
			// LogonView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(483, 297);
			this.Controls.Add(this.pnlContent);
			this.Controls.Add(this.autoLabel3);
			this.Controls.Add(this.autoLabel2);
			this.Controls.Add(this.autoLabel1);
			this.Controls.Add(this.elementHost2);
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
			this.PerformLayout();

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
		private System.Windows.Forms.Panel pnlContent;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel2;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel3;
		private System.Windows.Forms.Integration.ElementHost elementHost2;
		private WpfControls.Controls.Progress progress1;

	}
}