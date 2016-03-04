using System;
using System.Collections.Generic;
using System.Linq;
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
	public class IntradayOptimization : IHandleEvent<OptimizationWasOrdered>, IRunInProcess
	{
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IntradayOptimizer2Creator _intradayOptimizer2Creator;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;
		private readonly IntradayOptimizationContext _intradayOptimizationContext;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;

		public IntradayOptimization(OptimizationPreferencesFactory optimizationPreferencesFactory,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
									Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
									WeeklyRestSolverExecuter weeklyRestSolverExecuter,
									IntradayOptimizer2Creator intradayOptimizer2Creator,
									IIntradayOptimizerContainer intradayOptimizerContainer,
									IntradayOptimizationContext intradayOptimizationContext,
									IFillSchedulerStateHolder fillSchedulerStateHolder)
		{
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_intradayOptimizerContainer = intradayOptimizerContainer;
			_intradayOptimizationContext = intradayOptimizationContext;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
		}

		[LogTime]
		public virtual void Handle(OptimizationWasOrdered @event)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, @event.Period);
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffOptimizationPreference = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var agents = schedulerStateHolder.AllPermittedPersons;
			if (@event.AgentIds != null)
			{
				agents = agents.Where(x => @event.AgentIds.Contains(x.Id.Value)).ToList();				
			}
			var schedules = new List<IScheduleDay>();
			foreach (var person in agents)
			{
				schedules.AddRange(schedulerStateHolder.Schedules[person].ScheduledDayCollection(@event.Period));
			}

			var optimizers = _intradayOptimizer2Creator.Create(@event.Period, schedules, optimizationPreferences, dayOffOptimizationPreference);

			using (_intradayOptimizationContext.Create(@event.Period))
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);
				_intradayOptimizerContainer.Execute(optimizers);
				_weeklyRestSolverExecuter.Resolve(optimizationPreferences, @event.Period, schedules, agents, dayOffOptimizationPreference);
			}
		}
	}
}