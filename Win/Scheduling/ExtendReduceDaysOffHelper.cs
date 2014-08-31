using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Win.Commands;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public interface IExtendReduceDaysOffHelper
    {
        void RunExtendReduceDayOffOptimization(IOptimizationPreferences optimizerPreferences,
                                               BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays,
                                               ISchedulerStateHolder schedulerStateHolder,
                                               DateOnlyPeriod selectedPeriod,
                                               IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class ExtendReduceDaysOffHelper : IExtendReduceDaysOffHelper
    {
        private readonly ILifetimeScope _container;
        private BackgroundWorker _backgroundWorker;
        private readonly IWorkShiftFinderResultHolder _allResults;

        public ExtendReduceDaysOffHelper(ILifetimeScope container)
        {
            _container = container;
            _allResults = _container.Resolve<IWorkShiftFinderResultHolder>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void RunExtendReduceDayOffOptimization(IOptimizationPreferences optimizerPreferences,
                                             BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays,
                                             ISchedulerStateHolder schedulerStateHolder,
                                             DateOnlyPeriod selectedPeriod,
                                             IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax)
        {
            if (backgroundWorker.CancellationPending)
                return;

            _backgroundWorker = backgroundWorker;

			IList<IScheduleMatrixPro> matrixList = _container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);
            lockDaysForExtendReduceOptimization(matrixList, selectedPeriod);

            IList<IScheduleMatrixOriginalStateContainer> originalStateListForScheduleTag = createMatrixContainerList(matrixList);

	        IScheduleResultDataExtractorProvider dataExtractorProvider =
		        _container.Resolve<IScheduleResultDataExtractorProvider>();

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

            var schedulePartModifyAndRollbackService =
                _container.Resolve<ISchedulePartModifyAndRollbackService>();
            var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
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

                
                IExtendReduceDaysOffDecisionMaker decisionMaker = new ExtendReduceDaysOffDecisionMaker();
                IScheduleMatrixLockableBitArrayConverter bitArrayConverter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrixPro, scheduleMatrixLockableBitArrayConverterEx);
                ILockableBitArray bitArray =
                    bitArrayConverter.Convert(optimizerPreferences.DaysOff.ConsiderWeekBefore,
                                              optimizerPreferences.DaysOff.ConsiderWeekAfter);
	            var daysOffLegalStateValidatorsFactory = _container.Resolve<IDaysOffLegalStateValidatorsFactory>();
	            var validators = daysOffLegalStateValidatorsFactory.CreateLegalStateValidators(bitArray,
	                                                                                           optimizerPreferences);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_container.Resolve<IResourceOptimizationHelper>(), 1, true,
																		true);

                INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
                new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
                                                    new DeleteAndResourceCalculateService(new DeleteSchedulePartService( schedulerStateHolder.SchedulingResultState), resourceOptimizationHelper),
													scheduleServiceForFlexibleAgents, _allResults, resourceCalculateDelayer);

                IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
                 OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

                IDayOffsInPeriodCalculator dayOffsInPeriodCalculator = new DayOffsInPeriodCalculator(schedulerStateHolder.SchedulingResultState);

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
                IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrixPro, checkerRestriction, optimizerPreferences, originalStateListForScheduleTag[i]);
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
                
                IExtendReduceDaysOffOptimizer optimizer = new ExtendReduceDaysOffOptimizer(
                    personalSkillsPeriodValueCalculator, 
                    personalSkillsDataExtractor, 
                    decisionMaker, 
                    bitArrayConverter,
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
                    optimizationOverLimitDecider,
                    schedulingOptionsCreator,
					mainShiftOptimizeActivitySpecificationSetter);

                optimizers.Add(optimizer);
            }

            extendReduceDaysOffOptimizerService.ReportProgress += extendReduceDaysoffOptimizerService_ReportProgress;
            extendReduceDaysOffOptimizerService.Execute(optimizers);
            extendReduceDaysOffOptimizerService.ReportProgress -= extendReduceDaysoffOptimizerService_ReportProgress;

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

        private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
        {
			IScheduleDayEquator scheduleDayEquator = _container.Resolve<IScheduleDayEquator>();
            IList<IScheduleMatrixOriginalStateContainer> result =
                matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
                .Cast<IScheduleMatrixOriginalStateContainer>().ToList();
            return result;
        }

        void extendReduceDaysoffOptimizerService_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                e.UserCancel = true;
            }
            _backgroundWorker.ReportProgress(1, e);
        }
    }
}