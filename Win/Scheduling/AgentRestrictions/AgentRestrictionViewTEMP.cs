using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

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
			agentRestrictionGrid.LoadData();
			agentRestrictionGrid.Refresh();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.BeginUpdate();
			agentRestrictionGrid.FinishedTest = true;
			agentRestrictionGrid.MergeCells(2, true);
			agentRestrictionGrid.MergeCells(3, true);
			agentRestrictionGrid.EndUpdate();
			agentRestrictionGrid.Refresh();
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.FinishedTest = false;
			Refresh();
		}
	}
}
