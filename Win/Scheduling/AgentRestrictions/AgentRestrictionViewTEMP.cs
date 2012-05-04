using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public partial class AgentRestrictionViewTemp : Form
	{
		public AgentRestrictionViewTemp()
		{
			InitializeComponent();
		}

		private void Button1Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void AgentRestrictionViewTempLoad(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.MergeHeaders();
			agentRestrictionGrid.Model.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}
	}
}
