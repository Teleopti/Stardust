using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public partial class AgentRestrictionViewTemp : Form
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IList<IPerson> _persons;

		public AgentRestrictionViewTemp(ISchedulerStateHolder stateHolder, IList<IPerson> persons)
		{
			InitializeComponent();
			_stateHolder = stateHolder;
			_persons = persons;
		}

		private void Button1Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void AgentRestrictionViewTempLoad(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.MergeHeaders();
			agentRestrictionGrid.LoadData(_stateHolder, _persons);
			agentRestrictionGrid.Refresh();
			agentRestrictionGrid.Model.ColWidths.ResizeToFit(GridRangeInfo.Col(0), GridResizeToFitOptions.IncludeCellsWithinCoveredRange);	
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.BeginUpdate();
			agentRestrictionGrid.MergeCells(2, true);
			agentRestrictionGrid.MergeCells(3, true);
			agentRestrictionGrid.EndUpdate();
			agentRestrictionGrid.Refresh();
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			Refresh();
		}
	}
}
