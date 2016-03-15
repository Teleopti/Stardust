using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeIntradayOptimizationResult _synchronizeIntradayOptimizationResult;

		public IntradayOptimizationEventHandler(IntradayOptimization intradayOptimization,
									OptimizationPreferencesFactory optimizationPreferencesFactory,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									IFillSchedulerStateHolder fillSchedulerStateHolder,
									ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult)
		{
			_intradayOptimization = intradayOptimization;
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeIntradayOptimizationResult = synchronizeIntradayOptimizationResult;
		}

		[LogTime]
		public virtual void Handle(OptimizationWasOrdered @event)
		{
			DoOptimization(@event.Period, @event.AgentIds, @event.RunResolveWeeklyRestRule);
			_synchronizeIntradayOptimizationResult.Execute(_schedulerStateHolder().Schedules, @event.Period);
		}

		[UnitOfWork]
		protected virtual void DoOptimization(DateOnlyPeriod period, IEnumerable<Guid> agentIds, bool runResolveWeeklyRestRule)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentIds, period);
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var agents = schedulerStateHolder.AllPermittedPersons;
			if (agentIds != null)
			{
				agents = agents.Where(x => agentIds.Contains(x.Id.Value)).ToList();
			}
			_intradayOptimization.Execute(period, agents, optimizationPreferences, runResolveWeeklyRestRule);
		}
	}
}