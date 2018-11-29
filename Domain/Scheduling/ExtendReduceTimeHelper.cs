using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ExtendReduceTimeHelper
	{
		private readonly MatrixListFactory _matrixListFactory;
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
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private ISchedulingProgress _backgroundWorker;

		public ExtendReduceTimeHelper(MatrixListFactory matrixListFactory, 
										IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
										IScheduleService scheduleService,
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										IDeleteSchedulePartService deleteSchedulePartService,
										IResourceCalculation resourceCalculation,
										IEffectiveRestrictionCreator effectiveRestrictionCreator,
										ScheduleChangesAffectedDates scheduleChangesAffectedDates,
										IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
										IUserTimeZone userTimeZone,
										IScheduleDayEquator scheduleDayEquator,
										IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter)
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
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
		}

		public void RunExtendReduceTimeOptimization(IOptimizationPreferences optimizerPreferences,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (backgroundWorker.CancellationPending)
				return;

			_backgroundWorker = backgroundWorker;

			var matrixList = _matrixListFactory.CreateMatrixListForSelection(schedulingResultStateHolder.Schedules, selectedAgents, selectedPeriod).ToList();
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
				//kolla om rätt employment type annars hoppa till nästa
				if (contract.EmploymentType == EmploymentType.HourlyStaff)
					continue;
				if (contract.PositivePeriodWorkTimeTolerance == TimeSpan.Zero && contract.NegativePeriodWorkTimeTolerance == TimeSpan.Zero)
					continue;
				IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro, optimizerPreferences.Advanced, schedulingResultStateHolder);
				IPeriodValueCalculator personalSkillsPeriodValueCalculator =
					periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);

				var decisionMaker = new ExtendReduceTimeDecisionMaker(scheduleMatrixLockableBitArrayConverterEx);
				
				var restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateListForScheduleTag[i], dayOffOptimizationPreference);

				var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider);

				var schedulingOptionsCreator = new SchedulingOptionsCreator();
				var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(resourceOptimizationHelper, schedulingOptions.ConsiderShortBreaks, schedulingResultStateHolder, _userTimeZone);

				var optimizer = new ExtendReduceTimeOptimizer(
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
					_mainShiftOptimizeActivitySpecificationSetter,
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
			var matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			var noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
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