namespace Teleopti.Ccc.Win.Main
{
	partial class LoginWebView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginWebView));
			this.labelVersion = new System.Windows.Forms.Label();
			this.webControl = new EO.WebBrowser.WinForm.WebControl();
			this.webView1 = new EO.WebBrowser.WebView();
			this.SuspendLayout();
			// 
			// labelVersion
			// 
			this.labelVersion.AutoSize = true;
			this.labelVersion.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.labelVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.labelVersion.Location = new System.Drawing.Point(1, 354);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(124, 15);
			this.labelVersion.TabIndex = 3;
			this.labelVersion.Text = "Version 8.0.123.123456";
			// 
			// webControl
			// 
			this.webControl.BackColor = System.Drawing.Color.White;
			this.webControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webControl.Location = new System.Drawing.Point(1, 1);
			this.webControl.Name = "webControl";
			this.webControl.Size = new System.Drawing.Size(562, 353);
			this.webControl.TabIndex = 4;
			this.webControl.TabStop = false;
			this.webControl.Text = "webControl2";
			this.webControl.WebView = this.webView1;
			// 
			// webView1
			// 
			this.webView1.AllowDropLoad = true;
			// 
			// LoginWebView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(564, 370);
			this.Controls.Add(this.webControl);
			this.Controls.Add(this.labelVersion);
			this.DropShadow = true;
			this.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(254)))), ((int)(((byte)(255)))));
			this.MinimizeBox = false;
			this.Name = "LoginWebView";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelVersion;
		private EO.WebBrowser.WinForm.WebControl webControl;
		private EO.WebBrowser.WebView webView1;
	}
}