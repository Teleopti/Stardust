using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IClassicDaysOffOptimizationCommand
	{
		void Execute(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences,
					ISchedulerStateHolder schedulerStateHolder, IBackgroundWorkerWrapper backgroundWorker, 
					IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}

	public class ClassicDaysOffOptimizationCommand : IClassicDaysOffOptimizationCommand
	{
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _bitArrayConverter;
		private readonly IWorkShiftBackToLegalStateServiceFactory _workShiftBackToLegalStateServiceFactory;
		private readonly IScheduleService _scheduleService;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly IDayOffBackToLegalStateFunctions _dayOffBackToLegalStateFunctions;
		private readonly IDayOffDecisionMaker _dayOffDecisionMaker;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IMinWeekWorkTimeRule _minWeekWorkTimeRule;
		private readonly IDayOffOptimizerValidator _dayOffOptimizerValidator;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly Func<IWorkShiftFinderResultHolder> _workShiftFinderResultHolder;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;

		public ClassicDaysOffOptimizationCommand(IOptimizerHelperHelper optimizerHelperHelper, Func<IPersonSkillProvider> personSkillProvider,
			IScheduleMatrixLockableBitArrayConverterEx bitArrayConverter,
			IWorkShiftBackToLegalStateServiceFactory workShiftBackToLegalStateServiceFactory, IScheduleService scheduleService,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
			IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions, IDayOffDecisionMaker dayOffDecisionMaker,
			IResourceOptimizationHelper resourceOptimizationHelper, IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IMinWeekWorkTimeRule minWeekWorkTimeRule, IDayOffOptimizerValidator dayOffOptimizerValidator,
			ISchedulingOptionsCreator schedulingOptionsCreator, Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended)
		{
			_optimizerHelperHelper = optimizerHelperHelper;
			_personSkillProvider = personSkillProvider;
			_bitArrayConverter = bitArrayConverter;
			_workShiftBackToLegalStateServiceFactory = workShiftBackToLegalStateServiceFactory;
			_scheduleService = scheduleService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
			_dayOffBackToLegalStateFunctions = dayOffBackToLegalStateFunctions;
			_dayOffDecisionMaker = dayOffDecisionMaker;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_minWeekWorkTimeRule = minWeekWorkTimeRule;
			_dayOffOptimizerValidator = dayOffOptimizerValidator;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
		}

		public void Execute(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences, 
							ISchedulerStateHolder schedulerStateHolder, IBackgroundWorkerWrapper backgroundWorker, 
							IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider();

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(
					schedulerStateHolder.SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag));

			ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict =
				new SchedulePartModifyAndRollbackService(
					schedulerStateHolder.SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag));

			IList<IDayOffOptimizerContainer> optimizerContainers = new List<IDayOffOptimizerContainer>();
			for (int index = 0; index < matrixOriginalStateContainerListForDayOffOptimization.Count; index++)
			{
				IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer = matrixOriginalStateContainerListForDayOffOptimization[index];
				IScheduleMatrixPro matrix = matrixOriginalStateContainerListForDayOffOptimization[index].ScheduleMatrix;

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person, matrix.EffectivePeriodDays.First().Day);

				IScheduleResultDataExtractor personalSkillsDataExtractor =
					dataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);
				IPeriodValueCalculator localPeriodValueCalculator =
					_optimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, personalSkillsDataExtractor);
				IDayOffOptimizerContainer optimizerContainer =
					createOptimizer(matrix, dayOffOptimizationPreference, optimizationPreferences,
						rollbackService, schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate, _scheduleService, localPeriodValueCalculator,
						rollbackServiceDayOffConflict, matrixOriginalStateContainer, dataExtractorProvider, () => schedulerStateHolder.SchedulingResultState);

				if(matrix.SchedulePeriod.DaysOff() > 0)
					optimizerContainers.Add(optimizerContainer);
			}

			var minutesPerInterval = 15;

			if (schedulerStateHolder.SchedulingResultState.Skills.Any())
			{
				minutesPerInterval = schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
			}

			IScheduleResultDataExtractor allSkillsDataExtractor =
				_optimizerHelperHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod, schedulerStateHolder.SchedulingResultState);
			IPeriodValueCalculator periodValueCalculator =
				_optimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, allSkillsDataExtractor);

			IDayOffOptimizationService service = new DayOffOptimizationService(periodValueCalculator);

			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules);
			_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, false);
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{				
				EventHandler<ResourceOptimizerProgressEventArgs> handler = (s, e) => backgroundWorker.ReportProgress(0, e);
				service.ReportProgress += handler;
				service.Execute(optimizerContainers);
				service.ReportProgress -= handler;
			}			
		}

		private IDayOffOptimizerContainer createOptimizer(
			IScheduleMatrixPro scheduleMatrix,
			IDaysOffPreferences daysOffPreferences,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IDayOffTemplate dayOffTemplate,
			IScheduleService scheduleService,
			IPeriodValueCalculator periodValueCalculatorForAllSkills,
			ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict,
			IScheduleMatrixOriginalStateContainer originalStateContainer,
			IScheduleResultDataExtractorProvider dataExtractorProvider, 
			Func<ISchedulingResultStateHolder> scheduleResultStateHolder)
		{
			IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
				_workShiftBackToLegalStateServiceFactory.CreateWorkShiftBackToLegalStateServicePro();

			ILockableBitArray scheduleMatrixArray = _bitArrayConverter.Convert(scheduleMatrix, daysOffPreferences.ConsiderWeekBefore, daysOffPreferences.ConsiderWeekAfter);

			// create decisionmakers
			var decisionMakers = _dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(scheduleMatrixArray, optimizerPreferences, daysOffPreferences);
			var scheduleResultDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrix, optimizerPreferences.Advanced);

			ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService =
				new SmartDayOffBackToLegalStateService(
					_dayOffBackToLegalStateFunctions,
					25,
					_dayOffDecisionMaker);

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true);

			var dayOffOptimizerConflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrix, scheduleService,
				_effectiveRestrictionCreator,
				rollbackServiceDayOffConflict,
				resourceCalculateDelayer);

			var restrictionChecker = new RestrictionChecker();
			var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateContainer, daysOffPreferences);
			var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider, _minWeekWorkTimeRule);

			IDeleteAndResourceCalculateService deleteAndResourceCalculateService =
				new DeleteAndResourceCalculateService(new DeleteSchedulePartService(scheduleResultStateHolder), _resourceOptimizationHelper);
			INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
				new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
					deleteAndResourceCalculateService,
					scheduleService, _workShiftFinderResultHolder,
					resourceCalculateDelayer);
			var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
			var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(scheduleResultStateHolder);
			var deviationStatisticData = new DeviationStatisticData();
			var dayOffOptimizerPreMoveResultPredictor =
				new DayOffOptimizerPreMoveResultPredictor(dailySkillForecastAndScheduledValueCalculator, deviationStatisticData);

			IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter
				= new DayOffDecisionMakerExecuter(rollbackService,
					dayOffBackToLegalStateService,
					dayOffTemplate,
					scheduleService,
					optimizerPreferences,
					periodValueCalculatorForAllSkills,
					workShiftBackToLegalStateService,
					_effectiveRestrictionCreator,
					_resourceOptimizationHelper,
					new ResourceCalculateDaysDecider(),
					_dayOffOptimizerValidator,
					dayOffOptimizerConflictHandler,
					originalStateContainer,
					optimizationLimits,
					nightRestWhiteSpotSolverService,
					_schedulingOptionsCreator,
					mainShiftOptimizeActivitySpecificationSetter,
					dayOffOptimizerPreMoveResultPredictor,
					daysOffPreferences);

			IDayOffOptimizerContainer optimizerContainer =
				new DayOffOptimizerContainer(_bitArrayConverter,
					decisionMakers,
					scheduleResultDataExtractor,
					daysOffPreferences,
					scheduleMatrix,
					dayOffDecisionMakerExecuter,
					originalStateContainer);
			return optimizerContainer;
		}
	}
}