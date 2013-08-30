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
            this.tableLayoutPanelPerAgent = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // tableLayoutPanelPerAgent
            // 
            this.tableLayoutPanelPerAgent.ColumnCount = 1;
            this.tableLayoutPanelPerAgent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelPerAgent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPerAgent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelPerAgent.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelPerAgent.Name = "tableLayoutPanelPerAgent";
            this.tableLayoutPanelPerAgent.RowCount = 2;
            this.tableLayoutPanelPerAgent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanelPerAgent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelPerAgent.Size = new System.Drawing.Size(150, 150);
            this.tableLayoutPanelPerAgent.TabIndex = 0;
            // 
            // ShiftPerAgentControl
            // 
            this.Controls.Add(this.tableLayoutPanelPerAgent);
            this.Name = "ShiftPerAgentControl";
            this.ResumeLayout(false);



        }

        
    }
}

