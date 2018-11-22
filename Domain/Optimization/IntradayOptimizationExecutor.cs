using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourcePlanner;
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
		private readonly IBlockPreferenceProviderForPlanningPeriod _blockPreferenceProviderForPlanningPeriod;
		private readonly DeadLockRetrier _deadLockRetrier;
		private readonly IPlanningGroupSettingsProvider _planningGroupSettingsProvider;

		public IntradayOptimizationExecutor(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			FillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
			IGridlockManager gridlockManager,
			IBlockPreferenceProviderForPlanningPeriod blockPreferenceProviderForPlanningPeriod,
			DeadLockRetrier deadLockRetrier,
			IPlanningGroupSettingsProvider planningGroupSettingsProvider)
		{
			_intradayOptimization = intradayOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
			_blockPreferenceProviderForPlanningPeriod = blockPreferenceProviderForPlanningPeriod;
			_deadLockRetrier = deadLockRetrier;
			_planningGroupSettingsProvider = planningGroupSettingsProvider;
		}

		[TestLog]
		public virtual void HandleEvent(IntradayOptimizationWasOrdered @event)
		{
			_deadLockRetrier.RetryOnDeadlock(() =>
			{
				var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
				DoOptimization(period, @event.AgentsInIsland, @event.Agents, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule, @event.PlanningPeriodId);
				_synchronizeSchedulesAfterIsland.Synchronize(_schedulerStateHolder().Schedules, period);
			});
		}
		
		[ReadonlyUnitOfWork]
		protected virtual void DoOptimization(
			DateOnlyPeriod period,
			IEnumerable<Guid> agentsInIsland,
			IEnumerable<Guid> agentsToOptimize,
			IEnumerable<LockInfo> locks,
			IEnumerable<Guid> onlyUseSkills,
			bool runResolveWeeklyRestRule,
			Guid planningPeriodId)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var planningGroup = _planningGroupSettingsProvider.Execute(planningPeriodId);
			var blockPreferenceProvider = _blockPreferenceProviderForPlanningPeriod.Fetch(planningGroup);
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, new LockInfoForStateHolder(_gridlockManager, locks), period, onlyUseSkills);
			_intradayOptimization.Execute(period, schedulerStateHolder.ChoosenAgents.Filter(agentsToOptimize), runResolveWeeklyRestRule, blockPreferenceProvider);
		}
	}
}