using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class BlockOptimizerHelper
    {
        private IWorkShiftFinderResultHolder _allResults;
        private BackgroundWorker _backgroundWorker;
        private readonly ILifetimeScope _container;
        private readonly ScheduleOptimizerHelper _scheduleOptimizerHelper;
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly ISchedulerStateHolder _schedulerStateHolder;

        public BlockOptimizerHelper(ILifetimeScope container, ScheduleOptimizerHelper scheduleOptimizerHelper)
        {
            _container = container;
            _scheduleOptimizerHelper = scheduleOptimizerHelper;
            _stateHolder = _container.Resolve<ISchedulingResultStateHolder>();
            _schedulerStateHolder = _container.Resolve<ISchedulerStateHolder>();
            _scheduleDayChangeCallback = _container.Resolve<IScheduleDayChangeCallback>();
            _allResults = _container.Resolve<IWorkShiftFinderResultHolder>();
            _resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
        }

        public ISchedulingResultStateHolder SchedulingStateHolder
        {
            get { return _stateHolder; }
        }

        private void optimizeDaysOff(IList<IScheduleMatrixPro> matrixList,
            IDayOffTemplate dayOffTemplate,
           DateOnlyPeriod selectedPeriod, IScheduleService scheduleService,
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers)
        {
            var optimizerPreferences = _container.Resolve<IOptimizerOriginalPreferences>();

            var rollbackService = _container.Resolve<ISchedulePartModifyAndRollbackService>();

            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict =
                new SchedulePartModifyAndRollbackService(SchedulingStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.SchedulingOptions.TagToUseOnOptimize));

            DayOffPlannerSessionRuleSet dayOffPlannerRuleSet = 
                OptimizerHelperHelper.DayOffPlannerRuleSetFromOptimizerPreferences(optimizerPreferences);


            IPeriodValueCalculator periodValueCalculator = null;
            IList<IBlockDayOffOptimizerContainer> optimizerContainers = new List<IBlockDayOffOptimizerContainer>();

            for (int index = 0; index < matrixList.Count; index++)
            {

                IScheduleMatrixPro matrix = matrixList[index];
                IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer = matrixOriginalStateContainers[index];

                IScheduleResultDataExtractor personalSkillsDataExtractor =
                    OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences, matrix);
                periodValueCalculator = 
                    OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences, personalSkillsDataExtractor);
                IBlockDayOffOptimizerContainer optimizerContainer =
                    createOptimizer(matrix, dayOffPlannerRuleSet, optimizerPreferences,
                    rollbackService, dayOffTemplate, scheduleService, periodValueCalculator,
                    rollbackServiceDayOffConflict, matrixOriginalStateContainer);
                optimizerContainers.Add(optimizerContainer);
            }

            IScheduleResultDataExtractor allSkillsDataExtractor =
                OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences, selectedPeriod, SchedulingStateHolder);
            OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences, allSkillsDataExtractor);
            var service = new BlockDayOffOptimizationService(periodValueCalculator, rollbackService);
            service.ReportProgress += resourceOptimizer_PersonOptimized;
            service.Execute(optimizerContainers);
            service.ReportProgress -= resourceOptimizer_PersonOptimized;
        }

        private void daysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
                                    BackgroundWorker backgroundWorker,
                                    IDayOffTemplate dayOffTemplate,
                                    bool reschedule)
        {
            _allResults = new WorkShiftFinderResultHolder();
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizerOriginalPreferences>();
            // varför i h-vete skapar man om en likadan klass som finns i
            //optimizerPreferences.DayOffPlannerRules
            DayOffPlannerSessionRuleSet dayOffPlannerRuleSet = OptimizerHelperHelper.DayOffPlannerRuleSetFromOptimizerPreferences(optimizerPreferences);
            IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers = OptimizerHelperHelper.CreateSmartDayOffSolverContainers(matrixOriginalStateContainers, dayOffPlannerRuleSet);

            using (PerformanceOutput.ForOperation("SmartSolver for " + solverContainers.Count + " containers"))
            {
                foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
                {
                    backToLegalStateSolverContainer.Execute();
                    //create list to send to bruteforce
                    if (!backToLegalStateSolverContainer.Result)
                    {
                        backToLegalStateSolverContainer.MatrixOriginalStateContainer.StillAlive = false;
                        IWorkShiftFinderResult workShiftFinderResult =
                            new WorkShiftFinderResult(backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix.Person, DateOnly.Today) { Successful = false };
                        foreach (string descriptionKey in backToLegalStateSolverContainer.FailedSolverDescriptionKeys)
                        {
                            string localizedText = Resources.ResourceManager.GetString(descriptionKey);
                            IWorkShiftFilterResult workShiftFilterResult =
                            new WorkShiftFilterResult(localizedText, 0, 0);
                            workShiftFinderResult.AddFilterResults(workShiftFilterResult);
                        }
                        _allResults.AddResults(new List<IWorkShiftFinderResult> { workShiftFinderResult }, DateTime.Now);

                    }

                }
            }

            using (PerformanceOutput.ForOperation("Moving days off according to solvers"))
            {
                foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
                {
                    if (backToLegalStateSolverContainer.Result)
                        OptimizerHelperHelper.SyncSmartDayOffContainerWithMatrix(backToLegalStateSolverContainer, dayOffTemplate, dayOffPlannerRuleSet, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.SchedulingOptions.TagToUseOnOptimize));
                }
            }

            if (reschedule)
            {
                //call backtolegal
                //reschedule blank days
            }

        }

        

        public void ReOptimize(BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays)
        {
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizerOriginalPreferences>();
            var onlyShiftsWhenUnderstaffed = optimizerPreferences.SchedulingOptions.OnlyShiftsWhenUnderstaffed;

            optimizerPreferences.SchedulingOptions.OnlyShiftsWhenUnderstaffed = false;

            IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);
            IList<IScheduleMatrixPro> matrixListForDayOffOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);
            IList<IScheduleMatrixPro> matrixListForIntradayOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);

            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization = createMatrixContainerList(matrixListForWorkShiftOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization = createMatrixContainerList(matrixListForDayOffOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization = createMatrixContainerList(matrixListForIntradayOptimization);

            var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            var selectedPeriod = new DateOnlyPeriod(OptimizerHelperHelper.GetStartDateInSelectedDays(selectedDays, currentPersonTimeZone), OptimizerHelperHelper.GetEndDateInSelectedDays(selectedDays, currentPersonTimeZone));

            OptimizerHelperHelper.SetConsiderShortBreaks(ScheduleViewBase.AllSelectedPersons(selectedDays), selectedPeriod, optimizerPreferences.SchedulingOptions, _container);

            IScheduleTagSetter tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(optimizerPreferences.SchedulingOptions.TagToUseOnOptimize);

            using (PerformanceOutput.ForOperation("Optimizing " + matrixListForWorkShiftOptimization.Count + " matrixes"))
            {
                if (optimizerPreferences.AdvancedPreferences.AllowDayOffOptimization)
                    runDayOffOptimization(optimizerPreferences, matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod);

                var originalKeepCategorySetting = optimizerPreferences.SchedulingOptions.RescheduleOptions;
                optimizerPreferences.SchedulingOptions.RescheduleOptions = OptimizationRestriction.KeepShiftCategory;

                IList<IScheduleMatrixPro> matrixListForWorkShiftAndIntradayOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container);
                IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization =
                    createMatrixContainerList(matrixListForWorkShiftAndIntradayOptimization);

                if (optimizerPreferences.AdvancedPreferences.AllowWorkShiftOptimization)
                    _scheduleOptimizerHelper.RunWorkShiftOptimization(
                        optimizerPreferences, 
                        matrixOriginalStateContainerListForWorkShiftOptimization, 
                        workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
                        selectedPeriod, 
                        _backgroundWorker);

                if (optimizerPreferences.AdvancedPreferences.AllowIntradayOptimization)
                    _scheduleOptimizerHelper.RunIntradayOptimization(
                        optimizerPreferences, 
                        matrixOriginalStateContainerListForIntradayOptimization, 
                        workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
                        backgroundWorker);

                optimizerPreferences.SchedulingOptions.RescheduleOptions = originalKeepCategorySetting;
            }


            if (optimizerPreferences.SchedulingOptions.UseShiftCategoryLimitations)
            {
                removeShiftCategoryBackToLegalState(matrixListForWorkShiftOptimization, backgroundWorker);
            }
            //set back
            optimizerPreferences.SchedulingOptions.OnlyShiftsWhenUnderstaffed = onlyShiftsWhenUnderstaffed;
        }

        private static IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IList<IScheduleMatrixPro> matrixList)
        {
            IScheduleDayEquator scheduleDayEquator = new ScheduleDayEquator();
            IList<IScheduleMatrixOriginalStateContainer> result =
                matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
                .Cast<IScheduleMatrixOriginalStateContainer>().ToList();
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void runDayOffOptimization(IOptimizerOriginalPreferences optimizerPreferences,
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList, DateOnlyPeriod selectedPeriod)
        {

            if (_backgroundWorker.CancellationPending)
                return;

            // checking for maximum moveble days user settings limitation
            if (optimizerPreferences.AdvancedPreferences.MaximumMovableDayOffPercentagePerPerson == 0)
                return;

            IList<IScheduleMatrixPro> matrixList =
               matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

            OptimizerHelperHelper.LockDaysForDayOffOptimization(optimizerPreferences, matrixList, _container);

            var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots);
            resourceOptimizer_PersonOptimized(this, e);

            // to make sure we are in legal state before we can do day off optimization
            IList<IDayOffTemplate> displayList = (from item in _schedulerStateHolder.CommonStateHolder.DayOffs
                                                  where ((IDeleteTag)item).IsDeleted == false
                                                  select item).ToList();
            ((List<IDayOffTemplate>)displayList).Sort(new DayOffTemplateSorter());
            daysOffBackToLegalState(matrixContainerList, _backgroundWorker,
                                    displayList[0], false);

            e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.Rescheduling + Resources.ThreeDots);
            resourceOptimizer_PersonOptimized(this, e);

            // Schedule White Spots after back to legal state
            var scheduleService = _container.Resolve<IScheduleService>();

            // schedule those are the white spots after back to legal state
            // ??? check if all white spots could be scheduled ??????
            OptimizerHelperHelper.ScheduleBlankSpots(optimizerPreferences.SchedulingOptions, matrixContainerList, scheduleService, _container);

            ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
            bool found = false;
            foreach (IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in matrixContainerList)
            {
                if (!matrixOriginalStateContainer.IsFullyScheduled())
                {
                    found = true;
                    rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService);
                }
            }

            if (found)
            {
                foreach (var dateOnly in selectedPeriod.DayCollection())
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, optimizerPreferences.SchedulingOptions.ConsiderShortBreaks);
                }
            }

            // day off optimization

            optimizeDaysOff(matrixList,
                               displayList[0],
                               selectedPeriod,
                               scheduleService,
                               matrixContainerList);

            // we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
            rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
            foreach (IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in matrixContainerList)
            {
                if (!matrixOriginalStateContainer.IsFullyScheduled())
                    rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService);
            }
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ResourceOptimizerProgressEventArgs.#ctor(Teleopti.Interfaces.Domain.IPerson,System.Double,System.Double,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name);
            resourceOptimizer_PersonOptimized(this, e);

            rollbackService.ClearModificationCollection();
            foreach (IScheduleDayPro scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
            {
                IScheduleDay originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
                rollbackService.Modify(originalPart);
            }
        }

        

        void resourceOptimizer_PersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                e.UserCancel = true;
            }
            _backgroundWorker.ReportProgress(1, e);
        }

        private void removeShiftCategoryBackToLegalState(
            IList<IScheduleMatrixPro> matrixList,
            BackgroundWorker backgroundWorker)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
            using (PerformanceOutput.ForOperation("ShiftCategoryLimitations"))
            {
                var backToLegalStateServicePro =
                    _container.Resolve<ISchedulePeriodListShiftCategoryBackToLegalStateService>();

                if (backgroundWorker.CancellationPending)
                    return;

                backToLegalStateServicePro.Execute(matrixList);
            }
        }

        #region Local

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private IBlockDayOffOptimizerContainer createOptimizer(
            IScheduleMatrixPro scheduleMatrix,
            DayOffPlannerSessionRuleSet ruleSet,
            IOptimizerOriginalPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDayOffTemplate dayOffTemplate,
            IScheduleService scheduleService,
            IPeriodValueCalculator periodValueCalculatorForAllSkills,
            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict,
            IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
                 OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(scheduleMatrix, rollbackService, _container);

            IScheduleMatrixLockableBitArrayConverter scheduleMatrixArrayConverter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);
            ILockableBitArray scheduleMatrixArray = scheduleMatrixArrayConverter.Convert(ruleSet.ConsiderWeekBefore, ruleSet.ConsiderWeekAfter);

            IPerson person = scheduleMatrix.Person;
            CultureInfo culture = person.PermissionInformation.Culture();

            IEnumerable<IDayOffDecisionMaker> decisionMakers = OptimizerHelperHelper.CreateDecisionMakers(culture, person, scheduleMatrixArray, ruleSet, optimizerPreferences);
            IScheduleResultDataExtractor scheduleResultDataExtractor = OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences, scheduleMatrix);

            IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions = new DayOffBackToLegalStateFunctions(scheduleMatrixArray, culture);
            ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService = new SmartDayOffBackToLegalStateService(dayOffBackToLegalStateFunctions, ruleSet, 25);

            var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
            var dayOffOptimizerConflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrix, scheduleService,
                                                                                    effectiveRestrictionCreator,
                                                                                    optimizerPreferences.
                                                                                        SchedulingOptions,
                                                                                        rollbackServiceDayOffConflict);

            
            var dayOffOptimizerValidator = _container.Resolve<IDayOffOptimizerValidator>();
            var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();

            int moveMaxDaysOff = ScheduleOptimizerHelper.MaximumMovableDayOff(optimizerPreferences, scheduleMatrixArrayConverter);
            int moveMaxWorkShift = ScheduleOptimizerHelper.MaximumMovableWorkShift(optimizerPreferences, scheduleMatrixArrayConverter);


            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter
                = new DayOffDecisionMakerExecuter(rollbackService,
                                                  dayOffBackToLegalStateService,
                                                  dayOffTemplate,
                                                  scheduleService,
                                                  optimizerPreferences,
                                                  periodValueCalculatorForAllSkills,
                                                  workShiftBackToLegalStateService,
                                                  ruleSet,
                                                  effectiveRestrictionCreator,
                                                  _resourceOptimizationHelper,
                                                  new ResourceCalculateDaysDecider(),
                                                  dayOffOptimizerValidator,
                                                  dayOffOptimizerConflictHandler, 
                                                  originalStateContainer, 
                                                  moveMaxDaysOff, 
                                                  moveMaxWorkShift, null
                                                  );

            var blockSchedulingService = _container.Resolve<IBlockSchedulingService>();
            var blockCleaner = _container.Resolve<IBlockOptimizerBlockCleaner>();
            var blockDayOffOptimizer = new BlockDayOffOptimizer(scheduleMatrixArrayConverter,
                                                                scheduleResultDataExtractor, ruleSet,
                                                                dayOffDecisionMakerExecuter, blockSchedulingService,
                                                                blockCleaner, new LockableBitArrayChangesTracker(),
                                                                resourceOptimizationHelper);

            IBlockDayOffOptimizerContainer optimizerContainer =
                new BlockDayOffOptimizerContainer(scheduleMatrixArrayConverter,
                                             decisionMakers,
                                             ruleSet,
                                             scheduleMatrix,
                                             optimizerPreferences,
                                             originalStateContainer,
                                             blockDayOffOptimizer);
            return optimizerContainer;
        }

        #endregion

    }
}
