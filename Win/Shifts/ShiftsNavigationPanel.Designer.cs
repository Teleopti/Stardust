namespace Teleopti.Ccc.Win.Shifts
{
    partial class ShiftsNavigationPanel
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
            this.linkLabelShifts = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanelLinks.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelLinks
            // 
            this.tableLayoutPanelLinks.ColumnCount = 1;
            this.tableLayoutPanelLinks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelLinks.Controls.Add(this.linkLabelShifts, 0, 0);
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
            this.tableLayoutPanelLinks.TabIndex = 7;
            // 
            // linkLabelShifts
            // 
            this.linkLabelShifts.AutoSize = true;
            this.linkLabelShifts.BackColor = System.Drawing.Color.Transparent;
            this.linkLabelShifts.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelShifts.LinkColor = System.Drawing.Color.DodgerBlue;
            this.linkLabelShifts.Location = new System.Drawing.Point(3, 3);
            this.linkLabelShifts.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelShifts.Name = "linkLabelShifts";
            this.linkLabelShifts.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
            this.linkLabelShifts.Size = new System.Drawing.Size(53, 18);
            this.linkLabelShifts.TabIndex = 5;
            this.linkLabelShifts.TabStop = true;
            this.linkLabelShifts.Text = "xxShifts";
            this.linkLabelShifts.Visible = false;
            this.linkLabelShifts.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelShifts_LinkClicked);
            // 
            // ShiftsNavigationPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelLinks);
            this.Name = "ShiftsNavigationPanel";
            this.Size = new System.Drawing.Size(193, 560);
            this.tableLayoutPanelLinks.ResumeLayout(false);
            this.tableLayoutPanelLinks.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelLinks;
        private System.Windows.Forms.LinkLabel linkLabelShifts;

    }
}