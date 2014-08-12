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
			this.autoLabel3 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.elementHost2 = new System.Windows.Forms.Integration.ElementHost();
			this.progress1 = new Teleopti.Ccc.WpfControls.Controls.Progress();
			this.SuspendLayout();
			// 
			// pnlContent
			// 
			this.pnlContent.Location = new System.Drawing.Point(4, 4);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Size = new System.Drawing.Size(475, 249);
			this.pnlContent.TabIndex = 1;
			this.pnlContent.Visible = false;
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
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(483, 258);
			this.Controls.Add(this.pnlContent);
			this.Controls.Add(this.autoLabel3);
			this.Controls.Add(this.elementHost2);
			this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LogonView";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Teleopti WFM";
			this.Load += new System.EventHandler(this.LogonView_Load);
			this.Shown += new System.EventHandler(this.logonViewShown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel pnlContent;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel3;
		private System.Windows.Forms.Integration.ElementHost elementHost2;
		private WpfControls.Controls.Progress progress1;

	}
}