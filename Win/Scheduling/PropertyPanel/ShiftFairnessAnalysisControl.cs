using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftFairnessAnalysisControl : BaseUserControl
    {
        private Label label1;
        private Label label2;
        private Label label3;
        private TableLayoutPanel shiftFairnessTableLayoutPanel;
	    private IDistributionInformationExtractor _model;
        private ShiftFairnessGrid _shiftFairnessGrid;
        private ShiftPerAgentGrid _shiftPerAgentGrid;

        public ShiftFairnessAnalysisControl( ISchedulerStateHolder schedulerState)
        {
            initializeComponent();
            _shiftFairnessGrid = new ShiftFairnessGrid();
            _shiftPerAgentGrid = new ShiftPerAgentGrid(schedulerState);
            _shiftFairnessGrid.Dock = DockStyle.Fill;
            _shiftPerAgentGrid.Dock = DockStyle.Fill;
            shiftFairnessTableLayoutPanel.Controls.Add(_shiftFairnessGrid,0,3);
            shiftFairnessTableLayoutPanel.Controls.Add(_shiftPerAgentGrid, 0, 5);
        }

        public void UpdateModel(IDistributionInformationExtractor distributionInformationExtractor)
        {
            _model = distributionInformationExtractor;
            _shiftFairnessGrid.UpdateModel(_model);
            _shiftPerAgentGrid.UpdateModel(_model);
        }

        private void initializeComponent()
        {
            shiftFairnessTableLayoutPanel = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            shiftFairnessTableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // shiftFairnessTableLayoutPanel
            // 
            shiftFairnessTableLayoutPanel.ColumnCount = 1;
            shiftFairnessTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            shiftFairnessTableLayoutPanel.Controls.Add(label1, 0, 0);
            shiftFairnessTableLayoutPanel.Controls.Add(label2, 0, 2);
            shiftFairnessTableLayoutPanel.Controls.Add(label3, 0, 4);
            shiftFairnessTableLayoutPanel.Dock = DockStyle.Fill;
            shiftFairnessTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            shiftFairnessTableLayoutPanel.Name = "shiftFairnessTableLayoutPanel";
			shiftFairnessTableLayoutPanel.RowCount = 6;
            shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
			shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            shiftFairnessTableLayoutPanel.Size = new System.Drawing.Size(150, 150);
            shiftFairnessTableLayoutPanel.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(96, 13);
            label1.TabIndex = 0;
	        label1.Text = UserTexts.Resources.PerShiftCategory;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 47);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(77, 13);
            label2.TabIndex = 1;
	        label2.Text = UserTexts.Resources.OverView;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 94);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(121, 13);
			label3.TabIndex = 2;
            label3.Text = UserTexts.Resources.PerAgent;
            // 
            // ShiftFairnessAnalysisControl
            // 
            Controls.Add(shiftFairnessTableLayoutPanel);
            Name = "ShiftFairnessAnalysisControl";
            shiftFairnessTableLayoutPanel.ResumeLayout(false);
            shiftFairnessTableLayoutPanel.PerformLayout();
            ResumeLayout(false);

        }
    }
}
