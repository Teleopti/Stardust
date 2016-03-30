using System;
using System.Collections.Generic;
using System.Linq;
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

		public IntradayOptimizationEventHandler(IntradayOptimization intradayOptimization,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									IFillSchedulerStateHolder fillSchedulerStateHolder,
									ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult)
		{
			_intradayOptimization = intradayOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeIntradayOptimizationResult = synchronizeIntradayOptimizationResult;
		}

		[LogTime]
		public virtual void Handle(OptimizationWasOrdered @event)
		{
			using (TrackIdentifierScope.Create(@event))
			{
				DoOptimization(@event.Period, @event.AgentsInIsland, @event.AgentsToOptimize, @event.RunResolveWeeklyRestRule);
				_synchronizeIntradayOptimizationResult.Synchronize(_schedulerStateHolder().Schedules, @event.Period);
			}
		}

		[UnitOfWork]
		protected virtual void DoOptimization(DateOnlyPeriod period, IEnumerable<Guid> agentsInIsland, IEnumerable<Guid> agentsToOptimize, bool runResolveWeeklyRestRule)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, period);
			_intradayOptimization.Execute(period, schedulerStateHolder.AllPermittedPersons.Filter(agentsToOptimize).ToList(), runResolveWeeklyRestRule);
		}
	}
}