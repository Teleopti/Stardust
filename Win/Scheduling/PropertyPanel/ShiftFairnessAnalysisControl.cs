using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftFairnessAnalysisControl : BaseUserControl
    {
        private Label label1;
        private Label label2;
        private Label label3;
        private System.Windows.Forms.TableLayoutPanel shiftFairnessTableLayoutPanel;
	    private IDistributionInformationExtractor _model;

        public ShiftFairnessAnalysisControl(IDistributionInformationExtractor distributionInformationExtractor)
        {
            InitializeComponent();
	        _model = distributionInformationExtractor;
            var shiftFairnessGrid = new ShiftFairnessGrid();
            var shiftPerAgentGrid = new ShiftPerAgentGrid();
            shiftFairnessGrid.Dock = DockStyle.Fill;
            shiftPerAgentGrid.Dock = DockStyle.Fill;
            shiftFairnessTableLayoutPanel.Controls.Add(shiftFairnessGrid,0,3);
            shiftFairnessTableLayoutPanel.Controls.Add(shiftPerAgentGrid, 0, 5);
        }

        private void InitializeComponent()
        {
            this.shiftFairnessTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.shiftFairnessTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // shiftFairnessTableLayoutPanel
            // 
            this.shiftFairnessTableLayoutPanel.ColumnCount = 1;
            this.shiftFairnessTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.shiftFairnessTableLayoutPanel.Controls.Add(this.label1, 0, 0);
            this.shiftFairnessTableLayoutPanel.Controls.Add(this.label2, 0, 2);
            this.shiftFairnessTableLayoutPanel.Controls.Add(this.label3, 0, 4);
            this.shiftFairnessTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shiftFairnessTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.shiftFairnessTableLayoutPanel.Name = "shiftFairnessTableLayoutPanel";
            this.shiftFairnessTableLayoutPanel.RowCount = 6;
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.shiftFairnessTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.shiftFairnessTableLayoutPanel.Size = new System.Drawing.Size(150, 150);
            this.shiftFairnessTableLayoutPanel.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "xxPerShiftCategory";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "xxShiftFairness";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "xxShiftFairnessPerAgent";
            // 
            // ShiftFairnessAnalysisControl
            // 
            this.Controls.Add(this.shiftFairnessTableLayoutPanel);
            this.Name = "ShiftFairnessAnalysisControl";
            this.shiftFairnessTableLayoutPanel.ResumeLayout(false);
            this.shiftFairnessTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
