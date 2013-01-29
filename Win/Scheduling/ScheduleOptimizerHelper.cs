﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
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
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
    	private readonly ISingleSkillDictionary _singleSkillDictionary;
    	private readonly IDaysOffSchedulingService _daysOffSchedulingService;

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
            _groupPersonBuilderForOptimization = _container.Resolve<IGroupPersonBuilderForOptimization>();
        	_singleSkillDictionary = _container.Resolve<ISingleSkillDictionary>();
        	_daysOffSchedulingService = _container.Resolve<IDaysOffSchedulingService>();
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
                                                                     SchedulingStateHolder,
																	 _singleSkillDictionary);

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
                                             SchedulingStateHolder,
											 _singleSkillDictionary);

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
            IScheduleService scheduleService)
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


            IList<IDayOffOptimizerContainer> optimizerContainers = new List<IDayOffOptimizerContainer>();

            for (int index = 0; index < matrixContainerList.Count; index++)
            {
				IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer = matrixContainerList[index];
                IScheduleMatrixPro matrix = matrixContainerList[index].ScheduleMatrix;
                IScheduleResultDataExtractor personalSkillsDataExtractor =
                    OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, matrix);
                IPeriodValueCalculator localPeriodValueCalculator = 
                    OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);
                IDayOffOptimizerContainer optimizerContainer =
                    createOptimizer(matrix, optimizerPreferences.DaysOff, optimizerPreferences,
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
        public void ScheduleSelectedStudents(IList<IScheduleDay> allSelectedSchedules, BackgroundWorker backgroundWorker, ISchedulingOptions schedulingOptions)
        {
            if (allSelectedSchedules == null) throw new ArgumentNullException("allSelectedSchedules");
			if(schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

            var optimizationPreferences = _container.Resolve<IOptimizationPreferences>();
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
            var tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
            studentSchedulingService.ClearFinderResults();
            _backgroundWorker = backgroundWorker;
            studentSchedulingService.DayScheduled += schedulingServiceDayScheduled;
            DateTime schedulingTime = DateTime.Now;
        	ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder,
        	                                                                                                 _scheduleDayChangeCallback,
        	                                                                                                 new ScheduleTagSetter
        	                                                                                                 	(schedulingOptions
        	                                                                                                 	 	.
        	                                                                                                 	 	TagToUseOnScheduling));
            using (PerformanceOutput.ForOperation("Scheduling " + unlockedSchedules.Count))
            {
                studentSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, false, false, rollbackService);
            }

            _allResults.AddResults(studentSchedulingService.FinderResults, schedulingTime);
            studentSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ScheduleSelectedPersonDays(IList<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, bool useOccupancyAdjustment, BackgroundWorker backgroundWorker, ISchedulingOptions schedulingOptions)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            
            schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;

            var unlockedSchedules = (from scheduleMatrixPro in matrixList
                                     from scheduleDayPro in scheduleMatrixPro.UnlockedDays
                                     select scheduleDayPro.DaySchedulePart()).ToList();

            if (!unlockedSchedules.Any()) return;

            var selectedPersons = matrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();

            var selectedPeriod = ScheduleViewBase.AllSelectedDates(unlockedSchedules);
            var sorted = new List<DateOnly>(selectedPeriod);
            sorted.Sort();
            var period = new DateOnlyPeriod(sorted.First(), sorted.Last());

            OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, period, schedulingOptions, _container);

            var stateHolder = _container.Resolve<ISchedulingResultStateHolder>();

            var scheduleTagSetter = _container.Resolve<IScheduleTagSetter>();
            scheduleTagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
            var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();
            

            fixedStaffSchedulingService.ClearFinderResults();
            _backgroundWorker = backgroundWorker;
            _sendEventEvery = schedulingOptions.RefreshRate;
            _scheduledCount = 0;
            fixedStaffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
            DateTime schedulingTime = DateTime.Now;
			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService =
				new DeleteAndResourceCalculateService(new DeleteSchedulePartService(_stateHolder), resourceOptimizationHelper);
			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder,
			                                                                                                 _scheduleDayChangeCallback,
			                                                                                                 new ScheduleTagSetter
			                                                                                                 	(schedulingOptions.
			                                                                                                 	 	TagToUseOnScheduling));
            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService = 
                        new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
															deleteAndResourceCalculateService,
                                                            _container.Resolve<IScheduleService>(), WorkShiftFinderResultHolder,
															new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, schedulingOptions.ConsiderShortBreaks));

            using (PerformanceOutput.ForOperation(string.Concat("Scheduling ", unlockedSchedules.Count, " days")))
            {
                ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff = new SchedulePartModifyAndRollbackService(stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				_daysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
				_daysOffSchedulingService.Execute(matrixList, matrixListAll, schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions);
				_daysOffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

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

                fixedStaffSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, useOccupancyAdjustment, false, rollbackService);
                _allResults.AddResults(fixedStaffSchedulingService.FinderResults, schedulingTime);
                fixedStaffSchedulingService.FinderResults.Clear();

                foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
                {
                    int iterations = 0;
                    while (nightRestWhiteSpotSolverService.Resolve(scheduleMatrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, rollbackService) && iterations < 10)
                    {
                        iterations++;
                    }

                }

                if (schedulingOptions.RotationDaysOnly || schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly || schedulingOptions.AvailabilityDaysOnly)
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

            DateTime scheduleDayUtc = schedulePart.Period.StartDateTime;
            TimeZoneInfo timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();
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

                            	var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
                            	var effectiveRestriction = effectiveRestrictionCreator.GetEffectiveRestriction(
                            		schedulePart, schedulingOptions);
                                cache = finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction, null);
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

        public IWorkShiftFinderResultHolder WorkShiftFinderResultHolder
        {
            get { return _allResults; }
        }

        public void ResetWorkShiftFinderResults()
        {
            _allResults.Clear();
        }


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public void GetBackToLegalState(IList<IScheduleMatrixPro> matrixList,
            ISchedulerStateHolder schedulerStateHolder,
            BackgroundWorker backgroundWorker,
			ISchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixPro> allMatrixes )
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            if (schedulerStateHolder == null) throw new ArgumentNullException("schedulerStateHolder");
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
            foreach (IScheduleMatrixPro scheduleMatrix in matrixList)
            {
                ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
					new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
                IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateServicePro = OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);
				workShiftBackToLegalStateServicePro.Execute(scheduleMatrix, schedulingOptions, schedulePartModifyAndRollbackService);

                backgroundWorker.ReportProgress(1);
            }

            if (optimizerPreferences.General.UseShiftCategoryLimitations)
            {
            	RemoveShiftCategoryBackToLegalState(matrixList, backgroundWorker, optimizerPreferences, schedulingOptions, selectedPeriod, allMatrixes);
            }

        }

        // verkar inte användas. Vet inte om den någonsin kommer att användas, den löser alltid problemet men det kan ta 14 dagar
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
                                    BackgroundWorker backgroundWorker,
                                    IDayOffTemplate dayOffTemplate,
                                    bool reschedule, 
									ISchedulingOptions schedulingOptions,
									IDaysOffPreferences daysOffPreferences)
        {
			if(schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

            _allResults = new WorkShiftFinderResultHolder();
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

            IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers =
				OptimizerHelperHelper.CreateSmartDayOffSolverContainers(matrixOriginalStateContainers, daysOffPreferences);

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
					{
						OptimizerHelperHelper.SyncSmartDayOffContainerWithMatrix(
							backToLegalStateSolverContainer,
							dayOffTemplate,
							daysOffPreferences,
							_scheduleDayChangeCallback,
							new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

						var restrictionChecker = new RestrictionChecker();
						var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
						var originalStateContainer = backToLegalStateSolverContainer.MatrixOriginalStateContainer;
						var optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(matrix,
						                                                                                              restrictionChecker,
						                                                                                              optimizerPreferences,
						                                                                                              originalStateContainer);
						if(optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit() || optimizationOverLimitByRestrictionDecider.OverLimit().Count > 0)
						{
							var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
							rollbackMatrixChanges(originalStateContainer, rollbackService);
						}
					}
                }
            }

            if (reschedule)
            {
                //call backtolegal
                //reschedule blank days
            }

        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void ReOptimize(BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays, IList<IScheduleMatrixPro> allMatrixes)
        {
            _backgroundWorker = backgroundWorker;
            _scheduledCount = 0;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
            var onlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;
            _sendEventEvery = optimizerPreferences.Advanced.RefreshScreenInterval;

            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
            var tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(optimizerPreferences.General.ScheduleTag);
			IList<IPerson> selectedPersons = new List<IPerson>(ScheduleViewBase.AllSelectedPersons(selectedDays));
            IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);
            IList<IScheduleMatrixPro> matrixListForDayOffOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);
            IList<IScheduleMatrixPro> matrixListForIntradayOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, SchedulingStateHolder, _container);

            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization = createMatrixContainerList(matrixListForWorkShiftOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization = createMatrixContainerList(matrixListForDayOffOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization = createMatrixContainerList(matrixListForIntradayOptimization);

            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForMoveMax = createMatrixContainerList(matrixListForIntradayOptimization);

            var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            var selectedPeriod = new DateOnlyPeriod(OptimizerHelperHelper.GetStartDateInSelectedDays(selectedDays, currentPersonTimeZone), OptimizerHelperHelper.GetEndDateInSelectedDays(selectedDays, currentPersonTimeZone));
			
			OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, selectedPeriod, optimizerPreferences.Rescheduling, _container);

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

				if (optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime)
					_extendReduceTimeHelper.RunExtendReduceTimeOptimization(optimizerPreferences, _backgroundWorker,
																			selectedDays, SchedulingStateHolder,
																			selectedPeriod,
																			matrixOriginalStateContainerListForMoveMax);

                if (optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
                    _extendReduceDaysOffHelper.RunExtendReduceDayOffOptimization(optimizerPreferences, _backgroundWorker,
                                                                                 selectedDays, _schedulerStateHolder,
                                                                                 selectedPeriod,
                                                                                 matrixOriginalStateContainerListForMoveMax);

                if (optimizerPreferences.General.OptimizationStepShiftsWithinDay)
                    RunIntradayOptimization(
                        optimizerPreferences, 
                        matrixOriginalStateContainerListForIntradayOptimization,
                        workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization, 
                        backgroundWorker);

				if (optimizerPreferences.General.OptimizationStepFairness)
					runFairness(selectedDays, tagSetter, selectedPersons, optimizerPreferences);
            }


            if (optimizerPreferences.General.UseShiftCategoryLimitations)
            {
				var schedulingOptionsCreator = new SchedulingOptionsCreator();
				var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);

            	RemoveShiftCategoryBackToLegalState(matrixListForWorkShiftOptimization, backgroundWorker,
            	                                    optimizerPreferences, schedulingOptions, selectedPeriod, allMatrixes);
            }

            //set back
            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = onlyShiftsWhenUnderstaffed;
        }

		private void runFairness(IList<IScheduleDay> selectedDays, IScheduleTagSetter tagSetter, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizerPreferences)
		{
			var matrixListForFairness = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container);
			var fairnessOpt = _container.Resolve<IShiftCategoryFairnessOptimizer>();
			var selectedDates = OptimizerHelperHelper.GetSelectedPeriod(selectedDays).DayCollection();
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, new EmptyScheduleDayChangeCallback(), tagSetter);
			fairnessOpt.ReportProgress += resourceOptimizerPersonOptimized;
			fairnessOpt.ExecutePersonal(_backgroundWorker, selectedPersons, selectedDates, matrixListForFairness,
										optimizerPreferences.Extra.GroupPageOnCompareWith, rollbackService, optimizerPreferences.Advanced.UseAverageShiftLengths);
			fairnessOpt.ReportProgress -= resourceOptimizerPersonOptimized;
		}

        private static IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
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
			IMatrixMultipleShiftsLocker matrixMultipleShiftsLocker = new MatrixMultipleShiftsLocker(matrixList);
			matrixMultipleShiftsLocker.Execute();
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
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizerPreferences);
            DaysOffBackToLegalState(matrixContainerList, _backgroundWorker,
                                    displayList[0], false, schedulingOptions, optimizerPreferences.DaysOff);

			var workShiftBackToLegalStateService =
				OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

			ISchedulePartModifyAndRollbackService rollbackService =
			   new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			foreach (var matrixOriginalStateContainer in matrixContainerList)
			{
				rollbackService.ClearModificationCollection();
				workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions,
														 rollbackService);
			}

            e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.Rescheduling + Resources.ThreeDots);
            resourceOptimizerPersonOptimized(this, e);

            // Schedule White Spots after back to legal state
            var scheduleService = _container.Resolve<IScheduleService>();
			
            // schedule those are the white spots after back to legal state
            OptimizerHelperHelper.ScheduleBlankSpots(matrixContainerList, scheduleService, _container, rollbackService);
           

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
                            scheduleService);

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void RemoveShiftCategoryBackToLegalState(
            IList<IScheduleMatrixPro> matrixList,
			BackgroundWorker backgroundWorker, IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes )
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
			if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");
            using (PerformanceOutput.ForOperation("ShiftCategoryLimitations"))
            {
                if(schedulingOptions.UseGroupScheduling && schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
                {
                    var backToLegalStateServicePro =
                    _container.Resolve<IGroupListShiftCategoryBackToLegalStateService>();

                    if (backgroundWorker.CancellationPending)
                        return;
                    var groupOptimizerFindMatrixesForGroup =
                        new GroupOptimizerFindMatrixesForGroup(_groupPersonBuilderForOptimization, matrixList);

					var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
					var coherentChecker = new TeamSteadyStateCoherentChecker();
					var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
					var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(coherentChecker, scheduleMatrixProFinder, resourceOptimizationHelper);
					var groupPersonsBuilder = _container.Resolve<IGroupPersonsBuilder>();
					var targetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
                	var teamSteadyStateRunner = new TeamSteadyStateRunner(allMatrixes, targetTimeCalculator);
					var teamSteadyStateCreator = new TeamSteadyStateDictionaryCreator(teamSteadyStateRunner, allMatrixes, groupPersonsBuilder, schedulingOptions);
					var teamSteadyStateDictionary = teamSteadyStateCreator.Create(selectedPeriod);
                	var teamSteadyStateHolder = new TeamSteadyStateHolder(teamSteadyStateDictionary);
					IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization = new GroupPersonBuilderForOptimization(_schedulerStateHolder.SchedulingResultState, _container.Resolve<IGroupPersonFactory>(), _container.Resolve<IGroupPagePerDateHolder>());

                    backToLegalStateServicePro.Execute(matrixList, schedulingOptions, optimizationPreferences, groupOptimizerFindMatrixesForGroup, teamSteadyStateHolder, teamSteadyStateMainShiftScheduler, groupPersonBuilderForOptimization);
                }else
                {
                    var backToLegalStateServicePro =
                    _container.Resolve<ISchedulePeriodListShiftCategoryBackToLegalStateService>();

                    if (backgroundWorker.CancellationPending)
                        return;

                    backToLegalStateServicePro.Execute(matrixList, schedulingOptions, optimizationPreferences);
                }
            }
        }

		//public IGroupPagePerDate CreateGroupPagePerDate(IScheduleViewBase currentView, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping)
		//{
		//    return _container.Resolve<IGroupPageCreator>().CreateGroupPagePerDate(currentView, groupPageDataProvider, selectedGrouping);
		//    //IDictionary<DateOnly, IGroupPage> dic = new Dictionary<DateOnly, IGroupPage>();
		//    //DateOnlyPeriod selectedPeriod = GetSelectedPeriod(currentView);
		//    //foreach (var dateOnly in selectedPeriod.DayCollection())
		//    //{
		//    //    IGroupPage groupPage = createGroupPageForDate(groupPageDataProvider, selectedGrouping,  dateOnly);
		//    //    dic.Add(dateOnly, groupPage);
		//    //}
		//    //return new GroupPagePerDate(dic);
		//}

		//public IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping)
		//{
		//    return _container.Resolve<IGroupPageCreator>().CreateGroupPagePerDate(dates, groupPageDataProvider, selectedGrouping);
		//    //if (dates == null) throw new ArgumentNullException("dates");
		//    //if (groupPageDataProvider == null) throw new ArgumentNullException("groupPageDataProvider");
		//    //IDictionary<DateOnly, IGroupPage> dic = new Dictionary<DateOnly, IGroupPage>();

		//    //foreach (var dateOnly in dates)
		//    //{
		//    //    var groupPage = createGroupPageForDate(groupPageDataProvider, selectedGrouping, dateOnly);
		//    //    dic.Add(dateOnly, groupPage);
		//    //}

		//    //return new GroupPagePerDate(dic);
		//}

		///// <summary>
		///// Used to display a combo of groupings when starting groupScheduling
		///// </summary>
		///// <param name="currentView">The current view.</param>
		///// <param name="stateHolder">The state holder.</param>
		///// <returns></returns>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		//public IList<IGroupPage> CreateGroupPages(IScheduleViewBase currentView, ISchedulerStateHolder stateHolder)
		//{
		//    DateOnlyPeriod selectedPeriod = GetSelectedPeriod(currentView);


		//    IGroupPageDataProvider dataProvider = new GroupScheduleGroupPageDataProvider(stateHolder, new RepositoryFactory(),
		//                                                                                 UnitOfWorkFactory.Current);
		//    ((GroupScheduleGroupPageDataProvider)dataProvider).SetSelectedPeriod(selectedPeriod);
		//    IGroupingsCreator groupingsCreator = new GroupingsCreator(dataProvider);
		//    IList<IGroupPage> pages = groupingsCreator.CreateBuiltInGroupPages(true);
		//    foreach (var userDefinedGrouping in dataProvider.UserDefinedGroupings)
		//    {
		//        pages.Add(userDefinedGrouping);
		//    }

		//    return pages;

		//}

		//public static DateOnlyPeriod GetSelectedPeriod(IScheduleViewBase currentView)
		//{
		//    if (currentView == null) throw new ArgumentNullException("currentView");
		//    DateOnly minDate = DateOnly.MaxValue;
		//    DateOnly maxDate = DateOnly.MinValue;
		//    foreach (var dateOnly in currentView.AllSelectedDates())
		//    {
		//        if (dateOnly < minDate)
		//            minDate = dateOnly;

		//        if (dateOnly > maxDate)
		//            maxDate = dateOnly;
		//    }

		//    return new DateOnlyPeriod(minDate, maxDate);
		//}

		//private static IGroupPage createGroupPageForDate(IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping,  DateOnly dateOnly)
		//{
		//    IGroupPage groupPage;
		//    IGroupPageOptions options = new GroupPageOptions(groupPageDataProvider.PersonCollection)
		//                                    {
		//                                        SelectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly),
		//                                        CurrentGroupPageName = selectedGrouping.Name,
		//                                        CurrentGroupPageNameKey = selectedGrouping.Key
		//                                    };

		//    switch (selectedGrouping.Key)
		//    {
		//        case "Main":
		//            {
		//                var personGroupPage = new PersonGroupPage();
		//                groupPage = personGroupPage.CreateGroupPage(groupPageDataProvider.BusinessUnitCollection, options);
		//                break;
		//            }
		//        case "Contracts":
		//            {
		//                var contractGroupPage = new ContractGroupPage();
		//                groupPage = contractGroupPage.CreateGroupPage(groupPageDataProvider.ContractCollection, options);
		//                break;
		//            }
		//        case "ContractSchedule":
		//            {
		//                var contractScheduleGroupPage = new ContractScheduleGroupPage();
		//                groupPage = contractScheduleGroupPage.CreateGroupPage(groupPageDataProvider.ContractScheduleCollection, options);
		//                break;
		//            }
		//        case "PartTimepercentages":
		//            {
		//                var partTimePercentageGroupPage = new PartTimePercentageGroupPage();
		//                groupPage = partTimePercentageGroupPage.CreateGroupPage(groupPageDataProvider.PartTimePercentageCollection, options);
		//                break;
		//            }
		//        case "Note":
		//            {
		//                var personNoteGroupPage = new PersonNoteGroupPage();
		//                groupPage = personNoteGroupPage.CreateGroupPage(null, options);
		//                break;
		//            }
		//        case "RuleSetBag":
		//            {
		//                var ruleSetBagGroupPage = new RuleSetBagGroupPage();
		//                groupPage = ruleSetBagGroupPage.CreateGroupPage(groupPageDataProvider.RuleSetBagCollection, options);
		//                break;
		//            }
		//        default:
		//            {
		//                groupPage = null;// selectedGrouping;
		//                var groups = groupPageDataProvider.UserDefinedGroupings;
		//                foreach (var group in groups)
		//                {
		//                    if (group.IdOrDescriptionKey.Equals(selectedGrouping.Key))
		//                    {
		//                        groupPage = group;
		//                        break;
		//                    }
		//                }
						
		//                break;
		//            }
		//    }
		//    return groupPage;
		//}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "groupPageHelper"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void GroupSchedule(BackgroundWorker backgroundWorker, IList<IScheduleDay> scheduleDays, IList<IScheduleMatrixPro> matrixList, 
			IList<IScheduleMatrixPro> matrixListAll, ISchedulingOptions schedulingOptions, IGroupPageHelper groupPageHelper, IList<IScheduleMatrixPro> allMatrixes)
        {
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");

            var unlockedSchedules = (from scheduleMatrixPro in matrixList
                                     from scheduleDayPro in scheduleMatrixPro.UnlockedDays
                                     select scheduleDayPro.DaySchedulePart()).ToList();

            if (!unlockedSchedules.Any()) return;

            _sendEventEvery = schedulingOptions.RefreshRate;
            _backgroundWorker = backgroundWorker;

            DateOnlyPeriod selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(scheduleDays);

            IGroupPageDataProvider groupPageDataProvider = _container.Resolve<IGroupScheduleGroupPageDataProvider>();
            var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();
            groupPagePerDateHolder.GroupPersonGroupPagePerDate = _container.Resolve<IGroupPageCreator>().CreateGroupPagePerDate(selectedPeriod.DayCollection(),
                                                                                          groupPageDataProvider,
																						  schedulingOptions.GroupOnGroupPage);


            var selectedPersons = matrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();

            _allResults.Clear();

            var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();
            fixedStaffSchedulingService.ClearFinderResults();

            var groupSchedulingService = _container.Resolve<IGroupSchedulingService>();
            var tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
            fixedStaffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceforContractDaysOff = new SchedulePartModifyAndRollbackService(SchedulingStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			_daysOffSchedulingService.Execute(matrixList, matrixListAll, schedulePartModifyAndRollbackServiceforContractDaysOff, schedulingOptions);

			var targetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
			var groupPersonsBuilder = _container.Resolve<IGroupPersonsBuilder>();
			var teamSteadyStateRunner = new TeamSteadyStateRunner(allMatrixes, targetTimeCalculator);
			var teamSteadyStateCreator = new TeamSteadyStateDictionaryCreator(teamSteadyStateRunner, allMatrixes, groupPersonsBuilder, schedulingOptions);
			var teamSteadyStateDictionary = teamSteadyStateCreator.Create(selectedPeriod);

			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization = new GroupPersonBuilderForOptimization(_schedulerStateHolder.SchedulingResultState, _container.Resolve<IGroupPersonFactory>(), _container.Resolve<IGroupPagePerDateHolder>());
			var coherentChecker = new TeamSteadyStateCoherentChecker();
			var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
			var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(coherentChecker, scheduleMatrixProFinder, resourceOptimizationHelper);
			var teamSteadyStateHolder = new TeamSteadyStateHolder(teamSteadyStateDictionary);
			
            fixedStaffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
            groupSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			groupSchedulingService.Execute(selectedPeriod, matrixList, schedulingOptions, selectedPersons, backgroundWorker, teamSteadyStateHolder, teamSteadyStateMainShiftScheduler, groupPersonBuilderForOptimization);
			
            groupSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

            _allResults.AddResults(fixedStaffSchedulingService.FinderResults, DateTime.Now);

            if (schedulingOptions.RotationDaysOnly)
                schedulePartModifyAndRollbackServiceforContractDaysOff.Rollback();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void BlockSchedule(IList<IScheduleDay> allScheduleDays, IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, BackgroundWorker backgroundWorker, ISchedulingOptions schedulingOptions)
        {
            if (allScheduleDays == null) throw new ArgumentNullException("allScheduleDays");

            var unlockedSchedules = (from scheduleMatrixPro in matrixList
                                     from scheduleDayPro in scheduleMatrixPro.UnlockedDays
                                     select scheduleDayPro.DaySchedulePart()).ToList();

            if (!unlockedSchedules.Any()) return;

            _backgroundWorker = backgroundWorker;

            var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();
            fixedStaffSchedulingService.ClearFinderResults();

            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff = new SchedulePartModifyAndRollbackService(SchedulingStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			_daysOffSchedulingService.Execute(matrixList, matrixListAll, schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions);

            var tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);

            IList<IScheduleMatrixPro> matrixes = matrixList;

            IDictionary<string, IWorkShiftFinderResult> schedulingResults = new Dictionary<string, IWorkShiftFinderResult>();


            var blockSchedulingService = _container.Resolve<IBlockSchedulingService>();
            _allResults = new WorkShiftFinderResultHolder();

            int refreshRate = schedulingOptions.RefreshRate;

            schedulingOptions.RefreshRate = 1;
            blockSchedulingService.BlockScheduled += blockSchedulingServiceBlockScheduled;

			schedulingOptions.UseGroupSchedulingCommonCategory = true;
			schedulingOptions.UseGroupSchedulingCommonEnd = false;
			schedulingOptions.UseGroupSchedulingCommonStart = false;

		    using (PerformanceOutput.ForOperation("Scheduling x blocks"))
		            blockSchedulingService.Execute(matrixes, schedulingOptions, schedulingResults);


		    if (schedulingOptions.RotationDaysOnly)
                schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();

            blockSchedulingService.BlockScheduled -= blockSchedulingServiceBlockScheduled;
            schedulingOptions.RefreshRate = refreshRate;
            _allResults.AddResults(new List<IWorkShiftFinderResult>(schedulingResults.Values), DateTime.Now);
            _allResults.AddResults(fixedStaffSchedulingService.FinderResults, DateTime.Now);
        }

        public void BlockScheduleForAdvanceScheduling(IList<IScheduleDay> allScheduleDays, IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, BackgroundWorker backgroundWorker, ISchedulingOptions schedulingOptions)
        {
            //if (allScheduleDays == null) throw new ArgumentNullException("allScheduleDays");

            //var unlockedSchedules = (from scheduleMatrixPro in matrixList
            //                         from scheduleDayPro in scheduleMatrixPro.UnlockedDays
            //                         select scheduleDayPro.DaySchedulePart()).ToList();

            //if (!unlockedSchedules.Any()) return;

            //_backgroundWorker = backgroundWorker;

            var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();
            fixedStaffSchedulingService.ClearFinderResults();

            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff = new SchedulePartModifyAndRollbackService(SchedulingStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
            //_daysOffSchedulingService.Execute(matrixList, matrixListAll, schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions);

            //var tagSetter = _container.Resolve<IScheduleTagSetter>();
            //tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);

            //IList<IScheduleMatrixPro> matrixes = matrixList;

            IDictionary<string, IWorkShiftFinderResult> schedulingResults = new Dictionary<string, IWorkShiftFinderResult>();


            var blockSchedulingService = _container.Resolve<IBlockSchedulingService>();
            //_allResults = new WorkShiftFinderResultHolder();

            int refreshRate = schedulingOptions.RefreshRate;

            schedulingOptions.RefreshRate = 1;
            blockSchedulingService.BlockScheduled += blockSchedulingServiceBlockScheduled;

            //schedulingOptions.UseGroupSchedulingCommonCategory = true;
            //schedulingOptions.UseGroupSchedulingCommonEnd = false;
            //schedulingOptions.UseGroupSchedulingCommonStart = false;

                schedulingOptions.UseTwoDaysOffAsBlock = true;
                var skillDayPeriodIntervalData = _container.Resolve<ISkillDayPeriodIntervalData>();
            //var skillDayPeriodIntervalData = new SkillDayPeriodIntervalData(skillIntervalDataSkillFactorApplyer,
            //                                                                intervalDataMedianCalculator,
            //                                                                schedulingResultStateHolder);
                var dynamicBlockFinder = new DynamicBlockFinder(schedulingOptions, _stateHolder);
                var teamExtractor = new TeamExtractor(matrixList, _groupPersonBuilderForOptimization);
                var restrictionAggregator = _container.Resolve<IRestrictionAggregator>();
                var workShiftFilterService = _container.Resolve<IWorkShiftFilterService>();


                var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
                                                                            schedulingOptions.ConsiderShortBreaks);
                ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
                    new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback,
                                                             new ScheduleTagSetter(
                                                                 schedulingOptions.TagToUseOnScheduling));
                var teamScheduling = new TeamScheduling(resourceCalculateDelayer, schedulePartModifyAndRollbackService);

                IWorkShiftPeriodValueCalculator workShiftPeriodValueCalculator = new WorkShiftPeriodValueCalculator();
                IWorkShiftLengthValueCalculator workShiftLengthValueCalculator = new WorkShiftLengthValueCalculator();
                IWorkShiftValueCalculator workShiftValueCalculator =
                    new WorkShiftValueCalculator(workShiftPeriodValueCalculator, workShiftLengthValueCalculator);
                IEqualWorkShiftValueDecider equalWorkShiftValueDecider = new EqualWorkShiftValueDecider();
                IWorkShiftSelector workShiftSelector = new WorkShiftSelector(workShiftValueCalculator,
                                                                             equalWorkShiftValueDecider);
                IGroupPersonBuilderBasedOnContractTime groupPersonBuilderBasedOnContractTime =
                    new GroupPersonBuilderBasedOnContractTime(_container.Resolve<IGroupPersonFactory>());
                var advanceSchedulingService = new AdvanceSchedulingService(skillDayPeriodIntervalData,
                                                                            dynamicBlockFinder, teamExtractor,
                                                                            restrictionAggregator, matrixList,
                                                                            workShiftFilterService, teamScheduling,
                                                                            schedulingOptions, workShiftSelector,
                                                                            groupPersonBuilderBasedOnContractTime);

                advanceSchedulingService.Execute(schedulingResults);

            
            if (schedulingOptions.RotationDaysOnly)
                schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();

            blockSchedulingService.BlockScheduled -= blockSchedulingServiceBlockScheduled;
            schedulingOptions.RefreshRate = refreshRate;
            _allResults.AddResults(new List<IWorkShiftFinderResult>(schedulingResults.Values), DateTime.Now);
            _allResults.AddResults(fixedStaffSchedulingService.FinderResults, DateTime.Now);
        }

        void blockSchedulingServiceBlockScheduled(object sender, BlockSchedulingServiceEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }

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

        #endregion
        
        #region Local

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private IDayOffOptimizerContainer createOptimizer(
            IScheduleMatrixPro scheduleMatrix,
            IDaysOffPreferences daysOffPreferences,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDayOffTemplate dayOffTemplate,
            IScheduleService scheduleService,
            IPeriodValueCalculator periodValueCalculatorForAllSkills,
            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict,
            IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
                 OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

            IScheduleMatrixLockableBitArrayConverter scheduleMatrixArrayConverter =
                new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);
            ILockableBitArray scheduleMatrixArray =
                scheduleMatrixArrayConverter.Convert(daysOffPreferences.ConsiderWeekBefore, daysOffPreferences.ConsiderWeekAfter);

            // create decisionmakers

            IEnumerable<IDayOffDecisionMaker> decisionMakers =
                OptimizerHelperHelper.CreateDecisionMakers(scheduleMatrixArray, optimizerPreferences);
            IScheduleResultDataExtractor scheduleResultDataExtractor = OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, scheduleMatrix);

            IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions = new DayOffBackToLegalStateFunctions(scheduleMatrixArray);
			IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(new OfficialWeekendDays(), new TrueFalseRandomizer());
            ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService = new SmartDayOffBackToLegalStateService(dayOffBackToLegalStateFunctions, daysOffPreferences, 25, cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);

            var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, true);

            var dayOffOptimizerConflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrix, scheduleService,
                                                                                    effectiveRestrictionCreator,
                                                                                    rollbackServiceDayOffConflict,
																					resourceCalculateDelayer);

            var dayOffOptimizerValidator = _container.Resolve<IDayOffOptimizerValidator>();

            var restrictionChecker = new RestrictionChecker();
            var optimizationUserPreferences = _container.Resolve<IOptimizationPreferences>();
            var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrix, restrictionChecker, optimizationUserPreferences, originalStateContainer);

            var schedulingOptionsSyncronizer = new SchedulingOptionsCreator();
			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService =
				new DeleteAndResourceCalculateService(new DeleteSchedulePartService(_stateHolder), resourceOptimizationHelper);
            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
                new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
													deleteAndResourceCalculateService,
                                                    scheduleService, WorkShiftFinderResultHolder,
													resourceCalculateDelayer);
			var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
			var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(_stateHolder);
			var populationStatisticsCalculator = new PopulationStatisticsCalculator();
			var deviationStatisticData = new DeviationStatisticData();
        	var dayOffOptimizerPreMoveResultPredictor =
        		new DayOffOptimizerPreMoveResultPredictor(dailySkillForecastAndScheduledValueCalculator,
        		                                          populationStatisticsCalculator, deviationStatisticData);

            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter
                = new DayOffDecisionMakerExecuter(rollbackService,
                                                  dayOffBackToLegalStateService,
                                                  dayOffTemplate,
                                                  scheduleService,
                                                  optimizerPreferences,
                                                  periodValueCalculatorForAllSkills,
                                                  workShiftBackToLegalStateService,
                                                  effectiveRestrictionCreator,
                                                  _resourceOptimizationHelper,
                                                  new ResourceCalculateDaysDecider(),
                                                  dayOffOptimizerValidator,
                                                  dayOffOptimizerConflictHandler,
                                                  originalStateContainer,
                                                  optimizerOverLimitDecider,
                                                  nightRestWhiteSpotSolverService,
                                                  schedulingOptionsSyncronizer,
												  mainShiftOptimizeActivitySpecificationSetter,
												  dayOffOptimizerPreMoveResultPredictor);

            IDayOffOptimizerContainer optimizerContainer =
                new DayOffOptimizerContainer(scheduleMatrixArrayConverter,
                                             decisionMakers,
                                             scheduleResultDataExtractor,
                                             daysOffPreferences,
                                             scheduleMatrix,
                                             dayOffDecisionMakerExecuter,
                                             originalStateContainer);
            return optimizerContainer;
        }

        #endregion

    }
}
