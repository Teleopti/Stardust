using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftDistributionControl : BaseUserControl
    {
        private TableLayoutPanel shiftDistributiontableLayoutPanel;
        private readonly ShiftDistributionGrid _shiftDistributionGrid;

        public ShiftDistributionControl()
        {
            initializeComponent();
            _shiftDistributionGrid = new ShiftDistributionGrid {Dock = DockStyle.Fill};
            shiftDistributiontableLayoutPanel.Controls.Add(_shiftDistributionGrid,0,1);
        }

        public void UpdateModel(IDistributionInformationExtractor model)
        {
            _shiftDistributionGrid.UpdateModel(model );
        }

        private void initializeComponent()
        {
            shiftDistributiontableLayoutPanel = new TableLayoutPanel();
            SuspendLayout();
            // 
            // shiftDistributiontableLayoutPanel
            // 
            shiftDistributiontableLayoutPanel.ColumnCount = 1;
            shiftDistributiontableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            shiftDistributiontableLayoutPanel.Dock = DockStyle.Fill;
            shiftDistributiontableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            shiftDistributiontableLayoutPanel.Name = "shiftDistributiontableLayoutPanel";
            shiftDistributiontableLayoutPanel.RowCount = 2;
            shiftDistributiontableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            shiftDistributiontableLayoutPanel.RowStyles.Add(new RowStyle());
            shiftDistributiontableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            shiftDistributiontableLayoutPanel.Size = new System.Drawing.Size(150, 150);
            shiftDistributiontableLayoutPanel.TabIndex = 0;
            // 
            // ShiftDistributionControl
            // 
            Controls.Add(shiftDistributiontableLayoutPanel);
            Name = "ShiftDistributionControl";
            ResumeLayout(false);
        }
    }
}
