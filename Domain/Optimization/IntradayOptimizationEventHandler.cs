using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationEventHandler : IHandleEvent<OptimizationWasOrdered>, IRunInProcess
	{
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IntradayOptimizer2Creator _intradayOptimizer2Creator;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;
		private readonly IntradayOptimizationContext _intradayOptimizationContext;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeIntradayOptimizationResult _synchronizeIntradayOptimizationResult;

		public IntradayOptimizationEventHandler(OptimizationPreferencesFactory optimizationPreferencesFactory,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
									WeeklyRestSolverExecuter weeklyRestSolverExecuter,
									IntradayOptimizer2Creator intradayOptimizer2Creator,
									IIntradayOptimizerContainer intradayOptimizerContainer,
									IntradayOptimizationContext intradayOptimizationContext,
									IFillSchedulerStateHolder fillSchedulerStateHolder,
									ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult)
		{
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_intradayOptimizerContainer = intradayOptimizerContainer;
			_intradayOptimizationContext = intradayOptimizationContext;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeIntradayOptimizationResult = synchronizeIntradayOptimizationResult;
		}

		[LogTime]
		public virtual void Handle(OptimizationWasOrdered @event)
		{
			DoOptimization(@event.Period, @event.AgentIds);
			_synchronizeIntradayOptimizationResult.Execute(_schedulerStateHolder().Schedules);
		}

		[UnitOfWork]
		protected virtual void DoOptimization(DateOnlyPeriod period, IEnumerable<Guid> agentIds)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, period);
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffPreferencesProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()); //doesn't seem to be used with "real" values when doing intraday optimization
			var agents = schedulerStateHolder.AllPermittedPersons;
			if (agentIds != null)
			{
				agents = agents.Where(x => agentIds.Contains(x.Id.Value)).ToList();
			}
			var schedules = new List<IScheduleDay>();
			foreach (var person in agents)
			{
				schedules.AddRange(schedulerStateHolder.Schedules[person].ScheduledDayCollection(period));
			}

			var optimizers = _intradayOptimizer2Creator.Create(period, schedules, optimizationPreferences, dayOffPreferencesProvider);

			using (_intradayOptimizationContext.Create(period))
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);
				_intradayOptimizerContainer.Execute(optimizers);
				_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, schedules, agents, dayOffPreferencesProvider);
			}
		}
	}
}