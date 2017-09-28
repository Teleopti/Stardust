using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public interface IIntradayOptimization
	{
		void Execute(DateOnlyPeriod period, IEnumerable<IPerson> agents, bool runResolveWeeklyRestRule, IBlockPreferenceProvider blockPreferenceProvider);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class IntradayOptimizationClassic : IIntradayOptimization
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IntradayOptimizerCreator _intradayOptimizerCreator;
		private readonly IntradayOptimizationContext _intradayOptimizationContext;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly IResourceCalculation _resourceOptimization;

		public IntradayOptimizationClassic(Func<ISchedulerStateHolder> schedulerStateHolder,
			IntradayOptimizerCreator intradayOptimizerCreator,
			IntradayOptimizationContext intradayOptimizationContext,
			IIntradayOptimizerContainer intradayOptimizerContainer,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			IResourceCalculation resourceOptimization)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_intradayOptimizerCreator = intradayOptimizerCreator;
			_intradayOptimizationContext = intradayOptimizationContext;
			_intradayOptimizerContainer = intradayOptimizerContainer;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_resourceOptimization = resourceOptimization;
		}

		public void Execute(DateOnlyPeriod period, IEnumerable<IPerson> agents, bool runResolveWeeklyRestRule, IBlockPreferenceProvider blockPreferenceProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			var dayOffPreferencesProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()); //doesn't seem to be used with "real" values when doing intraday optimization
			var optimizers = _intradayOptimizerCreator.Create(period, agents, optimizationPreferences, dayOffPreferencesProvider);

			using (_intradayOptimizationContext.Create(period))
			{
				_resourceOptimization.ResourceCalculate(period.Extend(1), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, schedulerStateHolder.ConsiderShortBreaks, false));
				_intradayOptimizerContainer.Execute(optimizers);

				if (runResolveWeeklyRestRule)
				{
					_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, agents, dayOffPreferencesProvider);
				}
			}
		}
	}
}