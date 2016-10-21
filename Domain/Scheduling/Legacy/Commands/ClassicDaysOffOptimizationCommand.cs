﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ClassicDaysOffOptimizationCommand
	{
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _bitArrayConverter;
		private readonly IWorkShiftBackToLegalStateServiceFactory _workShiftBackToLegalStateServiceFactory;
		private readonly IScheduleService _scheduleService;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly IDayOffDecisionMaker _dayOffDecisionMaker;
		private readonly IResourceOptimization _resourceOptimizationHelper;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IDayOffOptimizerValidator _dayOffOptimizerValidator;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly Func<IWorkShiftFinderResultHolder> _workShiftFinderResultHolder;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly PersonalSkillsProvider _personalSkillsProvider;
		private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;

		public ClassicDaysOffOptimizationCommand(IOptimizerHelperHelper optimizerHelperHelper, 
			IScheduleMatrixLockableBitArrayConverterEx bitArrayConverter,
			IWorkShiftBackToLegalStateServiceFactory workShiftBackToLegalStateServiceFactory, 
			IScheduleService scheduleService,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
			IDayOffDecisionMaker dayOffDecisionMaker,
			IResourceOptimization resourceOptimizationHelper, 
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IDayOffOptimizerValidator dayOffOptimizerValidator,
			ISchedulingOptionsCreator schedulingOptionsCreator, 
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			PersonalSkillsProvider personalSkillsProvider,
			IResourceCalculateDaysDecider resourceCalculateDaysDecider,
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider)
		{
			_optimizerHelperHelper = optimizerHelperHelper;
			_bitArrayConverter = bitArrayConverter;
			_workShiftBackToLegalStateServiceFactory = workShiftBackToLegalStateServiceFactory;
			_scheduleService = scheduleService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
			_dayOffDecisionMaker = dayOffDecisionMaker;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_dayOffOptimizerValidator = dayOffOptimizerValidator;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_schedulerStateHolder = schedulerStateHolder;
			_personalSkillsProvider = personalSkillsProvider;
			_resourceCalculateDaysDecider = resourceCalculateDaysDecider;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
		}

		public void Execute(
			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization,
			DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences,
			ISchedulingProgress backgroundWorker,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			IScheduleResultDataExtractorProvider dataExtractorProvider = _scheduleResultDataExtractorProvider;

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
				IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer =
					matrixOriginalStateContainerListForDayOffOptimization[index];
				IScheduleMatrixPro matrix = matrixOriginalStateContainerListForDayOffOptimization[index].ScheduleMatrix;

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,
					matrix.EffectivePeriodDays.First().Day);

				IScheduleResultDataExtractor personalSkillsDataExtractor =
					dataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced, schedulerStateHolder.SchedulingResultState);
				IPeriodValueCalculator localPeriodValueCalculator =
					_optimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, personalSkillsDataExtractor);
				IDayOffOptimizerContainer optimizerContainer =
					createOptimizer(matrix, dayOffOptimizationPreference, optimizationPreferences,
						rollbackService, schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate, _scheduleService,
						localPeriodValueCalculator,
						rollbackServiceDayOffConflict, matrixOriginalStateContainer, dataExtractorProvider,
						() => schedulerStateHolder.SchedulingResultState);

				if (matrix.SchedulePeriod.DaysOff() > 0)
					optimizerContainers.Add(optimizerContainer);
			}

			IScheduleResultDataExtractor allSkillsDataExtractor =
				_optimizerHelperHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod,
					schedulerStateHolder.SchedulingResultState);
			IPeriodValueCalculator periodValueCalculator =
				_optimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, allSkillsDataExtractor);

			IDayOffOptimizationService service = new DayOffOptimizationService(periodValueCalculator);

			EventHandler<ResourceOptimizerProgressEventArgs> handler = (s, e) => backgroundWorker.ReportProgress(0, e);
			service.ReportProgress += handler;
			service.Execute(optimizerContainers);
			service.ReportProgress -= handler;
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
			var scheduleResultDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrix, optimizerPreferences.Advanced, scheduleResultStateHolder());

			ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService =
				new SmartDayOffBackToLegalStateService(
					25,
					_dayOffDecisionMaker);

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, scheduleResultStateHolder());

			var dayOffOptimizerConflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrix, scheduleService,
				_effectiveRestrictionCreator,
				rollbackServiceDayOffConflict,
				resourceCalculateDelayer);

			var restrictionChecker = new RestrictionChecker();
			var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateContainer, daysOffPreferences);
			var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider);

			INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
				new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
					_deleteAndResourceCalculateService,
					scheduleService, _workShiftFinderResultHolder,
					resourceCalculateDelayer);
			var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
			var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(scheduleResultStateHolder);
			var deviationStatisticData = new DeviationStatisticData();
			var dayOffOptimizerPreMoveResultPredictor =
				new DayOffOptimizerPreMoveResultPredictor(dailySkillForecastAndScheduledValueCalculator, deviationStatisticData, _personalSkillsProvider);

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
					_resourceCalculateDaysDecider,
					_dayOffOptimizerValidator,
					dayOffOptimizerConflictHandler,
					originalStateContainer,
					optimizationLimits,
					nightRestWhiteSpotSolverService,
					_schedulingOptionsCreator,
					mainShiftOptimizeActivitySpecificationSetter,
					dayOffOptimizerPreMoveResultPredictor,
					daysOffPreferences,
					_schedulerStateHolder().SchedulingResultState);

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