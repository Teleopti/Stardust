using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public abstract class IntradayOptimizationEventBaseHandler
	{
		private readonly IntradayOptimization _intradayOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;

		protected IntradayOptimizationEventBaseHandler(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IFillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
			IGridlockManager gridlockManager)
		{
			_intradayOptimization = intradayOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
		}

		[TestLog]
		protected virtual void HandleEvent(IntradayOptimizationWasOrdered @event)
		{
			using (CommandScope.Create(@event))
			{
				var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
				DoOptimization(period, @event.AgentsInIsland, @event.AgentsToOptimize, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule);
				_synchronizeSchedulesAfterIsland.Synchronize(_schedulerStateHolder().Schedules, period);
			}
		}

		[UnitOfWork]
		protected virtual void DoOptimization(DateOnlyPeriod period,
			IEnumerable<Guid> agentsInIsland,
			IEnumerable<Guid> agentsToOptimize,
			IEnumerable<LockInfo> locks,
			IEnumerable<Guid> onlyUseSkills,
			bool runResolveWeeklyRestRule)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, _gridlockManager, locks, period, onlyUseSkills);
			_intradayOptimization.Execute(period, schedulerStateHolder.AllPermittedPersons.Filter(agentsToOptimize), runResolveWeeklyRestRule);
		}
	}
}