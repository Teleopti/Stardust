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
			Syncfusion.Windows.Forms.CaptionImage captionImage1 = new Syncfusion.Windows.Forms.CaptionImage();
			this.labelVersion = new System.Windows.Forms.Label();
			this.webControl = new EO.WebBrowser.WinForm.WebControl();
			this.webView1 = new EO.WebBrowser.WebView();
			this.panel1 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// labelVersion
			// 
			this.labelVersion.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.labelVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.labelVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelVersion.ForeColor = System.Drawing.Color.White;
			this.labelVersion.Location = new System.Drawing.Point(0, 390);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(788, 27);
			this.labelVersion.TabIndex = 3;
			this.labelVersion.Text = "Version 8.0.123.123456";
			// 
			// webControl
			// 
			this.webControl.BackColor = System.Drawing.Color.White;
			this.webControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.webControl.Location = new System.Drawing.Point(0, 0);
			this.webControl.Margin = new System.Windows.Forms.Padding(0);
			this.webControl.Name = "webControl";
			this.webControl.Size = new System.Drawing.Size(788, 390);
			this.webControl.TabIndex = 4;
			this.webControl.TabStop = false;
			this.webControl.Text = "webControl2";
			this.webControl.WebView = this.webView1;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackColor = System.Drawing.Color.Gainsboro;
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(788, 390);
			this.panel1.TabIndex = 5;
			// 
			// LoginWebView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.LightBlue;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.BorderColor = System.Drawing.Color.LightBlue;
			this.BorderThickness = 11;
			this.CaptionAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.CaptionBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(214)))), ((int)(((byte)(255)))));
			this.CaptionBarHeight = 18;
			this.CaptionButtonColor = System.Drawing.Color.White;
			this.CaptionButtonHoverColor = System.Drawing.Color.DeepSkyBlue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			captionImage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(214)))), ((int)(((byte)(255)))));
			captionImage1.Location = new System.Drawing.Point(5, 5);
			captionImage1.Name = "CaptionImage1";
			captionImage1.Size = new System.Drawing.Size(200, 24);
			this.CaptionImages.Add(captionImage1);
			this.ClientSize = new System.Drawing.Size(788, 410);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.webControl);
			this.Controls.Add(this.labelVersion);
			this.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(214)))), ((int)(((byte)(255)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MetroColor = System.Drawing.Color.LightBlue;
			this.MinimizeBox = false;
			this.Name = "LoginWebView";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "WFM Login";
			this.TransparencyKey = System.Drawing.Color.LightBlue;
			this.Load += new System.EventHandler(this.loginWebViewLoad);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelVersion;
		private EO.WebBrowser.WinForm.WebControl webControl;
		private EO.WebBrowser.WebView webView1;
		private System.Windows.Forms.Panel panel1;
	}
}