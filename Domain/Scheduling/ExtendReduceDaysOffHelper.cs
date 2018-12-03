using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ExtendReduceDaysOffHelper 
	{
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly IScheduleService _scheduleService;
		private readonly DeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ScheduleChangesAffectedDates _scheduleChangesAffectedDates;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _scheduleMatrixLockableBitArrayConverterEx;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IDaysOffLegalStateValidatorsFactory _daysOffLegalStateValidatorsFactory;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

		public ExtendReduceDaysOffHelper(MatrixListFactory matrixListFactory,
										IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
										IScheduleService scheduleService,
										DeleteAndResourceCalculateService deleteAndResourceCalculateService,
										ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										IEffectiveRestrictionCreator effectiveRestrictionCreator,
										ScheduleChangesAffectedDates scheduleChangesAffectedDates,
										IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
										IResourceCalculation resourceCalculation,
										IUserTimeZone userTimeZone,
										IDaysOffLegalStateValidatorsFactory daysOffLegalStateValidatorsFactory,
										WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
										IScheduleDayEquator scheduleDayEquator,
										IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter)
		{
			_matrixListFactory = matrixListFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_scheduleService = scheduleService;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleChangesAffectedDates = scheduleChangesAffectedDates;
			_scheduleMatrixLockableBitArrayConverterEx = scheduleMatrixLockableBitArrayConverterEx;
			_resourceCalculation = resourceCalculation;
			_userTimeZone = userTimeZone;
			_daysOffLegalStateValidatorsFactory = daysOffLegalStateValidatorsFactory;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_scheduleDayEquator = scheduleDayEquator;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
		}

		public void RunExtendReduceDayOffOptimization(IOptimizationPreferences optimizerPreferences,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents,
			ISchedulerStateHolder schedulerStateHolder,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (backgroundWorker.CancellationPending)
				return;

			var matrixList = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedAgents, selectedPeriod).ToList();
			lockDaysForExtendReduceOptimization(matrixList, selectedPeriod, optimizerPreferences.Shifts.SelectedActivities);

			IList<IScheduleMatrixOriginalStateContainer> originalStateListForScheduleTag = createMatrixContainerList(matrixList);

			var dataExtractorProvider = _scheduleResultDataExtractorProvider;

			IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, schedulerStateHolder.SchedulingResultState, optimizerPreferences.Advanced);

			IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
			IPeriodValueCalculator allSkillsPeriodValueCalculator =
				periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);
			IExtendReduceDaysOffOptimizerService extendReduceDaysOffOptimizerService = new ExtendReduceDaysOffOptimizerService(allSkillsPeriodValueCalculator);

			var scheduleServiceForFlexibleAgents = _scheduleService;
			var deleteAndResourceCalculateService = _deleteAndResourceCalculateService;

			var schedulePartModifyAndRollbackService = _schedulePartModifyAndRollbackService;
			var effectiveRestrictionCreator = _effectiveRestrictionCreator;
			var resourceCalculateDaysDecider = _scheduleChangesAffectedDates;
			var scheduleMatrixLockableBitArrayConverterEx = _scheduleMatrixLockableBitArrayConverterEx;

			IList<IExtendReduceDaysOffOptimizer> optimizers = new List<IExtendReduceDaysOffOptimizer>();
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
				IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro, optimizerPreferences.Advanced, schedulerStateHolder.SchedulingResultState);
				IPeriodValueCalculator personalSkillsPeriodValueCalculator =
					periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);


				var dayOffOptimizePreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				var decisionMaker = new ExtendReduceDaysOffDecisionMaker(scheduleMatrixLockableBitArrayConverterEx);
				ILockableBitArray bitArray = scheduleMatrixLockableBitArrayConverterEx.Convert(scheduleMatrixPro, dayOffOptimizePreference.ConsiderWeekBefore, dayOffOptimizePreference.ConsiderWeekAfter);
				var daysOffLegalStateValidatorsFactory = _daysOffLegalStateValidatorsFactory;
				var validators = daysOffLegalStateValidatorsFactory.CreateLegalStateValidators(bitArray,
					optimizerPreferences, dayOffOptimizePreference);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, true, schedulerStateHolder.SchedulingResultState, _userTimeZone);

				INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
					new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(_effectiveRestrictionCreator),
						deleteAndResourceCalculateService,
						scheduleServiceForFlexibleAgents, resourceCalculateDelayer);

				IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService = _workShiftBackToLegalStateServiceProFactory.Create();

				IDayOffsInPeriodCalculator dayOffsInPeriodCalculator = new DayOffsInPeriodCalculator();

				IList<IDayOffTemplate> displayList = schedulerStateHolder.CommonStateHolder.DayOffs.NonDeleted().ToList();

				IDayOffOptimizerConflictHandler conflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrixPro,
					scheduleServiceForFlexibleAgents,
					effectiveRestrictionCreator,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer);

				IWorkTimeStartEndExtractor workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
				INewDayOffRule dayOffRule = new NewDayOffRule(workTimeStartEndExtractor);
				var dayOffOptimizerValidator = new DayOffOptimizerValidator(dayOffRule);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				ICheckerRestriction checkerRestriction = new RestrictionChecker();

				var dayOffOptimizationPreferences = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(checkerRestriction, optimizerPreferences, originalStateListForScheduleTag[i], dayOffOptimizationPreferences);

				var optimizationLimits = new OptimizationLimits(optimizationOverLimitDecider);

				IExtendReduceDaysOffOptimizer optimizer = new ExtendReduceDaysOffOptimizer(
					personalSkillsPeriodValueCalculator,
					personalSkillsDataExtractor,
					decisionMaker,
					scheduleServiceForFlexibleAgents,
					optimizerPreferences,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer,
					effectiveRestrictionCreator,
					resourceCalculateDaysDecider,
					originalStateListForScheduleTag[i],
					workShiftBackToLegalStateService,
					nightRestWhiteSpotSolverService,
					validators,
					dayOffsInPeriodCalculator,
					displayList[0],
					conflictHandler,
					dayOffOptimizerValidator,
					optimizationLimits,
					schedulingOptionsCreator,
					_mainShiftOptimizeActivitySpecificationSetter,
					scheduleMatrixPro,
					schedulerStateHolder.Schedules);

				optimizers.Add(optimizer);
			}

			EventHandler<ResourceOptimizerProgressEventArgs> optimizerServiceOnReportProgress = (sender, e) =>
			{
				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
					e.CancelAction();
				}
				backgroundWorker.ReportProgress(1, e);
			};
			extendReduceDaysOffOptimizerService.ReportProgress += optimizerServiceOnReportProgress;
			extendReduceDaysOffOptimizerService.Execute(optimizers);
			extendReduceDaysOffOptimizerService.ReportProgress -= optimizerServiceOnReportProgress;
		}

		private static void lockDaysForExtendReduceOptimization(IEnumerable<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod, IList<IActivity> keepActivities )
		{
			var matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			var noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
			noMainShiftLocker.Execute();
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();

			var matrixKeepActivityLocker = new MatrixKeepActivityLocker(matrixList, keepActivities);
			matrixKeepActivityLocker.Execute();

		}

		private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
		{
			IScheduleDayEquator scheduleDayEquator = _scheduleDayEquator;
			IList<IScheduleMatrixOriginalStateContainer> result =
				matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();
			return result;
		}
	}
}