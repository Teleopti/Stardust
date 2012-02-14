namespace Teleopti.Ccc.Win.Reporting
{
    partial class ReportSettingsHostView
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.reportHeader1 = new Teleopti.Ccc.Win.Reporting.ReportHeader();
            this.panelSettingsContainer = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.reportHeader1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelSettingsContainer, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(656, 598);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // reportHeader1
            // 
            this.reportHeader1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.reportHeader1.HeaderText = "autoLabel1";
            this.reportHeader1.Location = new System.Drawing.Point(3, 3);
            this.reportHeader1.Name = "reportHeader1";
            this.reportHeader1.ReportName = null;
            this.reportHeader1.Size = new System.Drawing.Size(650, 32);
            this.reportHeader1.TabIndex = 10;
            // 
            // panelSettingsContainer
            // 
            this.panelSettingsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSettingsContainer.Location = new System.Drawing.Point(3, 41);
            this.panelSettingsContainer.Name = "panelSettingsContainer";
            this.panelSettingsContainer.Size = new System.Drawing.Size(650, 554);
            this.panelSettingsContainer.TabIndex = 11;
            // 
            // ReportSettingsHostView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ReportSettingsHostView";
            this.Size = new System.Drawing.Size(656, 598);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ReportHeader reportHeader1;
        private System.Windows.Forms.Panel panelSettingsContainer;

    }
}
