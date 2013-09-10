using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftPerAgentControl : BaseUserControl
    {
        private readonly ShiftPerAgentGrid  _shiftPerAgentGrid ;
        private TableLayoutPanel tableLayoutPanelPerAgent;

        public ShiftPerAgentControl(ISchedulerStateHolder schedulerState)
        {
            initializeComponent();
            _shiftPerAgentGrid = new ShiftPerAgentGrid(schedulerState) {Dock = DockStyle.Fill};
            tableLayoutPanelPerAgent.Controls.Add(_shiftPerAgentGrid, 0, 1);
            
        }

        public void UpdateModel(IDistributionInformationExtractor model)
        {
            _shiftPerAgentGrid.UpdateModel(model);
        }

        private void initializeComponent()
        {
            tableLayoutPanelPerAgent = new TableLayoutPanel();
            SuspendLayout();
            // 
            // tableLayoutPanelPerAgent
            // 
            tableLayoutPanelPerAgent.ColumnCount = 1;
            tableLayoutPanelPerAgent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelPerAgent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanelPerAgent.Dock = DockStyle.Fill;
            tableLayoutPanelPerAgent.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanelPerAgent.Name = "tableLayoutPanelPerAgent";
            tableLayoutPanelPerAgent.RowCount = 2;
            tableLayoutPanelPerAgent.RowStyles.Add(new RowStyle(SizeType.Absolute, 21F));
            tableLayoutPanelPerAgent.RowStyles.Add(new RowStyle());
            tableLayoutPanelPerAgent.Size = new System.Drawing.Size(150, 150);
            tableLayoutPanelPerAgent.TabIndex = 0;
            // 
            // ShiftPerAgentControl
            // 
            Controls.Add(tableLayoutPanelPerAgent);
            Name = "ShiftPerAgentControl";
            ResumeLayout(false);
        }    
    }
}

