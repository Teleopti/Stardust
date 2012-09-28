using Teleopti.Support.Tool.Controls.General;

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
            this.PTracks = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.linkManageDBVersions = new Teleopti.Support.Tool.Controls.General.SmoothLink();
            this.LLChangeDBConn = new Teleopti.Support.Tool.Controls.General.SmoothLink();
            this.BClose = new System.Windows.Forms.Button();
            this.pictureBoxBathingBall = new System.Windows.Forms.PictureBox();
            this.panelContent = new System.Windows.Forms.Panel();
            this.labelHeader = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.PTracks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBathingBall)).BeginInit();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // PTracks
            // 
            this.PTracks.Controls.Add(this.label2);
            this.PTracks.Controls.Add(this.label1);
            this.PTracks.Controls.Add(this.linkManageDBVersions);
            this.PTracks.Controls.Add(this.LLChangeDBConn);
            this.PTracks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PTracks.Location = new System.Drawing.Point(0, 0);
            this.PTracks.Name = "PTracks";
            this.PTracks.Size = new System.Drawing.Size(670, 320);
            this.PTracks.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "List your Teleopti CCC databases";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(482, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Update database names and users in all connections and updates database reference" +
    "s in databases";
            // 
            // linkManageDBVersions
            // 
            this.linkManageDBVersions.AutoSize = true;
            this.linkManageDBVersions.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkManageDBVersions.Location = new System.Drawing.Point(5, 82);
            this.linkManageDBVersions.Margin = new System.Windows.Forms.Padding(5);
            this.linkManageDBVersions.Name = "linkManageDBVersions";
            this.linkManageDBVersions.Size = new System.Drawing.Size(323, 21);
            this.linkManageDBVersions.TabIndex = 1;
            this.linkManageDBVersions.TabStop = true;
            this.linkManageDBVersions.Text = "Manage your Teleopti CCC database versions";
            this.linkManageDBVersions.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            this.linkManageDBVersions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // LLChangeDBConn
            // 
            this.LLChangeDBConn.AutoSize = true;
            this.LLChangeDBConn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LLChangeDBConn.Location = new System.Drawing.Point(5, 26);
            this.LLChangeDBConn.Margin = new System.Windows.Forms.Padding(5);
            this.LLChangeDBConn.Name = "LLChangeDBConn";
            this.LLChangeDBConn.Size = new System.Drawing.Size(221, 21);
            this.LLChangeDBConn.TabIndex = 0;
            this.LLChangeDBConn.TabStop = true;
            this.LLChangeDBConn.Text = "Change Database Connections";
            this.LLChangeDBConn.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            this.LLChangeDBConn.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LLChangeDBConn_LinkClicked);
            // 
            // BClose
            // 
            this.BClose.Location = new System.Drawing.Point(605, 381);
            this.BClose.Name = "BClose";
            this.BClose.Size = new System.Drawing.Size(75, 23);
            this.BClose.TabIndex = 0;
            this.BClose.Text = "Exit";
            this.BClose.UseVisualStyleBackColor = true;
            this.BClose.Click += new System.EventHandler(this.BClose_Click);
            // 
            // pictureBoxBathingBall
            // 
            this.pictureBoxBathingBall.Image = global::Teleopti.Support.Tool.Properties.Resources.ccc_Menu;
            this.pictureBoxBathingBall.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxBathingBall.Name = "pictureBoxBathingBall";
            this.pictureBoxBathingBall.Size = new System.Drawing.Size(40, 40);
            this.pictureBoxBathingBall.TabIndex = 6;
            this.pictureBoxBathingBall.TabStop = false;
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.Controls.Add(this.PTracks);
            this.panelContent.Location = new System.Drawing.Point(12, 55);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(670, 320);
            this.panelContent.TabIndex = 7;
            // 
            // labelHeader
            // 
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.Location = new System.Drawing.Point(58, 14);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(410, 38);
            this.labelHeader.TabIndex = 5;
            this.labelHeader.Text = "Teleopti CCC Configuration Manager";
            this.labelHeader.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            this.labelHeader.UseCompatibleTextRendering = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(694, 416);
            this.Controls.Add(this.pictureBoxBathingBall);
            this.Controls.Add(this.labelHeader);
            this.Controls.Add(this.BClose);
            this.Controls.Add(this.panelContent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MainForm";
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
        SmoothLink LLChangeDBConn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BClose;
        private SmoothLabel labelHeader;
        private System.Windows.Forms.PictureBox pictureBoxBathingBall;
        private System.Windows.Forms.Panel panelContent;


    }
}