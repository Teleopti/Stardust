﻿namespace Teleopti.Runtime.Environment
{
    partial class MainForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.webControl1 = new EO.WebBrowser.WinForm.WebControl();
			this.webView1 = new EO.WebBrowser.WebView();
			this.notifyIconScheduleMessenger = new System.Windows.Forms.NotifyIcon(this.components);
			this.SuspendLayout();
			// 
			// webControl1
			// 
			this.webControl1.BackColor = System.Drawing.Color.White;
			this.webControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webControl1.Location = new System.Drawing.Point(0, 0);
			this.webControl1.Name = "webControl1";
			this.webControl1.Size = new System.Drawing.Size(1020, 600);
			this.webControl1.TabIndex = 0;
			this.webControl1.Text = "webControl1";
			this.webControl1.WebView = this.webView1;
			// 
			// webView1
			// 
			this.webView1.Url = "http://localhost:56760";
			this.webView1.IsLoadingChanged += new System.EventHandler(this.webView1_IsLoadingChanged);
			this.webView1.BeforeContextMenu += new EO.WebBrowser.BeforeContextMenuHandler(this.webView1_BeforeContextMenu);
			this.webView1.NewWindow += new EO.WebBrowser.NewWindowHandler(this.webView1_NewWindow);
			// 
			// notifyIconScheduleMessenger
			// 
			this.notifyIconScheduleMessenger.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconScheduleMessenger.Icon")));
			this.notifyIconScheduleMessenger.Text = "Agent Schedule Messenger";
			this.notifyIconScheduleMessenger.Visible = true;
			this.notifyIconScheduleMessenger.BalloonTipClicked += new System.EventHandler(this.notifyIconScheduleMessenger_BalloonTipClicked);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1020, 600);
			this.Controls.Add(this.webControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.Text = "Teleopti WFM";
			this.ResumeLayout(false);

        }

        #endregion

        private EO.WebBrowser.WinForm.WebControl webControl1;
        private EO.WebBrowser.WebView webView1;
		private System.Windows.Forms.NotifyIcon notifyIconScheduleMessenger;
    }
}

