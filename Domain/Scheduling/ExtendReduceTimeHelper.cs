using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ExtendReduceTimeHelper
	{
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly IScheduleService _scheduleService;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ScheduleChangesAffectedDates _scheduleChangesAffectedDates;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _scheduleMatrixLockableBitArrayConverterEx;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private ISchedulingProgress _backgroundWorker;

		public ExtendReduceTimeHelper(IMatrixListFactory matrixListFactory, 
										IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
										IScheduleService scheduleService,
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										IDeleteSchedulePartService deleteSchedulePartService,
										IResourceCalculation resourceCalculation,
										IEffectiveRestrictionCreator effectiveRestrictionCreator,
										ScheduleChangesAffectedDates scheduleChangesAffectedDates,
										IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
										IUserTimeZone userTimeZone,
										IScheduleDayEquator scheduleDayEquator)
		{
			_matrixListFactory = matrixListFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_scheduleService = scheduleService;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceCalculation = resourceCalculation;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleChangesAffectedDates = scheduleChangesAffectedDates;
			_scheduleMatrixLockableBitArrayConverterEx = scheduleMatrixLockableBitArrayConverterEx;
			_userTimeZone = userTimeZone;
			_scheduleDayEquator = scheduleDayEquator;
		}

		public void RunExtendReduceTimeOptimization(IOptimizationPreferences optimizerPreferences,
			ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedDays,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (backgroundWorker.CancellationPending)
				return;

			_backgroundWorker = backgroundWorker;

			IList<IScheduleMatrixPro> matrixList = _matrixListFactory.CreateMatrixListForSelection(schedulingResultStateHolder.Schedules, selectedDays);
			lockDaysForExtendReduceOptimization(matrixList, selectedPeriod);

			IList<IScheduleMatrixOriginalStateContainer> originalStateListForScheduleTag = createMatrixContainerList(matrixList);

			IScheduleResultDataExtractorProvider dataExtractorProvider = _scheduleResultDataExtractorProvider;

			IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, schedulingResultStateHolder, optimizerPreferences.Advanced);

			IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
			IPeriodValueCalculator allSkillsPeriodValueCalculator =
				periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

			IExtendReduceTimeOptimizerService extendReduceTimeOptimizerService = new ExtendReduceTimeOptimizerService(allSkillsPeriodValueCalculator);
			IScheduleService scheduleServiceForFlexibleAgents = _scheduleService;
				
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService = _schedulePartModifyAndRollbackService;
			IDeleteSchedulePartService deleteSchedulePartService = _deleteSchedulePartService;
			IResourceCalculation resourceOptimizationHelper = _resourceCalculation;
			IEffectiveRestrictionCreator effectiveRestrictionCreator = _effectiveRestrictionCreator;
			var resourceCalculateDaysDecider = _scheduleChangesAffectedDates;
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx = _scheduleMatrixLockableBitArrayConverterEx;

			IList<IExtendReduceTimeOptimizer> optimizers = new List<IExtendReduceTimeOptimizer>();
			for (int i = 0; i < matrixList.Count; i++)
			{
				IScheduleMatrixPro scheduleMatrixPro = matrixList[i];
				IVirtualSchedulePeriod virtualSchedulePeriod =
					scheduleMatrixPro.Person.VirtualSchedulePeriod(scheduleMatrixPro.EffectivePeriodDays[0].Day);
				if (!virtualSchedulePeriod.IsValid)
					continue;
				IContract contract = virtualSchedulePeriod.Contract;
				//kolla om r�tt employment type annars hoppa till n�sta
				if (contract.EmploymentType == EmploymentType.HourlyStaff)
					continue;
				if (contract.PositivePeriodWorkTimeTolerance == TimeSpan.Zero && contract.NegativePeriodWorkTimeTolerance == TimeSpan.Zero)
					continue;
				IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro, optimizerPreferences.Advanced, schedulingResultStateHolder);
				IPeriodValueCalculator personalSkillsPeriodValueCalculator =
					periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);

				IExtendReduceTimeDecisionMaker decisionMaker = new ExtendReduceTimeDecisionMaker(scheduleMatrixLockableBitArrayConverterEx);
				
				ICheckerRestriction restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				IOptimizationOverLimitByRestrictionDecider optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateListForScheduleTag[i], dayOffOptimizationPreference);

				IOptimizationLimits optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				ISchedulingOptions schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter(new OptimizerActivitiesPreferencesFactory());
				var resourceCalculateDelayer = new ResourceCalculateDelayer(resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, schedulingResultStateHolder, _userTimeZone);

				IExtendReduceTimeOptimizer optimizer = new ExtendReduceTimeOptimizer(
					personalSkillsPeriodValueCalculator,
					personalSkillsDataExtractor,
					decisionMaker,
					scheduleServiceForFlexibleAgents,
					optimizerPreferences,
					schedulePartModifyAndRollbackService,
					deleteSchedulePartService,
					resourceCalculateDelayer,
					effectiveRestrictionCreator,
					resourceCalculateDaysDecider,
					originalStateListForScheduleTag[i],
					optimizationLimits,
					schedulingOptions,
					mainShiftOptimizeActivitySpecificationSetter,
					scheduleMatrixPro);

				optimizers.Add(optimizer);
			}

			extendReduceTimeOptimizerService.ReportProgress += extendReduceTimeOptimizerService_ReportProgress;
			extendReduceTimeOptimizerService.Execute(optimizers);
			extendReduceTimeOptimizerService.ReportProgress -= extendReduceTimeOptimizerService_ReportProgress;
		}

		void extendReduceTimeOptimizerService_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
				e.CancelAction();
			}
			_backgroundWorker.ReportProgress(1, e);
		}

		private static void lockDaysForExtendReduceOptimization(IList<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod)
		{
			IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
			noMainShiftLocker.Execute();
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();

		}

		private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IList<IScheduleMatrixPro> matrixList)
		{
			IScheduleDayEquator scheduleDayEquator = _scheduleDayEquator;
			IList<IScheduleMatrixOriginalStateContainer> result =
				matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();
			return result;
		}
	}
}