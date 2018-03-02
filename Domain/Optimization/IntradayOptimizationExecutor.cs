using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationExecutor
	{
		private readonly IntradayOptimization _intradayOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;

		public IntradayOptimizationExecutor(IntradayOptimization intradayOptimization,
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
		public virtual void HandleEvent(IBlockPreferenceProvider blockPreferenceProvider, IntradayOptimizationWasOrdered @event, Guid? planningPeriodId)
		{
			var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			DoOptimization(blockPreferenceProvider, period, @event.AgentsInIsland, @event.Agents, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule, planningPeriodId);
			_synchronizeSchedulesAfterIsland.Synchronize(_schedulerStateHolder().Schedules, period);
		}

		[ReadonlyUnitOfWork]
		protected virtual void DoOptimization(
			IBlockPreferenceProvider blockPreferenceProvider,
			DateOnlyPeriod period,
			IEnumerable<Guid> agentsInIsland,
			IEnumerable<Guid> agentsToOptimize,
			IEnumerable<LockInfo> locks,
			IEnumerable<Guid> onlyUseSkills,
			bool runResolveWeeklyRestRule,
			Guid? planningPeriodId)
		{
			
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, new LockInfoForStateHolder(_gridlockManager, locks), period, onlyUseSkills);
			_intradayOptimization.Execute(period, schedulerStateHolder.ChoosenAgents.Filter(agentsToOptimize), runResolveWeeklyRestRule, blockPreferenceProvider);
		}
	}
}