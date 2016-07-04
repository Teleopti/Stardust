using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationEventHandler : IHandleEvent<OptimizationWasOrdered>, IRunInProcess
	{
		private readonly IntradayOptimization _intradayOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeIntradayOptimizationResult _synchronizeIntradayOptimizationResult;
		private readonly IGridlockManager _gridlockManager;

		public IntradayOptimizationEventHandler(IntradayOptimization intradayOptimization,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									IFillSchedulerStateHolder fillSchedulerStateHolder,
									ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult,
									IGridlockManager gridlockManager)
		{
			_intradayOptimization = intradayOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeIntradayOptimizationResult = synchronizeIntradayOptimizationResult;
			_gridlockManager = gridlockManager;
		}

		[LogTime]
		public virtual void Handle(OptimizationWasOrdered @event)
		{
			using (CommandScope.Create(@event))
			{
				DoOptimization(@event.Period, @event.AgentsInIsland, @event.AgentsToOptimize, @event.UserLocks, @event.RunResolveWeeklyRestRule);
				_synchronizeIntradayOptimizationResult.Synchronize(_schedulerStateHolder().Schedules, @event.Period);
			}
		}

		[UnitOfWork]
		protected virtual void DoOptimization(DateOnlyPeriod period, IEnumerable<Guid> agentsInIsland, IEnumerable<Guid> agentsToOptimize, IEnumerable<LockInfo> locks, bool runResolveWeeklyRestRule)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, _gridlockManager, locks, period);
			_intradayOptimization.Execute(period, schedulerStateHolder.AllPermittedPersons.Filter(agentsToOptimize), runResolveWeeklyRestRule);
		}
	}
}