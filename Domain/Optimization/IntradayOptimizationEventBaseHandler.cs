using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public abstract class IntradayOptimizationEventBaseHandler
	{
		private readonly IntradayOptimization _intradayOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeIntradayOptimizationResult _synchronizeIntradayOptimizationResult;
		private readonly IGridlockManager _gridlockManager;
		private readonly IFillStateHolderWithMaxSeatSkills _fillStateHolderWithMaxSeatSkills;

		protected IntradayOptimizationEventBaseHandler(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IFillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult,
			IGridlockManager gridlockManager,
			IFillStateHolderWithMaxSeatSkills fillStateHolderWithMaxSeatSkills)
		{
			_intradayOptimization = intradayOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeIntradayOptimizationResult = synchronizeIntradayOptimizationResult;
			_gridlockManager = gridlockManager;
			_fillStateHolderWithMaxSeatSkills = fillStateHolderWithMaxSeatSkills;
		}

		[TestLog]
		protected virtual void HandleEvent(OptimizationWasOrdered @event)
		{
			using (CommandScope.Create(@event))
			{
				var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
				DoOptimization(period, @event.AgentsInIsland, @event.AgentsToOptimize, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule);
				_synchronizeIntradayOptimizationResult.Synchronize(_schedulerStateHolder().Schedules, period);
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
			_fillStateHolderWithMaxSeatSkills.Execute(schedulerStateHolder.SchedulingResultState.MinimumSkillIntervalLength());
			_intradayOptimization.Execute(period, schedulerStateHolder.AllPermittedPersons.Filter(agentsToOptimize), runResolveWeeklyRestRule);
		}
	}
}