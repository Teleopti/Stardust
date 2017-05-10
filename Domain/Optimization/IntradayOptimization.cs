using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimization
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IntradayOptimizerCreator _intradayOptimizer2Creator;
		private readonly IntradayOptimizationContext _intradayOptimizationContext;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly IResourceCalculation _resourceOptimization;

		public IntradayOptimization(Func<ISchedulerStateHolder> schedulerStateHolder,
			IntradayOptimizerCreator intradayOptimizer2Creator,
			IntradayOptimizationContext intradayOptimizationContext,
			IIntradayOptimizerContainer intradayOptimizerContainer,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			IResourceCalculation resourceOptimization)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_intradayOptimizationContext = intradayOptimizationContext;
			_intradayOptimizerContainer = intradayOptimizerContainer;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_resourceOptimization = resourceOptimization;
		}

		public void Execute(DateOnlyPeriod period, 
												IPerson[] agents,
												bool runResolveWeeklyRestRule)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var schedules = schedulerStateHolder.Schedules.SchedulesForPeriod(period, agents);
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			var dayOffPreferencesProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()); //doesn't seem to be used with "real" values when doing intraday optimization
			var optimizers = _intradayOptimizer2Creator.Create(period, schedules, optimizationPreferences, dayOffPreferencesProvider);

			using (_intradayOptimizationContext.Create(period))
			{
				_resourceOptimization.ResourceCalculate(period.Extend(1), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, schedulerStateHolder.ConsiderShortBreaks, false));
				_intradayOptimizerContainer.Execute(optimizers);

				if (runResolveWeeklyRestRule)
				{
					_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, schedules, agents, dayOffPreferencesProvider);
				}
			}
		}
	}
}