﻿using Teleopti.Support.Tool.Controls.General;

namespace Teleopti.Support.Tool
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
            this.PTracks = new System.Windows.Forms.Panel();
            this.buttonVerifyCredentials = new System.Windows.Forms.Button();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.textBoxServerName = new System.Windows.Forms.TextBox();
            this.checkBoxUseWindowsAuthentication = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.BClose = new System.Windows.Forms.Button();
            this.pictureBoxBathingBall = new System.Windows.Forms.PictureBox();
            this.panelContent = new System.Windows.Forms.Panel();
            this.toolTipInfo = new System.Windows.Forms.ToolTip(this.components);
            this.smoothLabelVersion = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.labelHeader = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.smoothLabelTools = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.smoothLabelConnected = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.smoothLabelDBAdminstatrorLogonCredentials = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.linkManageDBVersions = new Teleopti.Support.Tool.Controls.General.SmoothLink();
            this.LLChangeSettings = new Teleopti.Support.Tool.Controls.General.SmoothLink();
            this.PTracks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBathingBall)).BeginInit();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // PTracks
            // 
            this.PTracks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PTracks.Controls.Add(this.smoothLabelTools);
            this.PTracks.Controls.Add(this.smoothLabelConnected);
            this.PTracks.Controls.Add(this.buttonVerifyCredentials);
            this.PTracks.Controls.Add(this.textBoxPassword);
            this.PTracks.Controls.Add(this.textBoxUserName);
            this.PTracks.Controls.Add(this.textBoxServerName);
            this.PTracks.Controls.Add(this.checkBoxUseWindowsAuthentication);
            this.PTracks.Controls.Add(this.smoothLabelDBAdminstatrorLogonCredentials);
            this.PTracks.Controls.Add(this.label2);
            this.PTracks.Controls.Add(this.label1);
            this.PTracks.Controls.Add(this.linkManageDBVersions);
            this.PTracks.Controls.Add(this.LLChangeSettings);
            this.PTracks.Location = new System.Drawing.Point(0, 0);
            this.PTracks.Name = "PTracks";
            this.PTracks.Size = new System.Drawing.Size(670, 321);
            this.PTracks.TabIndex = 4;
            // 
            // buttonVerifyCredentials
            // 
            this.buttonVerifyCredentials.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonVerifyCredentials.Location = new System.Drawing.Point(188, 132);
            this.buttonVerifyCredentials.Name = "buttonVerifyCredentials";
            this.buttonVerifyCredentials.Size = new System.Drawing.Size(66, 22);
            this.buttonVerifyCredentials.TabIndex = 14;
            this.buttonVerifyCredentials.Text = "Verify";
            this.buttonVerifyCredentials.UseVisualStyleBackColor = true;
            this.buttonVerifyCredentials.Click += new System.EventHandler(this.buttonVerifyCredentials_Click);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(27, 132);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(155, 20);
            this.textBoxPassword.TabIndex = 13;
            this.toolTipInfo.SetToolTip(this.textBoxPassword, "Password");
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUserName.Location = new System.Drawing.Point(27, 106);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(155, 20);
            this.textBoxUserName.TabIndex = 12;
            this.toolTipInfo.SetToolTip(this.textBoxUserName, "User name");
            this.textBoxUserName.TextChanged += new System.EventHandler(this.textBoxUserName_TextChanged);
            // 
            // textBoxServerName
            // 
            this.textBoxServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxServerName.Location = new System.Drawing.Point(27, 57);
            this.textBoxServerName.Name = "textBoxServerName";
            this.textBoxServerName.Size = new System.Drawing.Size(227, 20);
            this.textBoxServerName.TabIndex = 11;
            this.toolTipInfo.SetToolTip(this.textBoxServerName, "SQL Server Name");
            this.textBoxServerName.TextChanged += new System.EventHandler(this.textBoxServerName_TextChanged);
            // 
            // checkBoxUseWindowsAuthentication
            // 
            this.checkBoxUseWindowsAuthentication.AutoSize = true;
            this.checkBoxUseWindowsAuthentication.Location = new System.Drawing.Point(27, 83);
            this.checkBoxUseWindowsAuthentication.Name = "checkBoxUseWindowsAuthentication";
            this.checkBoxUseWindowsAuthentication.Size = new System.Drawing.Size(163, 17);
            this.checkBoxUseWindowsAuthentication.TabIndex = 10;
            this.checkBoxUseWindowsAuthentication.Text = "Use Windows Authentication";
            this.checkBoxUseWindowsAuthentication.UseVisualStyleBackColor = true;
            this.checkBoxUseWindowsAuthentication.CheckedChanged += new System.EventHandler(this.checkBoxUseWindowsAuthentication_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(323, 151);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(344, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "List and manage your Teleopti WFM databases";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(323, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(344, 46);
            this.label1.TabIndex = 7;
            this.label1.Text = "Update and save all settings and optionally replace the values in the config file" +
    "s.";
            // 
            // BClose
            // 
            this.BClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BClose.Location = new System.Drawing.Point(605, 441);
            this.BClose.Name = "BClose";
            this.BClose.Size = new System.Drawing.Size(75, 23);
            this.BClose.TabIndex = 0;
            this.BClose.Text = "Exit";
            this.BClose.UseVisualStyleBackColor = true;
            this.BClose.Click += new System.EventHandler(this.BClose_Click);
            // 
            // pictureBoxBathingBall
            // 
            this.pictureBoxBathingBall.Image = global::Teleopti.Support.Tool.Properties.Resources.Teleopti_WFM_main_icon256;
            this.pictureBoxBathingBall.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxBathingBall.Name = "pictureBoxBathingBall";
            this.pictureBoxBathingBall.Size = new System.Drawing.Size(96, 96);
            this.pictureBoxBathingBall.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxBathingBall.TabIndex = 6;
            this.pictureBoxBathingBall.TabStop = false;
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.Controls.Add(this.PTracks);
            this.panelContent.Location = new System.Drawing.Point(12, 114);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(670, 321);
            this.panelContent.TabIndex = 7;
            // 
            // toolTipInfo
            // 
            this.toolTipInfo.AutomaticDelay = 200;
            this.toolTipInfo.IsBalloon = true;
            this.toolTipInfo.OwnerDraw = true;
            // 
            // smoothLabelVersion
            // 
            this.smoothLabelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.smoothLabelVersion.AutoSize = true;
            this.smoothLabelVersion.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabelVersion.ForeColor = System.Drawing.Color.Gray;
            this.smoothLabelVersion.Location = new System.Drawing.Point(12, 454);
            this.smoothLabelVersion.Name = "smoothLabelVersion";
            this.smoothLabelVersion.Size = new System.Drawing.Size(0, 13);
            this.smoothLabelVersion.TabIndex = 8;
            this.smoothLabelVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.smoothLabelVersion.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            // 
            // labelHeader
            // 
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.labelHeader.Location = new System.Drawing.Point(114, 37);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(374, 46);
            this.labelHeader.TabIndex = 5;
            this.labelHeader.Text = "Teleopti WFM Support Tool";
            this.labelHeader.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            this.labelHeader.UseCompatibleTextRendering = true;
            // 
            // smoothLabelTools
            // 
            this.smoothLabelTools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.smoothLabelTools.AutoSize = true;
            this.smoothLabelTools.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabelTools.Location = new System.Drawing.Point(306, 28);
            this.smoothLabelTools.Margin = new System.Windows.Forms.Padding(5);
            this.smoothLabelTools.Name = "smoothLabelTools";
            this.smoothLabelTools.Size = new System.Drawing.Size(47, 21);
            this.smoothLabelTools.TabIndex = 16;
            this.smoothLabelTools.Text = "Tools";
            this.smoothLabelTools.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // smoothLabelConnected
            // 
            this.smoothLabelConnected.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smoothLabelConnected.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabelConnected.ForeColor = System.Drawing.Color.Blue;
            this.smoothLabelConnected.Location = new System.Drawing.Point(27, 155);
            this.smoothLabelConnected.Name = "smoothLabelConnected";
            this.smoothLabelConnected.Size = new System.Drawing.Size(237, 152);
            this.smoothLabelConnected.TabIndex = 15;
            this.smoothLabelConnected.Text = "Click \"Verify\" to check your credentials";
            this.smoothLabelConnected.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.smoothLabelConnected.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            // 
            // smoothLabelDBAdminstatrorLogonCredentials
            // 
            this.smoothLabelDBAdminstatrorLogonCredentials.AutoSize = true;
            this.smoothLabelDBAdminstatrorLogonCredentials.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabelDBAdminstatrorLogonCredentials.Location = new System.Drawing.Point(5, 28);
            this.smoothLabelDBAdminstatrorLogonCredentials.Margin = new System.Windows.Forms.Padding(5);
            this.smoothLabelDBAdminstatrorLogonCredentials.Name = "smoothLabelDBAdminstatrorLogonCredentials";
            this.smoothLabelDBAdminstatrorLogonCredentials.Size = new System.Drawing.Size(204, 21);
            this.smoothLabelDBAdminstatrorLogonCredentials.TabIndex = 9;
            this.smoothLabelDBAdminstatrorLogonCredentials.Text = "Database Logon Credentials";
            this.smoothLabelDBAdminstatrorLogonCredentials.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            // 
            // linkManageDBVersions
            // 
            this.linkManageDBVersions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkManageDBVersions.AutoSize = true;
            this.linkManageDBVersions.Enabled = false;
            this.linkManageDBVersions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkManageDBVersions.Location = new System.Drawing.Point(323, 129);
            this.linkManageDBVersions.Margin = new System.Windows.Forms.Padding(5);
            this.linkManageDBVersions.Name = "linkManageDBVersions";
            this.linkManageDBVersions.Size = new System.Drawing.Size(202, 17);
            this.linkManageDBVersions.TabIndex = 1;
            this.linkManageDBVersions.TabStop = true;
            this.linkManageDBVersions.Text = "Teleopti WFM Database Versions";
            this.linkManageDBVersions.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            this.linkManageDBVersions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // LLChangeSettings
            // 
            this.LLChangeSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LLChangeSettings.AutoSize = true;
            this.LLChangeSettings.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LLChangeSettings.Location = new System.Drawing.Point(323, 57);
            this.LLChangeSettings.Margin = new System.Windows.Forms.Padding(5);
            this.LLChangeSettings.Name = "LLChangeSettings";
            this.LLChangeSettings.Size = new System.Drawing.Size(102, 17);
            this.LLChangeSettings.TabIndex = 0;
            this.LLChangeSettings.TabStop = true;
            this.LLChangeSettings.Text = "Change Settings";
            this.LLChangeSettings.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            this.LLChangeSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LLChangeDBConn_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(694, 476);
            this.Controls.Add(this.smoothLabelVersion);
            this.Controls.Add(this.pictureBoxBathingBall);
            this.Controls.Add(this.labelHeader);
            this.Controls.Add(this.BClose);
            this.Controls.Add(this.panelContent);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "MainForm";
            this.Text = "Teleopti WFM Support Tool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.PTracks.ResumeLayout(false);
            this.PTracks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBathingBall)).EndInit();
            this.panelContent.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PTracks;
        private System.Windows.Forms.Label label1;
        SmoothLink linkManageDBVersions;
        SmoothLink LLChangeSettings;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BClose;
        private SmoothLabel labelHeader;
        private System.Windows.Forms.PictureBox pictureBoxBathingBall;
        private System.Windows.Forms.Panel panelContent;
        private SmoothLabel smoothLabelVersion;
        private SmoothLabel smoothLabelDBAdminstatrorLogonCredentials;
        private System.Windows.Forms.CheckBox checkBoxUseWindowsAuthentication;
        private System.Windows.Forms.TextBox textBoxServerName;
        private System.Windows.Forms.ToolTip toolTipInfo;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Button buttonVerifyCredentials;
        private SmoothLabel smoothLabelConnected;
        private SmoothLabel smoothLabelTools;


    }
}