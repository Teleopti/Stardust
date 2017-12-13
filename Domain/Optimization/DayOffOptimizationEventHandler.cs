using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationEventHandler : IRunInSyncInFatClientProcess, IHandleEvent<DayOffOptimizationWasOrdered>
	{
		private readonly DayOffOptimization _dayOffOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;

		public DayOffOptimizationEventHandler(DayOffOptimization dayOffOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			FillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
			IGridlockManager gridlockManager)
		{
			_dayOffOptimization = dayOffOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
		}
		
		public void Handle(DayOffOptimizationWasOrdered @event)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			using (CommandScope.Create(@event))
			{
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.AgentsInIsland, new LockInfoForStateHolder(_gridlockManager, @event.UserLocks), selectedPeriod, @event.Skills);
				_dayOffOptimization.Execute(new DateOnlyPeriod(@event.StartDate, @event.EndDate), 
					schedulerStateHolder.ChoosenAgents.Where(x => @event.Agents.Contains(x.Id.Value)).ToArray(), 
					new NoSchedulingProgress(), 
					@event.RunWeeklyRestSolver, 
					@event.PlanningPeriodId,
					null);
				_synchronizeSchedulesAfterIsland.Synchronize(schedulerStateHolder.Schedules, selectedPeriod);
			}		
		}
	}
}