using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimization
	{
		private readonly IDailyValueByAllSkillsExtractor _dailyValueByAllSkillsExtractor;
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly WebSchedulingSetup _webSchedulingSetup;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IIntradayOptimizer2Creator _intradayOptimizer2Creator;

		public IntradayOptimization(IDailyValueByAllSkillsExtractor dailyValueByAllSkillsExtractor, 
									OptimizationPreferencesFactory optimizationPreferencesFactory,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
									DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
									WebSchedulingSetup webSchedulingSetup,
									Func<IPersonSkillProvider> personSkillProvider,
									IMatrixListFactory matrixListFactory,
									IScheduleDayEquator scheduleDayEquator,
									IPlanningPeriodRepository planningPeriodRepository,
									Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
									IScheduleDictionaryPersister persister,
									IOptimizerHelperHelper optimizerHelperHelper,
									WeeklyRestSolverExecuter weeklyRestSolverExecuter,
									IIntradayOptimizer2Creator intradayOptimizer2Creator
									)
		{
			_dailyValueByAllSkillsExtractor = dailyValueByAllSkillsExtractor;
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_webSchedulingSetup = webSchedulingSetup;
			_personSkillProvider = personSkillProvider;
			_matrixListFactory = matrixListFactory;
			_scheduleDayEquator = scheduleDayEquator;
			_planningPeriodRepository = planningPeriodRepository;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_persister = persister;
			_optimizerHelperHelper = optimizerHelperHelper;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
		}

		public virtual OptimizationResultModel Optimize(Guid planningPeriodId)
		{
			var planningPeriod = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return CreateResult(planningPeriod);
		}

		[UnitOfWork]
		[LogTime]
		protected virtual IPlanningPeriod SetupAndOptimize(Guid planningPeriodId)
		{
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffOptimizationPreference = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var webScheduleState = _webSchedulingSetup.Setup(period);

			var matrixListForIntraDayOptimizationOriginal = _matrixListFactory.CreateMatrixListForSelection(webScheduleState.AllSchedules);
			var matrixOriginalStateContainerListForIntradayOptimizationOriginal =
				matrixListForIntraDayOptimizationOriginal.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator));

			var matrixOriginalStateContainerListForIntradayOptimizationWork =
				_matrixListFactory.CreateMatrixListForSelection(webScheduleState.AllSchedules).Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator));

			var rollbackService = new SchedulePartModifyAndRollbackService(
					_schedulerStateHolder().SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag));

			var optimizers = _intradayOptimizer2Creator.Create(matrixOriginalStateContainerListForIntradayOptimizationOriginal,
				matrixOriginalStateContainerListForIntradayOptimizationWork, optimizationPreferences, rollbackService, dayOffOptimizationPreference);
			var service = new IntradayOptimizerContainer(_dailyValueByAllSkillsExtractor);
			var minutesPerInterval = 15;

			if (_schedulerStateHolder().SchedulingResultState.Skills.Any())
			{
				minutesPerInterval = _schedulerStateHolder().SchedulingResultState.Skills.Min(s => s.DefaultResolution);
			}

			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(_schedulerStateHolder().Schedules);

			_optimizerHelperHelper.LockDaysForIntradayOptimization(matrixListForIntraDayOptimizationOriginal, period);

			_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoBackgroundWorker());
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				service.Execute(optimizers, period, optimizationPreferences.Advanced.TargetValueCalculation);
			}

			_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, webScheduleState.AllSchedules, webScheduleState.PeopleSelection.AllPeople, dayOffOptimizationPreference);
			return planningPeriod;
		}

		[LogTime]
		protected virtual OptimizationResultModel CreateResult(IPlanningPeriod planningPeriod)
		{
			var result = new OptimizationResultModel();
			result.Map(_schedulerStateHolder().SchedulingResultState.SkillDays, planningPeriod.Range);
			return result;
		}
	}
}