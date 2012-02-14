using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public interface IExtendReduceDaysOffHelper
    {
        void RunExtendReduceDayOffOptimization(IOptimizerOriginalPreferences optimizerPreferences,
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
        private IWorkShiftFinderResultHolder _allResults;

        public ExtendReduceDaysOffHelper(ILifetimeScope container)
        {
            _container = container;
            _allResults = _container.Resolve<IWorkShiftFinderResultHolder>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void RunExtendReduceDayOffOptimization(IOptimizerOriginalPreferences optimizerPreferences,
                                             BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays,
                                             ISchedulerStateHolder schedulerStateHolder,
                                             DateOnlyPeriod selectedPeriod,
                                             IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax)
        {
            if (backgroundWorker.CancellationPending)
                return;

            _backgroundWorker = backgroundWorker;

            if (optimizerPreferences.AdvancedPreferences.MaximumMovableDayOffPercentagePerPerson == 0)
                return;

            IList<IScheduleMatrixPro> matrixList = OptimizerHelperHelper.CreateMatrixList(selectedDays, schedulerStateHolder.SchedulingResultState, _container);
            lockDaysForExtendReduceOptimization(matrixList);

            IList<IScheduleMatrixOriginalStateContainer> originalStateListForScheduleTag = createMatrixContainerList(matrixList);

            IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider(optimizerPreferences);

            IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, schedulerStateHolder.SchedulingResultState);

            IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
            IPeriodValueCalculator allSkillsPeriodValueCalculator =
                periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences, allSkillsDataExtractor);

            //IExtendReduceTimeOptimizerService extendReduceTimeOptimizerService = new ExtendReduceTimeOptimizerService(allSkillsPeriodValueCalculator);
            IExtendReduceDaysOffOptimizerService extendReduceDaysOffOptimizerService = new ExtendReduceDaysOffOptimizerService(allSkillsPeriodValueCalculator);
            


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
                _container.Resolve<IScheduleService>(new TypedParameter(typeof(IWorkShiftFinderService),
                                                                        workShiftFinderServiceForFlexibleAgents));

            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
                _container.Resolve<ISchedulePartModifyAndRollbackService>();
            //IDeleteSchedulePartService deleteSchedulePartService = _container.Resolve<IDeleteSchedulePartService>();
            IResourceOptimizationHelper resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
            IEffectiveRestrictionCreator effectiveRestrictionCreator =
                _container.Resolve<IEffectiveRestrictionCreator>();
            IResourceCalculateDaysDecider resourceCalculateDaysDecider =
                _container.Resolve<IResourceCalculateDaysDecider>();

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
                IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro);
                IPeriodValueCalculator personalSkillsPeriodValueCalculator =
                periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences, personalSkillsDataExtractor);

                
                IExtendReduceDaysOffDecisionMaker decisionMaker = new ExtendReduceDaysOffDecisionMaker();
                IScheduleMatrixLockableBitArrayConverter bitArrayConverter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrixPro);
                ILockableBitArray bitArray =
                    bitArrayConverter.Convert(optimizerPreferences.DayOffPlannerRules.UsePreWeek,
                                              optimizerPreferences.DayOffPlannerRules.UsePostWeek);
                DayOffPlannerSessionRuleSet dayOffPlannerRuleSet = OptimizerHelperHelper.DayOffPlannerRuleSetFromOptimizerPreferences(optimizerPreferences);
                IList<IDayOffLegalStateValidator> validators =
                    OptimizerHelperHelper.CreateLegalStateValidators(scheduleMatrixPro.Person, bitArray,
                                                                     dayOffPlannerRuleSet, optimizerPreferences);

                INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
                new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
                                                    new DeleteSchedulePartService(schedulerStateHolder.SchedulingResultState), schedulePartModifyAndRollbackService,
                                                    scheduleServiceForFlexibleAgents, _allResults);

                IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
                 OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(scheduleMatrixPro, schedulePartModifyAndRollbackService, _container);

                IDayOffsInPeriodCalculator dayOffsInPeriodCalculator = new DayOffsInPeriodCalculator(schedulerStateHolder.SchedulingResultState);

                IList<IDayOffTemplate> displayList = (from item in schedulerStateHolder.CommonStateHolder.DayOffs
                                                  where ((IDeleteTag)item).IsDeleted == false
                                                  select item).ToList();

                IDayOffOptimizerConflictHandler conflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrixPro,
                                                                                                     scheduleServiceForFlexibleAgents,
                                                                                                     effectiveRestrictionCreator,
                                                                                                     optimizerPreferences.SchedulingOptions,
                                                                                                     schedulePartModifyAndRollbackService);

                IWorkTimeStartEndExtractor workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
                INewDayOffRule dayOffRule = new NewDayOffRule(workTimeStartEndExtractor);
                IDayOffOptimizerValidator dayOffOptimizerValidator = new DayOffOptimizerValidator(dayOffRule);

                IExtendReduceDaysOffOptimizer optimizer = new ExtendReduceDaysOffOptimizer(
                    personalSkillsPeriodValueCalculator, personalSkillsDataExtractor, decisionMaker, bitArrayConverter,
                    scheduleServiceForFlexibleAgents, optimizerPreferences, schedulePartModifyAndRollbackService,
                    resourceOptimizationHelper, effectiveRestrictionCreator,
                    resourceCalculateDaysDecider, originalStateListForScheduleTag[i],
                    workShiftBackToLegalStateService, nightRestWhiteSpotSolverService, validators,
                    dayOffsInPeriodCalculator, displayList[0], conflictHandler, dayOffOptimizerValidator);

                optimizers.Add(optimizer);
            }

            extendReduceDaysOffOptimizerService.ReportProgress += extendReduceDaysoffOptimizerService_ReportProgress;
            extendReduceDaysOffOptimizerService.Execute(optimizers);
            extendReduceDaysOffOptimizerService.ReportProgress -= extendReduceDaysoffOptimizerService_ReportProgress;

        }

        private static void lockDaysForExtendReduceOptimization(IList<IScheduleMatrixPro> matrixList)
        {
            IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
            matrixOvertimeLocker.Execute();
            IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
            noMainShiftLocker.Execute();

        }

        private static IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IList<IScheduleMatrixPro> matrixList)
        {
            IScheduleDayEquator scheduleDayEquator = new ScheduleDayEquator();
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