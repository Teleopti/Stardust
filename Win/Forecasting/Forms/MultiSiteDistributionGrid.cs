using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public class MultisiteDistributionGrid : GridControl
    {
        public MultisiteDistributionGrid()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            SuspendLayout();
            // 
            // MultisiteDistributionGrid
            // 
            GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
            GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            ResumeLayout(false);
        }
    }
}