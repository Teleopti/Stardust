using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[InstancePerLifetimeScope]
	public class DayOffOptimizationEventHandler : IRunInSyncInFatClientProcess, IHandleEvent<DayOffOptimizationWasOrdered>
	{
		private readonly DeadLockRetrier _deadLockRetrier;
		private readonly DayOffOptimization _dayOffOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;

		public DayOffOptimizationEventHandler(DeadLockRetrier deadLockRetrier, 
			DayOffOptimization dayOffOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			FillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
			IGridlockManager gridlockManager)
		{
			_deadLockRetrier = deadLockRetrier;
			_dayOffOptimization = dayOffOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
		}

		[TestLog]
		public virtual void Handle(DayOffOptimizationWasOrdered @event)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var selectedPeriod = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			
			using (CommandScope.Create(@event))
			{
				_deadLockRetrier.RetryOnDeadlock(() =>
				{
					DoOptimization(@event, schedulerStateHolder, selectedPeriod);
					_synchronizeSchedulesAfterIsland.Synchronize(schedulerStateHolder.Schedules, selectedPeriod);
				});
			}
		}

		[ReadonlyUnitOfWork]
		protected virtual void DoOptimization(DayOffOptimizationWasOrdered @event, ISchedulerStateHolder schedulerStateHolder,
			DateOnlyPeriod selectedPeriod)
		{
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.AgentsInIsland,
				new LockInfoForStateHolder(_gridlockManager, @event.UserLocks), selectedPeriod, @event.Skills);
			_dayOffOptimization.Execute(new DateOnlyPeriod(@event.StartDate, @event.EndDate),
				schedulerStateHolder.SchedulingResultState.LoadedAgents.Where(x => @event.Agents.Contains(x.Id.Value)).ToArray(),
				false,
				Guid.Empty); //To be removed
		}
	}
}