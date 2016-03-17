using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimization
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IntradayOptimizer2Creator _intradayOptimizer2Creator;
		private readonly IntradayOptimizationContext _intradayOptimizationContext;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;

		public IntradayOptimization(Func<ISchedulerStateHolder> schedulerStateHolder,
			IntradayOptimizer2Creator intradayOptimizer2Creator,
			IntradayOptimizationContext intradayOptimizationContext,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			IIntradayOptimizerContainer intradayOptimizerContainer,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter,
			IOptimizationPreferencesProvider optimizationPreferencesProvider)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_intradayOptimizationContext = intradayOptimizationContext;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_intradayOptimizerContainer = intradayOptimizerContainer;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
		}

		public void Execute(DateOnlyPeriod period, 
												IList<IPerson> agents,
												bool runResolveWeeklyRestRule)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var schedules = schedulerStateHolder.Schedules.SchedulesForPeriod(period, agents);
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			var dayOffPreferencesProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()); //doesn't seem to be used with "real" values when doing intraday optimization
			var optimizers = _intradayOptimizer2Creator.Create(period, schedules, optimizationPreferences, dayOffPreferencesProvider);

			using (_intradayOptimizationContext.Create(period))
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);
				_intradayOptimizerContainer.Execute(optimizers);

				if (runResolveWeeklyRestRule)
				{
					_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, schedules, agents, dayOffPreferencesProvider);
				}
			}
		}
	}
}