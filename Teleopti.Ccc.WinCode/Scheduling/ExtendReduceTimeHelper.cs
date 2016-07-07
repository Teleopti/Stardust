using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class ExtendReduceTimeHelper : IExtendReduceTimeHelper
	{
		private readonly ILifetimeScope _container;
		private ISchedulingProgress _backgroundWorker;

		public ExtendReduceTimeHelper(ILifetimeScope container)
		{
			_container = container;
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

			IList<IScheduleMatrixPro> matrixList = _container.Resolve<IMatrixListFactory>().CreateMatrixListForSelection(selectedDays);
			lockDaysForExtendReduceOptimization(matrixList, selectedPeriod);

			IList<IScheduleMatrixOriginalStateContainer> originalStateListForScheduleTag = createMatrixContainerList(matrixList);

			IScheduleResultDataExtractorProvider dataExtractorProvider =
				_container.Resolve<IScheduleResultDataExtractorProvider>();

			IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, schedulingResultStateHolder, optimizerPreferences.Advanced);

			IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
			IPeriodValueCalculator allSkillsPeriodValueCalculator =
				periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

			IExtendReduceTimeOptimizerService extendReduceTimeOptimizerService = new ExtendReduceTimeOptimizerService(allSkillsPeriodValueCalculator);

			IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor =
				_container.Resolve<IPossibleMinMaxWorkShiftLengthExtractor>();
			ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator =
				_container.Resolve<ISchedulePeriodTargetTimeCalculator>();
			IWorkShiftWeekMinMaxCalculator workShiftWeekMinMaxCalculator =
				_container.Resolve<IWorkShiftWeekMinMaxCalculator>();
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculatorForFlexibleAgents = new WorkShiftMinMaxCalculator(possibleMinMaxWorkShiftLengthExtractor, schedulePeriodTargetTimeCalculator, workShiftWeekMinMaxCalculator);
			IWorkShiftFinderService workShiftFinderServiceForFlexibleAgents =
				_container.Resolve<IWorkShiftFinderService>(new TypedParameter(typeof(IWorkShiftMinMaxCalculator), workShiftMinMaxCalculatorForFlexibleAgents));
			IScheduleService scheduleServiceForFlexibleAgents =
				_container.Resolve<IScheduleService>(new TypedParameter(typeof(IWorkShiftFinderService), workShiftFinderServiceForFlexibleAgents));

			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
				_container.Resolve<ISchedulePartModifyAndRollbackService>();
			IDeleteSchedulePartService deleteSchedulePartService = _container.Resolve<IDeleteSchedulePartService>();
			IResourceOptimizationHelper resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IEffectiveRestrictionCreator effectiveRestrictionCreator =
				_container.Resolve<IEffectiveRestrictionCreator>();
			IResourceCalculateDaysDecider resourceCalculateDaysDecider =
				_container.Resolve<IResourceCalculateDaysDecider>();
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx =
				_container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

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
				IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro, optimizerPreferences.Advanced);
				IPeriodValueCalculator personalSkillsPeriodValueCalculator =
					periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);

				IExtendReduceTimeDecisionMaker decisionMaker = new ExtendReduceTimeDecisionMaker(scheduleMatrixLockableBitArrayConverterEx);
				
				ICheckerRestriction restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				IOptimizationOverLimitByRestrictionDecider optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateListForScheduleTag[i], dayOffOptimizationPreference);

				IOptimizationLimits optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				ISchedulingOptions schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
				var resourceCalculateDelayer = new ResourceCalculateDelayer(resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, schedulingResultStateHolder);

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
			IScheduleDayEquator scheduleDayEquator = _container.Resolve<IScheduleDayEquator>();
			IList<IScheduleMatrixOriginalStateContainer> result =
				matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();
			return result;
		}
	}
}