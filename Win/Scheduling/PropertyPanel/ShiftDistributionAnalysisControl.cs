using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftDistributionAnalysisControl : BaseUserControl
    {
        private TableLayoutPanel shiftDistributiontableLayoutPanel;
        
        public ShiftDistributionAnalysisControl()
        {
            InitializeComponent();
            var shiftDistributionGrid = new ShiftDistributionGrid();
            shiftDistributionGrid.Dock = DockStyle.Fill;
            shiftDistributiontableLayoutPanel.Controls.Add(shiftDistributionGrid);
        }

        private void InitializeComponent()
        {
            this.shiftDistributiontableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // shiftDistributiontableLayoutPanel
            // 
            this.shiftDistributiontableLayoutPanel.ColumnCount = 1;
            this.shiftDistributiontableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.shiftDistributiontableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.shiftDistributiontableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shiftDistributiontableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.shiftDistributiontableLayoutPanel.Name = "shiftDistributiontableLayoutPanel";
            this.shiftDistributiontableLayoutPanel.RowCount = 3;
            this.shiftDistributiontableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.shiftDistributiontableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.shiftDistributiontableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.shiftDistributiontableLayoutPanel.Size = new System.Drawing.Size(150, 150);
            this.shiftDistributiontableLayoutPanel.TabIndex = 0;
            // 
            // ShiftDistributionAnalysisControl
            // 
            this.Controls.Add(this.shiftDistributiontableLayoutPanel);
            this.Name = "ShiftDistributionAnalysisControl";
            this.ResumeLayout(false);

        }
    }
}
