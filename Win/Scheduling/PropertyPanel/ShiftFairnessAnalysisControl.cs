using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftFairnessAnalysisControl : BaseUserControl
    {
        private Label label2;
        private TableLayoutPanel shiftFairnessTableLayoutPanel;
	    private IDistributionInformationExtractor _model;
        private readonly ShiftFairnessGrid _shiftFairnessGrid;
        private readonly PerShiftCategoryChartControl _perShiftCategoryChartControl;

        public ShiftFairnessAnalysisControl()
        {
            initializeComponent();
            _perShiftCategoryChartControl = new PerShiftCategoryChartControl {Dock = DockStyle.Fill};
	        _shiftFairnessGrid = new ShiftFairnessGrid {Dock = DockStyle.Fill};

            shiftFairnessTableLayoutPanel.Controls.Add(_perShiftCategoryChartControl, 0, 0);
            shiftFairnessTableLayoutPanel.Controls.Add(_shiftFairnessGrid,0,2);
        }

        public void UpdateModel(IDistributionInformationExtractor distributionInformationExtractor)
        {
            _model = distributionInformationExtractor;
            _shiftFairnessGrid.UpdateModel(_model);
            _perShiftCategoryChartControl.UpdateModel(_model);
        }

        private void initializeComponent()
        {
            shiftFairnessTableLayoutPanel = new TableLayoutPanel();
            label2 = new Label();
            shiftFairnessTableLayoutPanel.SuspendLayout();
            SuspendLayout();

            shiftFairnessTableLayoutPanel.ColumnCount = 1;
            shiftFairnessTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            shiftFairnessTableLayoutPanel.Controls.Add(label2, 0, 1);
            shiftFairnessTableLayoutPanel.Dock = DockStyle.Fill;
            shiftFairnessTableLayoutPanel.Location = new Point(0, 0);
            shiftFairnessTableLayoutPanel.Name = "shiftFairnessTableLayoutPanel";
			shiftFairnessTableLayoutPanel.RowCount = 3; 

            shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
			shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            shiftFairnessTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            shiftFairnessTableLayoutPanel.Size = new Size(150, 150);
            shiftFairnessTableLayoutPanel.TabIndex = 1;
            //
			// label2
            // 
			label2.AutoSize = true;
			label2.Location = new Point(3, 47);
			label2.Name = "label2";
			label2.Size = new Size(77, 13);
			label2.TabIndex = 1;
			label2.Text = UserTexts.Resources.OverView;
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
