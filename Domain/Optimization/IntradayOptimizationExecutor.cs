using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationExecutor
	{
		private const int maxNumberOfTries = 3;
		private static readonly ILog log = LogManager.GetLogger(typeof(IntradayOptimizationExecutor));
		
		private readonly IntradayOptimization _intradayOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;
		private readonly IBlockPreferenceProviderForPlanningPeriod _blockPreferenceProviderForPlanningPeriod;

		public IntradayOptimizationExecutor(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			FillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
			IGridlockManager gridlockManager,
			IBlockPreferenceProviderForPlanningPeriod blockPreferenceProviderForPlanningPeriod)
		{
			_intradayOptimization = intradayOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
			_blockPreferenceProviderForPlanningPeriod = blockPreferenceProviderForPlanningPeriod;
		}

		//if below is needed on more places - make an attribute/"something" instead?
		[TestLog]
		public virtual void HandleEvent(IntradayOptimizationWasOrdered @event)
		{
			var numberOfTries = 0;
			while (true)
			{
				try
				{
					numberOfTries++;
					var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
					DoOptimization(period, @event.AgentsInIsland, @event.Agents, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule, @event.PlanningPeriodId);
					_synchronizeSchedulesAfterIsland.Synchronize(_schedulerStateHolder().Schedules, period);
					return;
				}
				catch (DeadLockVictimException deadLockEx)
				{
					if (numberOfTries < maxNumberOfTries)
					{
						log.Warn($"Deadlock during intraday optimization. Attempt {numberOfTries} - retrying... {deadLockEx}");	
					}
					else
					{
						log.Warn($"Deadlock during intraday optimization. Attempt {numberOfTries} - giving up... {deadLockEx}");
						throw;
					}
				}
			}
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
			var blockPreferenceProvider = _blockPreferenceProviderForPlanningPeriod.Fetch(planningPeriodId);
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, new LockInfoForStateHolder(_gridlockManager, locks), period, onlyUseSkills);
			_intradayOptimization.Execute(period, schedulerStateHolder.ChoosenAgents.Filter(agentsToOptimize), runResolveWeeklyRestRule, blockPreferenceProvider);
		}
	}
}