namespace Teleopti.Ccc.Win.Settings
{
    partial class AdminNavigationPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanelLinks = new System.Windows.Forms.TableLayoutPanel();
            this.linkLabelPermissions = new System.Windows.Forms.LinkLabel();
            this.linkLabelShifts = new System.Windows.Forms.LinkLabel();
            this.linkLabelSettings = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanelLinks.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelLinks
            // 
            this.tableLayoutPanelLinks.ColumnCount = 1;
            this.tableLayoutPanelLinks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelLinks.Controls.Add(this.linkLabelPermissions, 0, 0);
            this.tableLayoutPanelLinks.Controls.Add(this.linkLabelShifts, 0, 2);
            this.tableLayoutPanelLinks.Controls.Add(this.linkLabelSettings, 0, 1);
            this.tableLayoutPanelLinks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelLinks.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelLinks.Name = "tableLayoutPanelLinks";
            this.tableLayoutPanelLinks.RowCount = 5;
            this.tableLayoutPanelLinks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanelLinks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanelLinks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanelLinks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanelLinks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelLinks.Size = new System.Drawing.Size(193, 560);
            this.tableLayoutPanelLinks.TabIndex = 6;
            // 
            // linkLabelPermissions
            // 
            this.linkLabelPermissions.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.linkLabelPermissions.AutoSize = true;
            this.linkLabelPermissions.BackColor = System.Drawing.Color.Transparent;
            this.linkLabelPermissions.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelPermissions.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelPermissions.LinkColor = System.Drawing.Color.DodgerBlue;
            this.linkLabelPermissions.Location = new System.Drawing.Point(3, 3);
            this.linkLabelPermissions.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelPermissions.Name = "linkLabelPermissions";
            this.linkLabelPermissions.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
            this.linkLabelPermissions.Size = new System.Drawing.Size(84, 18);
            this.linkLabelPermissions.TabIndex = 4;
            this.linkLabelPermissions.TabStop = true;
            this.linkLabelPermissions.Text = "xxPermissions";
            // 
            // linkLabelShifts
            // 
            this.linkLabelShifts.AutoSize = true;
            this.linkLabelShifts.BackColor = System.Drawing.Color.Transparent;
            this.linkLabelShifts.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelShifts.LinkColor = System.Drawing.Color.DodgerBlue;
            this.linkLabelShifts.Location = new System.Drawing.Point(3, 51);
            this.linkLabelShifts.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelShifts.Name = "linkLabelShifts";
            this.linkLabelShifts.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
            this.linkLabelShifts.Size = new System.Drawing.Size(56, 18);
            this.linkLabelShifts.TabIndex = 5;
            this.linkLabelShifts.TabStop = true;
            this.linkLabelShifts.Text = "xxShifts";
            // 
            // linkLabelSettings
            // 
            this.linkLabelSettings.AutoSize = true;
            this.linkLabelSettings.BackColor = System.Drawing.Color.Transparent;
            this.linkLabelSettings.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelSettings.LinkColor = System.Drawing.Color.DodgerBlue;
            this.linkLabelSettings.Location = new System.Drawing.Point(3, 27);
            this.linkLabelSettings.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelSettings.Name = "linkLabelSettings";
            this.linkLabelSettings.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
            this.linkLabelSettings.Size = new System.Drawing.Size(66, 18);
            this.linkLabelSettings.TabIndex = 5;
            this.linkLabelSettings.TabStop = true;
            this.linkLabelSettings.Text = "xxOptions";
            this.linkLabelSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSettings_LinkClicked);
            // 
            // AdminNavigationPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelLinks);
            this.Name = "AdminNavigationPanel";
            this.Size = new System.Drawing.Size(193, 560);
            this.tableLayoutPanelLinks.ResumeLayout(false);
            this.tableLayoutPanelLinks.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabelShifts;
        private System.Windows.Forms.LinkLabel linkLabelPermissions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelLinks;
        private System.Windows.Forms.LinkLabel linkLabelSettings;

    }
}
