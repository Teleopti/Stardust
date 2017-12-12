using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationEventHandler : IRunInSyncInFatClientProcess, IHandleEvent<DayOffOptimizationWasOrdered>
	{
		private readonly DayOffOptimization _dayOffOptimization;

		public DayOffOptimizationEventHandler(DayOffOptimization dayOffOptimization)
		{
			_dayOffOptimization = dayOffOptimization;
		}
		
		public void Handle(DayOffOptimizationWasOrdered @event)
		{
			using (CommandScope.Create(@event))
			{
				_dayOffOptimization.Execute(new DateOnlyPeriod(@event.StartDate, @event.EndDate), 
					@event.Agents, 
					new NoSchedulingProgress(), 
					@event.RunWeeklyRestSolver, 
					@event.PlanningPeriodId,
					null);
			}		
		}
	}
}