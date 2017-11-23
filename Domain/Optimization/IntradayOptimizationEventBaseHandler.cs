using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public abstract class IntradayOptimizationEventBaseHandler
	{
		private readonly IntradayOptimization _intradayOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;

		protected IntradayOptimizationEventBaseHandler(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			FillSchedulerStateHolder fillSchedulerStateHolder,
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
		protected virtual void HandleEvent(IntradayOptimizationWasOrdered @event, Guid? planningPeriodId)
		{
			// kind of a hack to keep ref to callers context. 
			// Might end up in this island on same thread as caller. And there we have a rescalc context which we need to hold on to.
			// If ending up here on a new thread (normal case), this should be a noop.
			using (ResourceCalculationCurrent.PreserveContext())
			{
				using (CommandScope.Create(@event))
				{
					var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
					DoOptimization(period, @event.AgentsInIsland, @event.AgentsToOptimize, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule, planningPeriodId);
					_synchronizeSchedulesAfterIsland.Synchronize(_schedulerStateHolder().Schedules, period);
				}
			}
		}

		[TestLog]
		protected virtual void HandleEvent(IntradayOptimizationWasOrdered @event)
		{
			HandleEvent(@event, null);
		}

		[UnitOfWork]
		protected virtual void DoOptimization(
			DateOnlyPeriod period,
			IEnumerable<Guid> agentsInIsland,
			IEnumerable<Guid> agentsToOptimize,
			IEnumerable<LockInfo> locks,
			IEnumerable<Guid> onlyUseSkills,
			bool runResolveWeeklyRestRule,
			Guid? planningPeriodId)
		{
			
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, agentsToOptimize, new LockInfoForStateHolder(_gridlockManager, locks), period, onlyUseSkills);
			_intradayOptimization.Execute(period, schedulerStateHolder.ChoosenAgents.Filter(agentsToOptimize), runResolveWeeklyRestRule, GetBlockPreferenceProvider(planningPeriodId));
		}

		protected abstract IBlockPreferenceProvider GetBlockPreferenceProvider(Guid? planningPeriodId);
	}
}