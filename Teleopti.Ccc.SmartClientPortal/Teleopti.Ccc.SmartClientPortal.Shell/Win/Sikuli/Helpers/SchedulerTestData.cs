using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers
{
	public class SchedulerTestData
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public SchedulerTestData(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public ISchedulerStateHolder SchedulerState
		{
			get { return _schedulerState; }
		}

		public IAggregateSkill TotalSkill
		{
			get { return _totalSkill; }
		}
	}
}
