using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Infrastructure;
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

		//if below is needed on more places - make an attribute/"something" instead?
		[TestLog]
		public virtual void HandleEvent(IntradayOptimizationWasOrdered @event, Guid? planningPeriodId, Func<IBlockPreferenceProvider> blockPreferenceProvider)
		{
			var numberOfTries = 0;
			while (true)
			{
				try
				{
					numberOfTries++;
					execute(@event, planningPeriodId, blockPreferenceProvider);
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
		

		private void execute(IntradayOptimizationWasOrdered @event, Guid? planningPeriodId, Func<IBlockPreferenceProvider> blockPreferenceProvider)
		{
			var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
			DoOptimization(blockPreferenceProvider, period, @event.AgentsInIsland, @event.Agents, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule, planningPeriodId);
			_synchronizeSchedulesAfterIsland.Synchronize(_schedulerStateHolder().Schedules, period);
		}

		[ReadonlyUnitOfWork]
		protected virtual void DoOptimization(
			Func<IBlockPreferenceProvider> blockPreferenceProvider,
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
			_intradayOptimization.Execute(period, schedulerStateHolder.ChoosenAgents.Filter(agentsToOptimize), runResolveWeeklyRestRule, blockPreferenceProvider());
		}
	}
}