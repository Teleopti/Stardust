using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
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
		private readonly WebSchedulingSetup _webSchedulingSetup;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IntradayOptimizer2Creator _intradayOptimizer2Creator;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;
		private readonly IntradayOptimizationContext _intradayOptimizationContext;

		public IntradayOptimization(OptimizationPreferencesFactory optimizationPreferencesFactory,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
									WebSchedulingSetup webSchedulingSetup,
									Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
									IScheduleDictionaryPersister persister,
									WeeklyRestSolverExecuter weeklyRestSolverExecuter,
									IntradayOptimizer2Creator intradayOptimizer2Creator,
									IIntradayOptimizerContainer intradayOptimizerContainer,
									IntradayOptimizationContext intradayOptimizationContext)
		{
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_webSchedulingSetup = webSchedulingSetup;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_persister = persister;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_intradayOptimizerContainer = intradayOptimizerContainer;
			_intradayOptimizationContext = intradayOptimizationContext;
		}


		public void Handle(OptimizationWasOrdered @event)
		{
			SetupAndOptimize(@event.Period);
			_persister.Persist(_schedulerStateHolder().Schedules);
		}

		[LogTime]
		protected virtual void SetupAndOptimize(DateOnlyPeriod period)
		{
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffOptimizationPreference = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var webScheduleState = _webSchedulingSetup.Setup(period);

			var optimizers = _intradayOptimizer2Creator.Create(period, webScheduleState.AllSchedules, optimizationPreferences, dayOffOptimizationPreference);

			using (_intradayOptimizationContext.Create(period))
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);
				_intradayOptimizerContainer.Execute(optimizers);
				_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, webScheduleState.AllSchedules,
					webScheduleState.PeopleSelection.AllPeople, dayOffOptimizationPreference);
			}
		}
	}
}