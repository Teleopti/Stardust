using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public interface IExtendReduceTimeHelper
    {
        void RunExtendReduceTimeOptimization(IOptimizerOriginalPreferences optimizerPreferences,
                                             BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays,
                                             ISchedulingResultStateHolder schedulingResultStateHolder,
                                             DateOnlyPeriod selectedPeriod,
                                             IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax);
    }

    public class ExtendReduceTimeHelper : IExtendReduceTimeHelper
    {
        private readonly ILifetimeScope _container;
        private BackgroundWorker _backgroundWorker;

        public ExtendReduceTimeHelper(ILifetimeScope container)
        {
            _container = container;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void RunExtendReduceTimeOptimization(IOptimizerOriginalPreferences optimizerPreferences,
                                             BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays,
                                             ISchedulingResultStateHolder schedulingResultStateHolder,
                                             DateOnlyPeriod selectedPeriod,
                                             IList<IScheduleMatrixOriginalStateContainer> originalStateListForMoveMax)
        {
            if (backgroundWorker.CancellationPending)
                return;

            _backgroundWorker = backgroundWorker;

            if (optimizerPreferences.AdvancedPreferences.MaximumMovableWorkShiftPercentagePerPerson == 0)
                return;

            IList<IScheduleMatrixPro> matrixList = OptimizerHelperHelper.CreateMatrixList(selectedDays, schedulingResultStateHolder, _container);
            lockDaysForExtendReduceOptimization(matrixList);

            IList<IScheduleMatrixOriginalStateContainer> originalStateListForScheduleTag = createMatrixContainerList(matrixList);

            IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider(optimizerPreferences);
            
            IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, schedulingResultStateHolder);

            IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
            IPeriodValueCalculator allSkillsPeriodValueCalculator =
                periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences, allSkillsDataExtractor);

            IExtendReduceTimeOptimizerService extendReduceTimeOptimizerService = new ExtendReduceTimeOptimizerService(allSkillsPeriodValueCalculator);

            

            IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor =
                _container.Resolve<IPossibleMinMaxWorkShiftLengthExtractor>();
            ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator =
                _container.Resolve<ISchedulePeriodTargetTimeCalculator>();
            IWorkShiftWeekMinMaxCalculator workShiftWeekMinMaxCalculator =
                _container.Resolve<IWorkShiftWeekMinMaxCalculator>();
            IWorkShiftMinMaxCalculator workShiftMinMaxCalculatorForFlexibleAgents =new WorkShiftMinMaxCalculator(possibleMinMaxWorkShiftLengthExtractor, schedulePeriodTargetTimeCalculator, workShiftWeekMinMaxCalculator);
            IWorkShiftFinderService workShiftFinderServiceForFlexibleAgents =
                _container.Resolve<IWorkShiftFinderService>(new TypedParameter(typeof(IWorkShiftMinMaxCalculator), workShiftMinMaxCalculatorForFlexibleAgents));
            IScheduleService scheduleServiceForFlexibleAgents =
                _container.Resolve<IScheduleService>(new TypedParameter(typeof (IWorkShiftFinderService),
                                                                        workShiftFinderServiceForFlexibleAgents));

            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
                _container.Resolve<ISchedulePartModifyAndRollbackService>();
            IDeleteSchedulePartService deleteSchedulePartService = _container.Resolve<IDeleteSchedulePartService>();
            IResourceOptimizationHelper resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
            IEffectiveRestrictionCreator effectiveRestrictionCreator =
                _container.Resolve<IEffectiveRestrictionCreator>();
            IResourceCalculateDaysDecider resourceCalculateDaysDecider =
                _container.Resolve<IResourceCalculateDaysDecider>();

            IList<IExtendReduceTimeOptimizer> optimizers = new List<IExtendReduceTimeOptimizer>();
            for (int i = 0; i < matrixList.Count; i++)
            {
                IScheduleMatrixPro scheduleMatrixPro = matrixList[i];
                IVirtualSchedulePeriod virtualSchedulePeriod =
                    scheduleMatrixPro.Person.VirtualSchedulePeriod(scheduleMatrixPro.EffectivePeriodDays[0].Day);
                if(!virtualSchedulePeriod.IsValid)
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

                IExtendReduceTimeDecisionMaker decisionMaker = new ExtendReduceTimeDecisionMaker();
                IScheduleMatrixLockableBitArrayConverter bitArrayConverter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrixPro);


                IExtendReduceTimeOptimizer optimizer = new ExtendReduceTimeOptimizer(
                    personalSkillsPeriodValueCalculator, personalSkillsDataExtractor, decisionMaker, bitArrayConverter,
                    scheduleServiceForFlexibleAgents, optimizerPreferences, schedulePartModifyAndRollbackService,
                    deleteSchedulePartService, resourceOptimizationHelper, effectiveRestrictionCreator,
                    resourceCalculateDaysDecider, originalStateListForMoveMax[i], originalStateListForScheduleTag[i]);

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
                e.UserCancel = true;
            }
            _backgroundWorker.ReportProgress(1, e);
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
    }
}