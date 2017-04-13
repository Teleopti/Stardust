namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    partial class GridWorkspace
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
            this.zoneWorkspace = new Microsoft.Practices.CompositeUI.WPF.ZoneWorkspace();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.zoneWorkspace)).BeginInit();
            this.zoneWorkspace.SuspendLayout();
            this.SuspendLayout();
            // 
            // zoneWorkspace
            // 
            this.zoneWorkspace.Controls.Add(this.tableLayoutPanel);
            this.zoneWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoneWorkspace.Location = new System.Drawing.Point(0, 0);
            this.zoneWorkspace.Name = "zoneWorkspace";
            this.zoneWorkspace.Size = new System.Drawing.Size(1270, 865);
            this.zoneWorkspace.TabIndex = 0;
            this.zoneWorkspace.TabStop = true;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.tableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(1270, 865);
            this.tableLayoutPanel.TabIndex = 0;
            this.tableLayoutPanel.TabStop = true;
            this.zoneWorkspace.SetZoneName(this.tableLayoutPanel, "zoneWorkSpace");
            // 
            // WorkSpaceLayOut
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.zoneWorkspace);
            this.Name = "WorkSpaceLayOut";
            this.Size = new System.Drawing.Size(1270, 865);
            this.Tag = "0";
            ((System.ComponentModel.ISupportInitialize)(this.zoneWorkspace)).EndInit();
            this.zoneWorkspace.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Practices.CompositeUI.WPF.ZoneWorkspace zoneWorkspace;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    }
}
