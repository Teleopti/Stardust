using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class ScheduleOptimizerHelper
    {
        private IWorkShiftFinderResultHolder _allResults;
        private BackgroundWorker _backgroundWorker;
        private int _scheduledCount;
        private int _sendEventEvery;
        private readonly ILifetimeScope _container;
        private readonly IExtendReduceTimeHelper _extendReduceTimeHelper;
        private readonly IExtendReduceDaysOffHelper _extendReduceDaysOffHelper;
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IGridlockManager _gridlockManager;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
        private readonly ISchedulerStateHolder _schedulerStateHolder;

        public ScheduleOptimizerHelper(ILifetimeScope container)
        {
            _container = container;
            _extendReduceTimeHelper = new ExtendReduceTimeHelper(_container);
            _extendReduceDaysOffHelper = new ExtendReduceDaysOffHelper(_container);
            _stateHolder = _container.Resolve<ISchedulingResultStateHolder>();
            _schedulerStateHolder = _container.Resolve<ISchedulerStateHolder>();
            _scheduleDayChangeCallback = _container.Resolve<IScheduleDayChangeCallback>();
            _gridlockManager = _container.Resolve<IGridlockManager>();
            _allResults = _container.Resolve<IWorkShiftFinderResultHolder>();
            _resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
            _scheduleMatrixListCreator = _container.Resolve<IScheduleMatrixListCreator>();
        }

        #region Interface

        public ISchedulingResultStateHolder SchedulingStateHolder
        {
            get { return _stateHolder; }
        }

        private void optimizeIntraday(
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
            IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList, 
            IOptimizationPreferences optimizerPreferences)
        {

            ISchedulePartModifyAndRollbackService rollbackService =
                new SchedulePartModifyAndRollbackService(
                    SchedulingStateHolder, 
                    _scheduleDayChangeCallback, 
                    new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

            IIntradayDecisionMaker decisionMaker = new IntradayDecisionMaker();
            var scheduleService = _container.Resolve<IScheduleService>();

            IIntradayOptimizer2Creator creator = new IntradayOptimizer2Creator(
                                                                     matrixContainerList,
                                                                     workShiftContainerList,
                                                                     decisionMaker,
                                                                     scheduleService,
                                                                     optimizerPreferences,
                                                                     rollbackService,
                                                                     SchedulingStateHolder);

            IList<IIntradayOptimizer2> optimizers = creator.Create();
            IScheduleOptimizationService service = new IntradayOptimizerContainer(optimizers);

            service.ReportProgress += resourceOptimizerPersonOptimized;
            service.Execute();
            service.ReportProgress -= resourceOptimizerPersonOptimized;
        }

        private void optimizeWorkShifts(
            IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
            IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerList,
            IOptimizationPreferences optimizerPreferences,
            DateOnlyPeriod selectedPeriod)
        {

            ISchedulePartModifyAndRollbackService rollbackService =
                new SchedulePartModifyAndRollbackService(SchedulingStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

            IScheduleResultDataExtractor allSkillsDataExtractor =
                OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder);
            IPeriodValueCalculator periodValueCalculator = 
                OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

            IMoveTimeDecisionMaker decisionMaker = new MoveTimeDecisionMaker2();
            var scheduleService = _container.Resolve<IScheduleService>();

            IMoveTimeOptimizerCreator creator =
                new MoveTimeOptimizerCreator(scheduleMatrixOriginalStateContainerList,
                                             workShiftOriginalStateContainerList,
                                             decisionMaker,
                                             scheduleService,
                                             optimizerPreferences,
                                             rollbackService,
                                             SchedulingStateHolder);

            IList<IMoveTimeOptimizer> optimizers = creator.Create();
            IScheduleOptimizationService service = new MoveTimeOptimizerContainer(optimizers, periodValueCalculator);

            service.ReportProgress += resourceOptimizerPersonOptimized;
            service.Execute();
            service.ReportProgress -= resourceOptimizerPersonOptimized;
        }


      private void optimizeDaysOff(
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
            IDayOffTemplate dayOffTemplate,
            DateOnlyPeriod selectedPeriod, 
            IScheduleService scheduleService,
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers)
        {
            //if (matrixList == null) throw new ArgumentNullException("matrixList");
            //if (dayOffTemplate == null) throw new ArgumentNullException("dayOffTemplate");
            //if (scheduleService == null) throw new ArgumentNullException("scheduleService");
            //if (matrixOriginalStateContainers == null) throw new ArgumentNullException("matrixOriginalStateContainers");

            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

            ISchedulePartModifyAndRollbackService rollbackService =
                new SchedulePartModifyAndRollbackService(
                    SchedulingStateHolder, 
                    _scheduleDayChangeCallback, 
                    new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict =
                new SchedulePartModifyAndRollbackService(
                    SchedulingStateHolder, 
                    _scheduleDayChangeCallback, 
                    new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

            // varför i h-vete skapar man om en likadan klass som finns i
            // optimizerPreferences.DayOffPlannerRules
            IDayOffPlannerSessionRuleSet dayOffPlannerRuleSet =
                OptimizerHelperHelper.DayOffPlannerRuleSetFromOptimizerPreferences(optimizerPreferences.DaysOff);

            IList<IDayOffOptimizerContainer> optimizerContainers = new List<IDayOffOptimizerContainer>();

            for (int index = 0; index < matrixContainerList.Count; index++)
            {

                IScheduleMatrixPro matrix = matrixContainerList[index].ScheduleMatrix;
                IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer = matrixOriginalStateContainers[index];
                IScheduleResultDataExtractor personalSkillsDataExtractor =
                    OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, matrix);
                IPeriodValueCalculator localPeriodValueCalculator = 
                    OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);
                IDayOffOptimizerContainer optimizerContainer =
                    createOptimizer(matrix, dayOffPlannerRuleSet, optimizerPreferences,
                    rollbackService, dayOffTemplate, scheduleService, localPeriodValueCalculator,
                    rollbackServiceDayOffConflict, matrixOriginalStateContainer);
                optimizerContainers.Add(optimizerContainer);
            }

            IScheduleResultDataExtractor allSkillsDataExtractor =
                OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder);
            IPeriodValueCalculator periodValueCalculator =
                OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

            IDayOffOptimizationService service = new DayOffOptimizationService(periodValueCalculator);
            service.ReportProgress += resourceOptimizerPersonOptimized;
            service.Execute(optimizerContainers);
            service.ReportProgress -= resourceOptimizerPersonOptimized;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ScheduleSelectedStudents(IList<IScheduleDay> allSelectedSchedules, BackgroundWorker backgroundWorker)
        {
            if (allSelectedSchedules == null) throw new ArgumentNullException("allSelectedSchedules");
            var optimizationPreferences = _container.Resolve<IOptimizationPreferences>();
            var schedulingOptions = _container.Resolve<ISchedulingOptions>();
            IList<IScheduleDay> unlockedSchedules = new List<IScheduleDay>();
            foreach (var scheduleDay in allSelectedSchedules)
            {
                GridlockDictionary locks = _gridlockManager.Gridlocks(scheduleDay);
                if (locks == null || locks.Count == 0)
                    unlockedSchedules.Add(scheduleDay);
            }

            if (unlockedSchedules.Count == 0)
            {
                return;
            }

            var selectedPersons = ScheduleViewBase.AllSelectedPersons(unlockedSchedules);
            var selectedPeriod = ScheduleViewBase.AllSelectedDates(unlockedSchedules);
            var sorted = new List<DateOnly>(selectedPeriod);
            sorted.Sort();
            var period = new DateOnlyPeriod(sorted.First(), sorted.Last());

            OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, period, optimizationPreferences.Rescheduling, _container);

            var studentSchedulingService = _container.Resolve<IStudentSchedulingService>();
            IScheduleTagSetter tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(optimizationPreferences.General.ScheduleTag);
            studentSchedulingService.ClearFinderResults();
            _backgroundWorker = backgroundWorker;
            studentSchedulingService.DayScheduled += schedulingServiceDayScheduled;
            DateTime schedulingTime = DateTime.Now;

            var schedulingOptionsCreator = new SchedulingOptionsSynchronizer();
            schedulingOptionsCreator.SynchronizeSchedulingOption(optimizationPreferences, schedulingOptions);

            using (PerformanceOutput.ForOperation("Scheduling " + unlockedSchedules.Count))
            {
                studentSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, false, false);
            }

            _allResults.AddResults(studentSchedulingService.FinderResults, schedulingTime);
            studentSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ScheduleSelectedPersonDays(IList<IScheduleDay> allSelectedSchedules,
                                                bool useOccupancyAdjustment,
                                                ISchedulingOptions schedulingOptions,
                                                BackgroundWorker backgroundWorker)
        {
            if (allSelectedSchedules == null) throw new ArgumentNullException("allSelectedSchedules");
            if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

            schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;

            IList<IScheduleDay> unlockedSchedules = new List<IScheduleDay>();
            foreach (var scheduleDay in allSelectedSchedules)
            {
                GridlockDictionary locks = _gridlockManager.Gridlocks(scheduleDay);
                if (locks == null || locks.Count == 0)
                    unlockedSchedules.Add(scheduleDay);
            }

            if (unlockedSchedules.Count == 0)
            {
                return;
            }

            var selectedPersons = ScheduleViewBase.AllSelectedPersons(unlockedSchedules);
            var selectedPeriod = ScheduleViewBase.AllSelectedDates(unlockedSchedules);
            var sorted = new List<DateOnly>(selectedPeriod);
            sorted.Sort();
            var period = new DateOnlyPeriod(sorted.First(), sorted.Last());

            OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, period, schedulingOptions, _container);

            var stateHolder = _container.Resolve<ISchedulingResultStateHolder>();

            var scheduleTagSetter = _container.Resolve<IScheduleTagSetter>();
            scheduleTagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
            var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();
            var options = _container.Resolve<ISchedulingOptions>();

            fixedStaffSchedulingService.ClearFinderResults();
            _backgroundWorker = backgroundWorker;
            _sendEventEvery = options.RefreshRate;
            _scheduledCount = 0;
            fixedStaffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
            DateTime schedulingTime = DateTime.Now;

            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
                        new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
                                                            new DeleteSchedulePartService(_stateHolder),
                                                            new SchedulePartModifyAndRollbackService(_stateHolder,
                                                                                                     _scheduleDayChangeCallback,
                                                                                                     new ScheduleTagSetter
                                                                                                         (options.
                                                                                                              TagToUseOnScheduling)),
                                                            _container.Resolve<IScheduleService>(), WorkShiftFinderResultHolder);

            using (PerformanceOutput.ForOperation(string.Concat("Scheduling ", unlockedSchedules.Count, " days")))
            {
                ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff = new SchedulePartModifyAndRollbackService(stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
                fixedStaffSchedulingService.DayOffScheduling(unlockedSchedules, schedulePartModifyAndRollbackServiceForContractDaysOff);

                IList<IScheduleMatrixOriginalStateContainer> originalStateContainers =
                    CreateScheduleMatrixOriginalStateContainers(allSelectedSchedules);

                foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
                {
                    foreach (var day in scheduleMatrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
                    {
                        if (day.DaySchedulePart().IsScheduled())
                            scheduleMatrixOriginalStateContainer.ScheduleMatrix.LockPeriod(new DateOnlyPeriod(day.Day, day.Day));
                    }
                }

                fixedStaffSchedulingService.DoTheScheduling(unlockedSchedules, useOccupancyAdjustment, false);
                _allResults.AddResults(fixedStaffSchedulingService.FinderResults, schedulingTime);
                fixedStaffSchedulingService.FinderResults.Clear();
                
                foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
                {
                    int iterations = 0;
                    while (nightRestWhiteSpotSolverService.Resolve(scheduleMatrixOriginalStateContainer.ScheduleMatrix) && iterations < 10)
                    {
                        iterations++;
                    }
                    
                }

                if (options.RotationDaysOnly || options.PreferencesDaysOnly || options.UsePreferencesMustHaveOnly || options.AvailabilityDaysOnly)
                    schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();
            }
            
            fixedStaffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
        }

        private void schedulingServiceDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            _scheduledCount++;
            if (_scheduledCount >= _sendEventEvery)
            {
                _backgroundWorker.ReportProgress(1, e.SchedulePart);
                _scheduledCount = 0;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public IMainShift PrepareAndChooseBestShift(IScheduleDay schedulePart,
            ISchedulingOptions schedulingOptions,
            IWorkShiftFinderService finderService)
        {
            if (schedulePart == null) throw new ArgumentNullException("schedulePart");
            if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");
            if (finderService == null) throw new ArgumentNullException("finderService");
            IEffectiveRestriction effectiveRestriction = getEffectiveRestriction(schedulePart, schedulingOptions);

            DateTime scheduleDayUtc = schedulePart.Period.StartDateTime;
            ICccTimeZoneInfo timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();
            var scheduleDateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(scheduleDayUtc, timeZoneInfo).Date);
            IPersonPeriod personPeriod = schedulePart.Person.Period(scheduleDateOnlyPerson);
            if (personPeriod != null)
            {
                //only fixed staff will be scheduled this way
                if (personPeriod.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
                    //no person assignment
                    //if (schedulePart.PersonAssignmentCollection().Count == 0)
                    if (PreSchedulingStatusChecker.CheckAssignments(schedulePart))
                        //no day off
                        if (schedulePart.PersonDayOffCollection().Count == 0)
                        {
                            DateTime schedulingTime = DateTime.Now;
                            IWorkShiftCalculationResultHolder cache;
                            using (PerformanceOutput.ForOperation("Finding the best shift"))
                            {
                                IScheduleMatrixPro matrix = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(
                                        new List<IScheduleDay> { schedulePart })[0];

                                cache = finderService.FindBestShift(schedulePart, effectiveRestriction, matrix);
                            }
                            var result = finderService.FinderResult;
                            _allResults.AddResults(new List<IWorkShiftFinderResult> { result }, schedulingTime);

                            if (cache == null)
                                return null;

                            result.Successful = true;
                            return cache.ShiftProjection.TheMainShift;
                        }
            }
            return null;
        }

        private static IEffectiveRestriction getEffectiveRestriction(IScheduleDay part, ISchedulingOptions options)
        {
            var extractor = new RestrictionExtractor(null);
            extractor.Extract(part);
            return extractor.CombinedRestriction(options);
        }

        public IWorkShiftFinderResultHolder WorkShiftFinderResultHolder
        {
            get { return _allResults; }
        }

        public void ResetWorkShiftFinderResults()
        {
            _allResults.Clear();
        }


        public void GetBackToLegalState(IList<IScheduleMatrixPro> matrixList,
            ISchedulerStateHolder schedulerStateHolder,
            BackgroundWorker backgroundWorker)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            if (schedulerStateHolder == null) throw new ArgumentNullException("schedulerStateHolder");
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
            var optimizerPreferences = _container.Resolve<IOptimizerOriginalPreferences>();
            foreach (IScheduleMatrixPro scheduleMatrix in matrixList)
            {
                ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
                    new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.SchedulingOptions.TagToUseOnOptimize));
                IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateServicePro = OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(scheduleMatrix, schedulePartModifyAndRollbackService, _container);
                workShiftBackToLegalStateServicePro.Execute(scheduleMatrix);

                backgroundWorker.ReportProgress(1);
            }

            if (optimizerPreferences.SchedulingOptions.UseShiftCategoryLimitations)
            {
                RemoveShiftCategoryBackToLegalState(matrixList, backgroundWorker);
            }

        }

        // verkar inte användas
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dayOffTemplate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        //public IList<IScheduleMatrixPro> DayOffBackToLegalStateBrutal(
        //                            IList<IScheduleMatrixPro> matrixes,
        //                            IOptimizerOriginalPreferences optimizerPreferences,
        //                            BackgroundWorker backgroundWorker,
        //                            IDayOffTemplate dayOffTemplate,
        //                            bool reSchedule)
        //{
        //    _backgroundWorker = backgroundWorker;

        //    IList<IScheduleMatrixPro> failedBruteForce = new List<IScheduleMatrixPro>();
        //    IDayOffPlannerRules dayOffPlannerRules =
        //        optimizerPreferences.DayOffPlannerRules;

        //    IList<ScheduleMatrixBackToLegalStateBrutalForceServiceContainer> serviceContainers = new List<ScheduleMatrixBackToLegalStateBrutalForceServiceContainer>();
        //    foreach (IScheduleMatrixPro matrix in matrixes)
        //    {
        //        IPerson agent = matrix.Person;
        //        CultureInfo cultureInfo = agent.PermissionInformation.Culture();
        //        IScheduleMatrixBitArrayConverter converter = new ScheduleMatrixBitArrayConverter();
        //        ISchedulePeriodDayOffBackToLegalStateByBrutalForceService service =
        //            new SchedulePeriodDayOffBackToLegalStateByBrutalForceService(converter, dayOffPlannerRules, cultureInfo);
        //        var serviceContainer = new ScheduleMatrixBackToLegalStateBrutalForceServiceContainer(service, matrix);
        //        serviceContainers.Add(serviceContainer);
        //    }

        //    using (PerformanceOutput.ForOperation("BruteForceSolver for " + serviceContainers.Count + " containers"))
        //    {
        //        foreach (ScheduleMatrixBackToLegalStateBrutalForceServiceContainer serviceContainer in serviceContainers)
        //        {
        //            serviceContainer.Service.Execute(serviceContainer.ScheduleMatrix);
        //            if (!serviceContainer.Service.Result.Value)
        //            {
        //                failedBruteForce.Add(serviceContainer.ScheduleMatrix);
        //            }
        //        }
        //    }

        //    if (reSchedule)
        //    {
        //        //call backtolegal
        //        //reschedule blank days
        //    }

        //    return failedBruteForce;
        //}

        public void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
                                    BackgroundWorker backgroundWorker,
                                    IDayOffTemplate dayOffTemplate,
                                    bool reschedule)
        {
            _allResults = new WorkShiftFinderResultHolder();
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

            IDayOffPlannerSessionRuleSet dayOffPlannerRuleSet = 
                OptimizerHelperHelper.DayOffPlannerRuleSetFromOptimizerPreferences(optimizerPreferences.DaysOff);

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
                        WorkShiftFinderResultHolder.AddResults(new List<IWorkShiftFinderResult> { workShiftFinderResult }, DateTime.Now);
                    }
                }
            }

            using (PerformanceOutput.ForOperation("Moving days off according to solvers"))
            {
                foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
                {
                    if (backToLegalStateSolverContainer.Result)
                        OptimizerHelperHelper.SyncSmartDayOffContainerWithMatrix(
                            backToLegalStateSolverContainer, 
                            dayOffTemplate, 
                            dayOffPlannerRuleSet, 
                            _scheduleDayChangeCallback, 
                            new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));
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
            _scheduledCount = 0;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
            var onlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;
            _sendEventEvery = optimizerPreferences.Advanced.RefreshScreenInterval;

            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
            IScheduleTagSetter tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(optimizerPreferences.General.ScheduleTag);

            IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);
            IList<IScheduleMatrixPro> matrixListForDayOffOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);
            IList<IScheduleMatrixPro> matrixListForIntradayOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);

            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization = createMatrixContainerList(matrixListForWorkShiftOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization = createMatrixContainerList(matrixListForDayOffOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization = createMatrixContainerList(matrixListForIntradayOptimization);

            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForMoveMax = createMatrixContainerList(matrixListForIntradayOptimization);

            var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            var selectedPeriod = new DateOnlyPeriod(OptimizerHelperHelper.GetStartDateInSelectedDays(selectedDays, currentPersonTimeZone), OptimizerHelperHelper.GetEndDateInSelectedDays(selectedDays, currentPersonTimeZone));

            OptimizerHelperHelper.SetConsiderShortBreaks(ScheduleViewBase.AllSelectedPersons(selectedDays), selectedPeriod, optimizerPreferences.Rescheduling, _container);

            using (PerformanceOutput.ForOperation("Optimizing " + matrixListForWorkShiftOptimization.Count + " matrixes"))
            {
                if (optimizerPreferences.General.OptimizationStepDaysOff)
                    runDayOffOptimization(optimizerPreferences, matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod);

                IList<IScheduleMatrixPro> matrixListForWorkShiftAndIntradayOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container);
                IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization =
                    createMatrixContainerList(matrixListForWorkShiftAndIntradayOptimization);

                if (optimizerPreferences.General.OptimizationStepTimeBetweenDays)
                    RunWorkShiftOptimization(
                        optimizerPreferences, 
                        matrixOriginalStateContainerListForWorkShiftOptimization, 
                        workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
                        selectedPeriod, 
                        _backgroundWorker);

                if (optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
                    _extendReduceDaysOffHelper.RunExtendReduceDayOffOptimization(optimizerPreferences, _backgroundWorker,
                                                                                 selectedDays, _schedulerStateHolder,
                                                                                 selectedPeriod,
                                                                                 matrixOriginalStateContainerListForMoveMax);

                if (optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime)
                    _extendReduceTimeHelper.RunExtendReduceTimeOptimization(optimizerPreferences, _backgroundWorker,
                                                                            selectedDays, SchedulingStateHolder,
                                                                            selectedPeriod,
                                                                            matrixOriginalStateContainerListForMoveMax);

                if (optimizerPreferences.General.OptimizationStepShiftsWithinDay)
                    RunIntradayOptimization(
                        optimizerPreferences, 
                        matrixOriginalStateContainerListForIntradayOptimization,
                        workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization, 
                        backgroundWorker);
            }


            if (optimizerPreferences.General.UseShiftCategoryLimitations)
            {
                RemoveShiftCategoryBackToLegalState(matrixListForWorkShiftOptimization, backgroundWorker);
            }
            //set back
            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = onlyShiftsWhenUnderstaffed;
        }

        private static IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IList<IScheduleMatrixPro> matrixList)
        {
            IScheduleDayEquator scheduleDayEquator = new ScheduleDayEquator();
            IList<IScheduleMatrixOriginalStateContainer> result =
                matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
                .Cast<IScheduleMatrixOriginalStateContainer>().ToList();
            return result;
        }

        internal void RunIntradayOptimization(
            IOptimizationPreferences optimizerPreferences,
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
            IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList, 
            BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
            using (PerformanceOutput.ForOperation("Running new intraday optimization"))
            {

                if (_backgroundWorker.CancellationPending)
                    return;

                ISchedulingResultStateHolder stateHolder = _stateHolder;

                IList<IScheduleMatrixPro> matrixList =
                    matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

                lockDaysForIntradayOptimization(matrixList);

                optimizeIntraday(matrixContainerList, workShiftContainerList, optimizerPreferences);

                // we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
                ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
                foreach (IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in matrixContainerList)
                {
                    if (!matrixOriginalStateContainer.IsFullyScheduled())
                        rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService);
                }
            }
        }

        private static void lockDaysForIntradayOptimization(IList<IScheduleMatrixPro> matrixList)
        {
            IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
            matrixOvertimeLocker.Execute();
            IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
            noMainShiftLocker.Execute();
        }

        internal void RunWorkShiftOptimization(
           IOptimizationPreferences optimizerPreferences,
           IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
           IList<IScheduleMatrixOriginalStateContainer> workshiftOriginalStateContainerList,
           DateOnlyPeriod selectedPeriod,
           BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
            using (PerformanceOutput.ForOperation("Running move time optimization"))
            {
                if (_backgroundWorker.CancellationPending)
                    return;

                IList<IScheduleMatrixPro> matrixList =
                    scheduleMatrixOriginalStateContainerList.Select(container => container.ScheduleMatrix).ToList();

                lockDaysForWorkShiftOptimization(matrixList);

                optimizeWorkShifts(scheduleMatrixOriginalStateContainerList, workshiftOriginalStateContainerList, optimizerPreferences, selectedPeriod);

                // we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
                ISchedulePartModifyAndRollbackService rollbackService =
                    new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
                foreach (
                    IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in scheduleMatrixOriginalStateContainerList)
                {
                    if (!matrixOriginalStateContainer.IsFullyScheduled())
                    {

                        rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService);
                    }

                }
            }
        }

        private static void lockDaysForWorkShiftOptimization(IList<IScheduleMatrixPro> matrixList)
        {
            IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
            matrixOvertimeLocker.Execute();
            IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
            noMainShiftLocker.Execute();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void runDayOffOptimization(IOptimizationPreferences optimizerPreferences,
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList, DateOnlyPeriod selectedPeriod)
        {

            if (_backgroundWorker.CancellationPending)
                return;

            IList<IScheduleMatrixPro> matrixList = matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

            OptimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, _container);

            var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots);
            resourceOptimizerPersonOptimized(this, e);

            // to make sure we are in legal state before we can do day off optimization
            IList<IDayOffTemplate> displayList = (from item in _schedulerStateHolder.CommonStateHolder.DayOffs
                                                  where ((IDeleteTag)item).IsDeleted == false
                                                  select item).ToList();
            ((List<IDayOffTemplate>)displayList).Sort(new DayOffTemplateSorter());
            DaysOffBackToLegalState(matrixContainerList, _backgroundWorker,
                                    displayList[0], false);

            e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.Rescheduling + Resources.ThreeDots);
            resourceOptimizerPersonOptimized(this, e);

            // Schedule White Spots after back to legal state
            var scheduleService = _container.Resolve<IScheduleService>();

            // schedule those are the white spots after back to legal state
            OptimizerHelperHelper.ScheduleBlankSpots(matrixContainerList, scheduleService, _container);
            ISchedulePartModifyAndRollbackService rollbackService = 
                new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

            bool notFullyScheduledMatrixFound = false;
            IList<IScheduleMatrixOriginalStateContainer> validMatrixContainerList = new List<IScheduleMatrixOriginalStateContainer>();
            foreach (IScheduleMatrixOriginalStateContainer matrixContainer in matrixContainerList)
            {
                bool isFullyScheduled = matrixContainer.IsFullyScheduled();
                if (!isFullyScheduled)

                {
                    notFullyScheduledMatrixFound = true;
                    rollbackMatrixChanges(matrixContainer, rollbackService);
                    continue;
                }
                validMatrixContainerList.Add(matrixContainer);
            }

            if (notFullyScheduledMatrixFound)
            {
                foreach (var dateOnly in selectedPeriod.DayCollection())
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, optimizerPreferences.Rescheduling.ConsiderShortBreaks);
                }
            }

            optimizeDaysOff(validMatrixContainerList,
                            displayList[0],
                            selectedPeriod,
                            scheduleService,
                            matrixContainerList);

            // we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
            rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
            foreach (IScheduleMatrixOriginalStateContainer matrixContainer in validMatrixContainerList)
            {
                if (!matrixContainer.IsFullyScheduled())
                    rollbackMatrixChanges(matrixContainer, rollbackService);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name);
            resourceOptimizerPersonOptimized(this, e);

            rollbackService.ClearModificationCollection();
            foreach (IScheduleDayPro scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
            {
                IScheduleDay originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
                rollbackService.Modify(originalPart);
            }
        }

        void resourceOptimizerPersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                e.UserCancel = true;
            }
            _backgroundWorker.ReportProgress(1, e);
        }

        public void RemoveShiftCategoryBackToLegalState(
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

        public static IGroupPagePerDate CreateGroupPagePerDate(IScheduleViewBase currentView, IGroupPageDataProvider groupPageDataProvider, IGroupPage selectedGrouping)
        {
            IDictionary<DateOnly, IGroupPage> dic = new Dictionary<DateOnly, IGroupPage>();
            DateOnlyPeriod selectedPeriod = GetSelectedPeriod(currentView);
            foreach (var dateOnly in selectedPeriod.DayCollection())
            {
                IGroupPage groupPage = createGroupPageForDate(groupPageDataProvider, selectedGrouping, dateOnly);
                dic.Add(dateOnly, groupPage);
            }
            return new GroupPagePerDate(dic);
        }

        public static IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, IGroupPage selectedGrouping)
        {
            if (dates == null) throw new ArgumentNullException("dates");
            if (groupPageDataProvider == null) throw new ArgumentNullException("groupPageDataProvider");
            IDictionary<DateOnly, IGroupPage> dic = new Dictionary<DateOnly, IGroupPage>();

            foreach (var dateOnly in dates)
            {
                var groupPage = createGroupPageForDate(groupPageDataProvider, selectedGrouping, dateOnly);
                dic.Add(dateOnly, groupPage);
            }

            return new GroupPagePerDate(dic);
        }

        /// <summary>
        /// Used to display a combo of groupings when starting groupScheduling
        /// </summary>
        /// <param name="currentView">The current view.</param>
        /// <param name="stateHolder">The state holder.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IList<IGroupPage> CreateGroupPages(IScheduleViewBase currentView, ISchedulerStateHolder stateHolder)
        {
            DateOnlyPeriod selectedPeriod = GetSelectedPeriod(currentView);


            IGroupPageDataProvider dataProvider = new GroupScheduleGroupPageDataProvider(stateHolder, new RepositoryFactory(),
                                                                                         UnitOfWorkFactory.Current);
            ((GroupScheduleGroupPageDataProvider)dataProvider).SetSelectedPeriod(selectedPeriod);
            IGroupingsCreator groupingsCreator = new GroupingsCreator(dataProvider);
            IList<IGroupPage> pages = groupingsCreator.CreateBuiltInGroupPages(true);
            foreach (var userDefinedGrouping in dataProvider.UserDefinedGroupings)
            {
                pages.Add(userDefinedGrouping);
            }

            return pages;

        }

        public static DateOnlyPeriod GetSelectedPeriod(IScheduleViewBase currentView)
        {
            if (currentView == null) throw new ArgumentNullException("currentView");
            DateOnly minDate = DateOnly.MaxValue;
            DateOnly maxDate = DateOnly.MinValue;
            foreach (var dateOnly in currentView.AllSelectedDates())
            {
                if (dateOnly < minDate)
                    minDate = dateOnly;

                if (dateOnly > maxDate)
                    maxDate = dateOnly;
            }

            return new DateOnlyPeriod(minDate, maxDate);
        }

        private static IGroupPage createGroupPageForDate(IGroupPageDataProvider groupPageDataProvider, IGroupPage selectedGrouping, DateOnly dateOnly)
        {
            IGroupPage groupPage;
            IGroupPageOptions options = new GroupPageOptions(groupPageDataProvider.PersonCollection)
                                            {
                                                SelectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly),
                                                CurrentGroupPageName = selectedGrouping.Description.Name,
                                                CurrentGroupPageNameKey = selectedGrouping.DescriptionKey
                                            };

            switch (selectedGrouping.DescriptionKey)
            {
                case "Main":
                    {
                        var personGroupPage = new PersonGroupPage();
                        groupPage = personGroupPage.CreateGroupPage(groupPageDataProvider.BusinessUnitCollection, options);
                        break;
                    }
                case "Contracts":
                    {
                        var contractGroupPage = new ContractGroupPage();
                        groupPage = contractGroupPage.CreateGroupPage(groupPageDataProvider.ContractCollection, options);
                        break;
                    }
                case "ContractSchedule":
                    {
                        var contractScheduleGroupPage = new ContractScheduleGroupPage();
                        groupPage = contractScheduleGroupPage.CreateGroupPage(groupPageDataProvider.ContractScheduleCollection, options);
                        break;
                    }
                case "PartTimepercentages":
                    {
                        var partTimePercentageGroupPage = new PartTimePercentageGroupPage();
                        groupPage = partTimePercentageGroupPage.CreateGroupPage(groupPageDataProvider.PartTimePercentageCollection, options);
                        break;
                    }
                case "Note":
                    {
                        var personNoteGroupPage = new PersonNoteGroupPage();
                        groupPage = personNoteGroupPage.CreateGroupPage(null, options);
                        break;
                    }
                case "RuleSetBag":
                    {
                        var ruleSetBagGroupPage = new RuleSetBagGroupPage();
                        groupPage = ruleSetBagGroupPage.CreateGroupPage(groupPageDataProvider.RuleSetBagCollection, options);
                        break;
                    }
                default:
                    {
                        groupPage = selectedGrouping;
                        break;
                    }
            }
            return groupPage;
        }

        public void GroupSchedule(BackgroundWorker backgroundWorker, IList<IScheduleDay> scheduleDays)
        {
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
            var schedulingOptions = _container.Resolve<ISchedulingOptions>();
            _sendEventEvery = schedulingOptions.RefreshRate;
            _backgroundWorker = backgroundWorker;

            DateOnlyPeriod selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(scheduleDays);

            IGroupPageDataProvider groupPageDataProvider = _container.Resolve<GroupScheduleGroupPageDataProvider>();
            var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();
            groupPagePerDateHolder.GroupPersonGroupPagePerDate = CreateGroupPagePerDate(selectedPeriod.DayCollection(),
                                                                                          groupPageDataProvider,
                                                                                          schedulingOptions.GroupOnGroupPage);


            IList<IPerson> selectedPersons = new PersonListExtractorFromScheduleParts(scheduleDays).ExtractPersons().ToList();

            _allResults.Clear();

            var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();
            fixedStaffSchedulingService.ClearFinderResults();

            var groupSchedulingService = _container.Resolve<IGroupSchedulingService>();
            IScheduleTagSetter tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
            fixedStaffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceforContractDaysOff = new SchedulePartModifyAndRollbackService(SchedulingStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
            fixedStaffSchedulingService.DayOffScheduling(scheduleDays, schedulePartModifyAndRollbackServiceforContractDaysOff);


            fixedStaffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
            groupSchedulingService.DayScheduled += schedulingServiceDayScheduled;
            groupSchedulingService.Execute(selectedPeriod, selectedPersons, backgroundWorker);
            groupSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

            _allResults.AddResults(fixedStaffSchedulingService.FinderResults, DateTime.Now);

            if (schedulingOptions.RotationDaysOnly)
                schedulePartModifyAndRollbackServiceforContractDaysOff.Rollback();
        }

        public void BlockSchedule(IList<IScheduleDay> allScheduleDays,
                                BackgroundWorker backgroundWorker)
        {
            if (allScheduleDays == null) throw new ArgumentNullException("allScheduleDays");
            _backgroundWorker = backgroundWorker;

            var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();
            fixedStaffSchedulingService.ClearFinderResults();

            var schedulingOptions = _container.Resolve<ISchedulingOptions>();

            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff = new SchedulePartModifyAndRollbackService(SchedulingStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
            fixedStaffSchedulingService.DayOffScheduling(allScheduleDays, schedulePartModifyAndRollbackServiceForContractDaysOff);

            IScheduleTagSetter tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);

            IList<IScheduleMatrixPro> matrixes = OptimizerHelperHelper.CreateMatrixList(allScheduleDays, SchedulingStateHolder, _container);

            IDictionary<string, IWorkShiftFinderResult> schedulingResults = new Dictionary<string, IWorkShiftFinderResult>();


            var blockSchedulingService = _container.Resolve<IBlockSchedulingService>();
            _allResults = new WorkShiftFinderResultHolder();

            //int refreshRate = optimizerPreferences.OriginalOptimizerPreferences.SchedulingOptions.RefreshRate;
            int refreshRate = schedulingOptions.RefreshRate;

            schedulingOptions.RefreshRate = 1;
            blockSchedulingService.BlockScheduled += blockSchedulingServiceBlockScheduled;

            using (PerformanceOutput.ForOperation("Scheduling x blocks"))
                blockSchedulingService.Execute(matrixes, schedulingOptions.UseBlockScheduling, schedulingResults);

            if (schedulingOptions.RotationDaysOnly)
                schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();

            blockSchedulingService.BlockScheduled -= blockSchedulingServiceBlockScheduled;
            schedulingOptions.RefreshRate = refreshRate;
            _allResults.AddResults(new List<IWorkShiftFinderResult>(schedulingResults.Values), DateTime.Now);
            _allResults.AddResults(fixedStaffSchedulingService.FinderResults, DateTime.Now);
        }

        void blockSchedulingServiceBlockScheduled(object sender, BlockSchedulingServiceEventArgs e)
        {
            if (_backgroundWorker.IsBusy)
                _backgroundWorker.ReportProgress(-e.PercentageCompleted);
        }

        public IList<IScheduleMatrixOriginalStateContainer> CreateScheduleMatrixOriginalStateContainers(IList<IScheduleDay> scheduleDays)
        {
            IList<IScheduleMatrixOriginalStateContainer> retList = new List<IScheduleMatrixOriginalStateContainer>();
            IScheduleDayEquator scheduleDayEquator = new ScheduleDayEquator();
            foreach (IScheduleMatrixPro scheduleMatrixPro in OptimizerHelperHelper.CreateMatrixList(scheduleDays, SchedulingStateHolder, _container))
                retList.Add(new ScheduleMatrixOriginalStateContainer(scheduleMatrixPro, scheduleDayEquator));

            return retList;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void ReOptimizeIntradayActivity(
            BackgroundWorker backgroundWorker,
            IOptimizerActivitiesPreferences preferences,
            IList<IScheduleDay> scheduleDays)
        {
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
            if (preferences == null) throw new ArgumentNullException("preferences");

            _backgroundWorker = backgroundWorker;

            foreach (IScheduleDay scheduleDay in scheduleDays.GetRandom(scheduleDays.Count, true))
            {
                if (_backgroundWorker.CancellationPending)
                    return;

                GridlockDictionary locks = _gridlockManager.Gridlocks(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly);
                if (locks != null && locks.Count != 0)
                    continue;
                reOptimizeIntradayActivityOnScheduleDay(
                     scheduleDay, preferences);
            }
            //reset
            var shiftProjectionCacheFilter = _container.Resolve<IShiftProjectionCacheFilter>();
            shiftProjectionCacheFilter.SetMainShiftOptimizeActivitiesSpecification(new Domain.Specification.All<IMainShift>());
        }

        #endregion

        #region Local

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private IDayOffOptimizerContainer createOptimizer(
            IScheduleMatrixPro scheduleMatrix,
            IDayOffPlannerSessionRuleSet ruleSet,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDayOffTemplate dayOffTemplate,
            IScheduleService scheduleService,
            IPeriodValueCalculator periodValueCalculatorForAllSkills,
            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict,
            IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
                 OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(scheduleMatrix, rollbackService, _container);

            IScheduleMatrixLockableBitArrayConverter scheduleMatrixArrayConverter =
                new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);
            ILockableBitArray scheduleMatrixArray =
                scheduleMatrixArrayConverter.Convert(ruleSet.ConsiderWeekBefore, ruleSet.ConsiderWeekAfter);

            IPerson person = scheduleMatrix.Person;
            // create decisionmakers
            CultureInfo culture = person.PermissionInformation.Culture();

            IEnumerable<IDayOffDecisionMaker> decisionMakers =
                OptimizerHelperHelper.CreateDecisionMakers(culture, person, scheduleMatrixArray, ruleSet, optimizerPreferences);
            IScheduleResultDataExtractor scheduleResultDataExtractor = OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, scheduleMatrix);

            IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions = new DayOffBackToLegalStateFunctions(scheduleMatrixArray, culture);
            ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService = new SmartDayOffBackToLegalStateService(dayOffBackToLegalStateFunctions, ruleSet, 25);

            var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
            var dayOffOptimizerConflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrix, scheduleService,
                                                                                    effectiveRestrictionCreator,
                                                                                    rollbackServiceDayOffConflict);

            var dayOffOptimizerValidator = _container.Resolve<IDayOffOptimizerValidator>();

            var restrictionChecker = new RestrictionChecker();
            var optimizationUserPreferences = _container.Resolve<IOptimizationPreferences>();
            var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(originalStateContainer, restrictionChecker, optimizationUserPreferences);

            var schedulingOptionsSyncronizer = new SchedulingOptionsSynchronizer();

            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
                new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
                                                    new DeleteSchedulePartService(_stateHolder), rollbackService,
                                                    scheduleService, WorkShiftFinderResultHolder);

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
                                                  optimizerOverLimitDecider,
                                                  nightRestWhiteSpotSolverService,
                                                  schedulingOptionsSyncronizer);

            IDayOffOptimizerContainer optimizerContainer =
                new DayOffOptimizerContainer(scheduleMatrixArrayConverter,
                                             decisionMakers,
                                             scheduleResultDataExtractor,
                                             ruleSet,
                                             scheduleMatrix,
                                             dayOffDecisionMakerExecuter,
                                             originalStateContainer);
            return optimizerContainer;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ResourceOptimizerProgressEventArgs.#ctor(Teleopti.Interfaces.Domain.IPerson,System.Double,System.Double,System.String)")]
        private void reOptimizeIntradayActivityOnScheduleDay(
            IScheduleDay scheduleDay,
            IOptimizerActivitiesPreferences preferences)
        {
            if (scheduleDay.SignificantPart() != SchedulePartView.MainShift)
                return;

            IPersonAssignment personAssignment = scheduleDay.AssignmentHighZOrder();

            if (personAssignment != null && !personAssignment.OvertimeShiftCollection.IsEmpty())
                return;

            if (scheduleDay.PersonAbsenceCollection().Count > 0)
                return;

            ISpecification<IMainShift> mainShiftOptimizeActivitiesSpecification =
                 new MainShiftOptimizeActivitiesSpecification(preferences, scheduleDay.AssignmentHighZOrder().MainShift,
                                                              scheduleDay.DateOnlyAsPeriod.DateOnly,
                                                              StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);

            var shiftProjectionCacheFilter = _container.Resolve<IShiftProjectionCacheFilter>();
            shiftProjectionCacheFilter.SetMainShiftOptimizeActivitiesSpecification(mainShiftOptimizeActivitiesSpecification);

            var intradayActivityOptimizerService = _container.Resolve<IIntradayActivityOptimizerService>();
            bool result = intradayActivityOptimizerService.Optimize(scheduleDay);
            string msg = Resources.Success;
            if (!result)
                msg = Resources.Unsuccessful;
            string message = Resources.OptimizeActivities + ": " +
                             scheduleDay.Person.Name.ToString(NameOrderOption.FirstNameLastName) + " " + msg;
            var args = new ResourceOptimizerProgressEventArgs(scheduleDay.Person, 0, 0, message);
            resourceOptimizerPersonOptimized(this, args);
            //return result;
        }



        #endregion

    }
}
