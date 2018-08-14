using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[EnabledBy(Toggles.ResourcePlanner_MergeSchedulingAndDO_76496)]
	public class SchedulingAndDayOffOptimization :
		IHandleEvent<SchedulingAndDayOffWasOrdered>,
		IRunOnStardust
	{
		private readonly FullScheduling _fullScheduling;
		private readonly DayOffOptimizationWeb _dayOffOptimizationWeb;

		public SchedulingAndDayOffOptimization(FullScheduling fullScheduling, DayOffOptimizationWeb dayOffOptimizationWeb)
		{
			_fullScheduling = fullScheduling;
			_dayOffOptimizationWeb = dayOffOptimizationWeb;
		}

		public void Handle(SchedulingAndDayOffWasOrdered @event)
		{
			_fullScheduling.DoScheduling(@event.PlanningPeriodId);
			_dayOffOptimizationWeb.Execute(@event.PlanningPeriodId);
		}
	}
}