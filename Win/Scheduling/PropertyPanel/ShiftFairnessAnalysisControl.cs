using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftFairnessAnalysisControl : BaseUserControl
    {
        private System.Windows.Forms.TableLayoutPanel shiftFairnessTableLayoutPanel;

        public ShiftFairnessAnalysisControl()
        {
            InitializeComponent();
            var shiftFairnessGrid = new ShiftFairnessGrid();
            var shiftPerAgentGrid = new ShiftPerAgentGrid();
            shiftFairnessGrid.Dock = DockStyle.Fill;
            shiftPerAgentGrid.Dock = DockStyle.Fill;
            shiftFairnessTableLayoutPanel.Controls.Add(shiftFairnessGrid,0,1);
            shiftFairnessTableLayoutPanel.Controls.Add(shiftPerAgentGrid, 0, 2);
        }

        private void InitializeComponent()
        {
            this.shiftFairnessTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // shiftFairnessTableLayoutPanel
            // 
            this.shiftFairnessTableLayoutPanel.ColumnCount = 1;
            this.shiftFairnessTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.shiftFairnessTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shiftFairnessTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.shiftFairnessTableLayoutPanel.Name = "shiftFairnessTableLayoutPanel";
            this.shiftFairnessTableLayoutPanel.RowCount = 3;
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.shiftFairnessTableLayoutPanel.Size = new System.Drawing.Size(150, 150);
            this.shiftFairnessTableLayoutPanel.TabIndex = 1;
            // 
            // ShiftFairnessAnalysisControl
            // 
            this.Controls.Add(this.shiftFairnessTableLayoutPanel);
            this.Name = "ShiftFairnessAnalysisControl";
            this.ResumeLayout(false);

        }
    }
}
