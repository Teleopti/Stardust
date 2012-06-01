using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public partial class AgentRestrictionViewTemp : Form
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IList<IPerson> _persons;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly IRuleSetProjectionService _projectionService;

		public AgentRestrictionViewTemp(ISchedulerStateHolder stateHolder, IList<IPerson> persons, ISchedulingOptions schedulingOptions, IRuleSetProjectionService projectionService)
		{
			InitializeComponent();
			_stateHolder = stateHolder;
			_persons = persons;
			_schedulingOptions = schedulingOptions;
			_projectionService = projectionService;
		}

		private void Button1Click(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.Dispose();
			Close();
		}

		private void AgentRestrictionViewTempLoad(object sender, System.EventArgs e)
		{
			agentRestrictionGrid.MergeHeaders();
			agentRestrictionGrid.LoadData(_stateHolder, _persons, _schedulingOptions, _projectionService);
			agentRestrictionGrid.Refresh();	
		}
	}
}
