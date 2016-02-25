using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimization
	{
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly WebSchedulingSetup _webSchedulingSetup;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IIntradayOptimizer2Creator _intradayOptimizer2Creator;
		private readonly VirtualSkillContext _virtualSkillContext;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;

		public IntradayOptimization(OptimizationPreferencesFactory optimizationPreferencesFactory,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
									WebSchedulingSetup webSchedulingSetup,
									IMatrixListFactory matrixListFactory,
									IScheduleDayEquator scheduleDayEquator,
									IPlanningPeriodRepository planningPeriodRepository,
									Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
									IScheduleDictionaryPersister persister,
									IOptimizerHelperHelper optimizerHelperHelper,
									WeeklyRestSolverExecuter weeklyRestSolverExecuter,
									IIntradayOptimizer2Creator intradayOptimizer2Creator,
									VirtualSkillContext virtualSkillContext,
									IIntradayOptimizerContainer intradayOptimizerContainer,
									ResourceCalculationContextFactory resourceCalculationContextFactory
									)
		{
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_webSchedulingSetup = webSchedulingSetup;
			_matrixListFactory = matrixListFactory;
			_scheduleDayEquator = scheduleDayEquator;
			_planningPeriodRepository = planningPeriodRepository;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_persister = persister;
			_optimizerHelperHelper = optimizerHelperHelper;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_virtualSkillContext = virtualSkillContext;
			_intradayOptimizerContainer = intradayOptimizerContainer;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public virtual OptimizationResultModel Optimize(Guid planningPeriodId)
		{
			var period = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return CreateResult(period);
		}

		[UnitOfWork]
		[LogTime]
		protected virtual DateOnlyPeriod SetupAndOptimize(Guid planningPeriodId)
		{
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffOptimizationPreference = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var webScheduleState = _webSchedulingSetup.Setup(period);

			var matrixListForIntraDayOptimizationOriginal = _matrixListFactory.CreateMatrixListForSelection(webScheduleState.AllSchedules);
			var matrixOriginalStateContainerListForIntradayOptimizationOriginal = matrixListForIntraDayOptimizationOriginal.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator)).ToList();
			var optimizers = _intradayOptimizer2Creator.Create(matrixOriginalStateContainerListForIntradayOptimizationOriginal, matrixOriginalStateContainerListForIntradayOptimizationOriginal.ToList(), optimizationPreferences, dayOffOptimizationPreference);
			_optimizerHelperHelper.LockDaysForIntradayOptimization(matrixListForIntraDayOptimizationOriginal, period);

			using (_virtualSkillContext.Create(period))
			{
				using (_resourceCalculationContextFactory.Create())
				{
					_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);
					_intradayOptimizerContainer.Execute(optimizers);
					_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, webScheduleState.AllSchedules, webScheduleState.PeopleSelection.AllPeople, dayOffOptimizationPreference);
				}
			}

			return period;
		}

		[LogTime]
		protected virtual OptimizationResultModel CreateResult(DateOnlyPeriod period)
		{
			var result = new OptimizationResultModel();
			result.Map(_schedulerStateHolder().SchedulingResultState.SkillDays, period);
			return result;
		}
	}
}