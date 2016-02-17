using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class ExtendReduceDaysOffHelper : IExtendReduceDaysOffHelper
	{
		private readonly ILifetimeScope _container;
		private readonly OptimizerHelperHelper _optimizerHelper;
		private readonly Func<IWorkShiftFinderResultHolder> _allResults;

		public ExtendReduceDaysOffHelper(ILifetimeScope container, 
				OptimizerHelperHelper optimizerHelper, 
				Func<IWorkShiftFinderResultHolder> allResults)
		{
			_container = container;
			_optimizerHelper = optimizerHelper;
			_allResults = allResults;
		}

		public void RunExtendReduceDayOffOptimization(IOptimizationPreferences optimizerPreferences,
			ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedDays,
			ISchedulerStateHolder schedulerStateHolder,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (backgroundWorker.CancellationPending)
				return;

			IList<IScheduleMatrixPro> matrixList = _container.Resolve<IMatrixListFactory>().CreateMatrixListForSelection(selectedDays);
			lockDaysForExtendReduceOptimization(matrixList, selectedPeriod, optimizerPreferences.Shifts.SelectedActivities);

			IList<IScheduleMatrixOriginalStateContainer> originalStateListForScheduleTag = createMatrixContainerList(matrixList);

			var dataExtractorProvider = _container.Resolve<IScheduleResultDataExtractorProvider>();

			IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, schedulerStateHolder.SchedulingResultState, optimizerPreferences.Advanced);

			IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
			IPeriodValueCalculator allSkillsPeriodValueCalculator =
				periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);
			IExtendReduceDaysOffOptimizerService extendReduceDaysOffOptimizerService = new ExtendReduceDaysOffOptimizerService(allSkillsPeriodValueCalculator);

			var possibleMinMaxWorkShiftLengthExtractor =
				_container.Resolve<IPossibleMinMaxWorkShiftLengthExtractor>();
			var schedulePeriodTargetTimeCalculator =
				_container.Resolve<ISchedulePeriodTargetTimeCalculator>();
			var workShiftWeekMinMaxCalculator =
				_container.Resolve<IWorkShiftWeekMinMaxCalculator>();
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculatorForFlexibleAgents = new WorkShiftMinMaxCalculator(possibleMinMaxWorkShiftLengthExtractor, schedulePeriodTargetTimeCalculator, workShiftWeekMinMaxCalculator);
			var workShiftFinderServiceForFlexibleAgents =
				_container.Resolve<IWorkShiftFinderService>(new TypedParameter(typeof(IWorkShiftMinMaxCalculator), workShiftMinMaxCalculatorForFlexibleAgents));
			var scheduleServiceForFlexibleAgents =
				_container.Resolve<IScheduleService>(new TypedParameter(typeof(IWorkShiftFinderService),
					workShiftFinderServiceForFlexibleAgents));
			var deleteAndResourceCalculateService = _container.Resolve<IDeleteAndResourceCalculateService>();

			var schedulePartModifyAndRollbackService =
				_container.Resolve<ISchedulePartModifyAndRollbackService>();
			var effectiveRestrictionCreator =
				_container.Resolve<IEffectiveRestrictionCreator>();
			var resourceCalculateDaysDecider =
				_container.Resolve<IResourceCalculateDaysDecider>();
			var scheduleMatrixLockableBitArrayConverterEx =
				_container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

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
				IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro, optimizerPreferences.Advanced);
				IPeriodValueCalculator personalSkillsPeriodValueCalculator =
					periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);


				var dayOffOptimizePreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				IExtendReduceDaysOffDecisionMaker decisionMaker = new ExtendReduceDaysOffDecisionMaker(scheduleMatrixLockableBitArrayConverterEx);
				ILockableBitArray bitArray = scheduleMatrixLockableBitArrayConverterEx.Convert(scheduleMatrixPro, dayOffOptimizePreference.ConsiderWeekBefore, dayOffOptimizePreference.ConsiderWeekAfter);
				var daysOffLegalStateValidatorsFactory = _container.Resolve<IDaysOffLegalStateValidatorsFactory>();
				var validators = daysOffLegalStateValidatorsFactory.CreateLegalStateValidators(bitArray,
					optimizerPreferences, dayOffOptimizePreference);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_container.Resolve<IResourceOptimizationHelper>(), 1, true);

				INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
					new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
						deleteAndResourceCalculateService,
						scheduleServiceForFlexibleAgents, _allResults, resourceCalculateDelayer);

				IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
					_optimizerHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

				IDayOffsInPeriodCalculator dayOffsInPeriodCalculator = new DayOffsInPeriodCalculator(()=>schedulerStateHolder.SchedulingResultState);

				IList<IDayOffTemplate> displayList = schedulerStateHolder.CommonStateHolder.ActiveDayOffs.ToList();

				IDayOffOptimizerConflictHandler conflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrixPro,
					scheduleServiceForFlexibleAgents,
					effectiveRestrictionCreator,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer);

				IWorkTimeStartEndExtractor workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
				INewDayOffRule dayOffRule = new NewDayOffRule(workTimeStartEndExtractor);
				IDayOffOptimizerValidator dayOffOptimizerValidator = new DayOffOptimizerValidator(dayOffRule);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				ICheckerRestriction checkerRestriction = new RestrictionChecker();

				var dayOffOptimizationPreferences = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(checkerRestriction, optimizerPreferences, originalStateListForScheduleTag[i], dayOffOptimizationPreferences);

				IOptimizationLimits optimizationLimits = new OptimizationLimits(optimizationOverLimitDecider, _container.Resolve<IMinWeekWorkTimeRule>());

				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

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
					mainShiftOptimizeActivitySpecificationSetter,
					scheduleMatrixPro);

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

		private static void lockDaysForExtendReduceOptimization(IList<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod, IList<IActivity> keepActivities )
		{
			IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
			noMainShiftLocker.Execute();
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();

			var matrixKeepActivityLocker = new MatrixKeepActivityLocker(matrixList, keepActivities);
			matrixKeepActivityLocker.Execute();

		}

		private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
		{
			IScheduleDayEquator scheduleDayEquator = _container.Resolve<IScheduleDayEquator>();
			IList<IScheduleMatrixOriginalStateContainer> result =
				matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();
			return result;
		}
	}
}