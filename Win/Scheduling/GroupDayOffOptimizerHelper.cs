using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class GroupDayOffOptimizerHelper
    {
        private readonly ILifetimeScope _container;
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private BackgroundWorker _backgroundWorker;
        private readonly ISchedulerStateHolder _schedulerState;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        public GroupDayOffOptimizerHelper(ILifetimeScope container)
        {
            _container = container;
            _schedulerState = _container.Resolve<ISchedulerStateHolder>();
            _stateHolder = _schedulerState.SchedulingResultState;
            _resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
            _scheduleDayChangeCallback = _container.Resolve<IScheduleDayChangeCallback>();
            _groupPersonBuilderForOptimization = _container.Resolve<IGroupPersonBuilderForOptimization>();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "matrixListForIntradayOptimization"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void ReOptimize(BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays, IList<IScheduleMatrixPro> allMatrixes )
        {
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
            var onlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;
            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
            IList<IPerson> selectedPersons = new List<IPerson>(ScheduleViewBase.AllSelectedPersons(selectedDays));
            IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container);
            IList<IScheduleMatrixPro> matrixListForDayOffOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container);
			IList<IScheduleMatrixPro> matrixListForIntradayOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container);

			//IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization = createMatrixContainerList(matrixListForWorkShiftOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization = createMatrixContainerList(matrixListForDayOffOptimization);
			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization = createMatrixContainerList(matrixListForIntradayOptimization);

            var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            var selectedPeriod = new DateOnlyPeriod(OptimizerHelperHelper.GetStartDateInSelectedDays(selectedDays, currentPersonTimeZone), OptimizerHelperHelper.GetEndDateInSelectedDays(selectedDays, currentPersonTimeZone));


            IGroupPageDataProvider groupPageDataProvider = _container.Resolve<IGroupScheduleGroupPageDataProvider>();
            var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();

			groupPagePerDateHolder.GroupPersonGroupPagePerDate = _container.Resolve<IGroupPageCreator>()
					.CreateGroupPagePerDate(selectedPeriod.DayCollection(), groupPageDataProvider,
					optimizerPreferences.Extra.GroupPageOnTeam);
			
            OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, selectedPeriod, optimizerPreferences.Rescheduling, _container);
            var tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(optimizerPreferences.General.ScheduleTag);

            using (PerformanceOutput.ForOperation("Optimizing " + matrixListForWorkShiftOptimization.Count + " matrixes"))
            {
				var schedulingOptionsCreator = new SchedulingOptionsCreator();
				var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
				var targetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
				var groupPersonsBuilder = _container.Resolve<IGroupPersonsBuilder>();
            	var teamSteadyStateRunner = new TeamSteadyStateRunner(allMatrixes, targetTimeCalculator);
				var teamSteadyStateCreator = new TeamSteadyStateDictionaryCreator(teamSteadyStateRunner, allMatrixes, groupPersonsBuilder, schedulingOptions);
				IDictionary<Guid, bool> teamSteadyStateDictionary = teamSteadyStateCreator.Create(selectedPeriod);
                if (optimizerPreferences.General.OptimizationStepDaysOff)
					runDayOffOptimization(optimizerPreferences, matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod, teamSteadyStateDictionary);
                if (optimizerPreferences.General.OptimizationStepTimeBetweenDays)
					runMoveTimeOptimization(matrixOriginalStateContainerListForIntradayOptimization, optimizerPreferences, teamSteadyStateDictionary);
                if (optimizerPreferences.General.OptimizationStepShiftsWithinDay)
                    runIntradayOptimization(matrixOriginalStateContainerListForIntradayOptimization, optimizerPreferences);

				var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, new EmptyScheduleDayChangeCallback(), tagSetter);
				if (optimizerPreferences.General.OptimizationStepFairness)
					runFairness(selectedPersons, selectedDays, rollbackService, optimizerPreferences);
            }
            if (optimizerPreferences.General.UseShiftCategoryLimitations)
            {
                var schedulingOptionsCreator = new SchedulingOptionsCreator();
                var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
                RemoveShiftCategoryBackToLegalState(matrixListForWorkShiftOptimization, backgroundWorker,
                                                    optimizerPreferences, schedulingOptions, selectedPeriod, allMatrixes);
            }
            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = onlyShiftsWhenUnderstaffed;
        }


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void RemoveShiftCategoryBackToLegalState(
            IList<IScheduleMatrixPro> matrixList,
			BackgroundWorker backgroundWorker, IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions, 
			DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
            using (PerformanceOutput.ForOperation("GroupShiftCategoryLimitations"))
            {
                var backToLegalStateServicePro =
                    _container.Resolve<IGroupListShiftCategoryBackToLegalStateService>();

                if (backgroundWorker.CancellationPending)
                    return;
                var groupOptimizerFindMatrixesForGroup =
                    new GroupOptimizerFindMatrixesForGroup(_groupPersonBuilderForOptimization, matrixList);


				IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
			   OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);
				var groupMatrixContainerCreator = _container.Resolve<IGroupMatrixContainerCreator>();
				var groupPersonConsistentChecker =
					_container.Resolve<IGroupPersonConsistentChecker>();
				var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
				var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
				IGroupMatrixHelper groupMatrixHelper = new GroupMatrixHelper(groupMatrixContainerCreator,
																		 groupPersonConsistentChecker,
																		 workShiftBackToLegalStateService,
																		 resourceOptimizationHelper,
																		 mainShiftOptimizeActivitySpecificationSetter);

		
				var coherentChecker = new TeamSteadyStateCoherentChecker();
				var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
				var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(groupMatrixHelper, coherentChecker, scheduleMatrixProFinder);
				var groupPersonsBuilder = _container.Resolve<IGroupPersonsBuilder>();
				var targetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
            	var teamSteadyStateRunner = new TeamSteadyStateRunner(allMatrixes, targetTimeCalculator);
				var teamSteadyStateCreator = new TeamSteadyStateDictionaryCreator(teamSteadyStateRunner, allMatrixes, groupPersonsBuilder, schedulingOptions);
				IDictionary<Guid, bool> teamSteadyStateDictionary = teamSteadyStateCreator.Create(selectedPeriod);
            	var teamSteadyStateHolder = new TeamSteadyStateHolder(teamSteadyStateDictionary);
				IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization = new GroupPersonBuilderForOptimization(_stateHolder, _container.Resolve<IGroupPersonFactory>(), _container.Resolve<IGroupPagePerDateHolder>());

                backToLegalStateServicePro.Execute(matrixList, schedulingOptions, optimizationPreferences,
                                                   groupOptimizerFindMatrixesForGroup, teamSteadyStateHolder, teamSteadyStateMainShiftScheduler, groupPersonBuilderForOptimization);

            }
        }

		private void runFairness(IList<IPerson> selectedPersons, IList<IScheduleDay> selectedDays, 
			SchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences)
		{
			var matrixListForFairness = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container);
			var fairnessOpt = _container.Resolve<IShiftCategoryFairnessOptimizer>();
			var selectedDates = OptimizerHelperHelper.GetSelectedPeriod(selectedDays).DayCollection();

			fairnessOpt.ReportProgress += resourceOptimizerPersonOptimized;
			fairnessOpt.Execute(_backgroundWorker, selectedPersons, selectedDates, matrixListForFairness, optimizationPreferences.Extra.GroupPageOnTeam, rollbackService);
			fairnessOpt.ReportProgress -= resourceOptimizerPersonOptimized;
			
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void runMoveTimeOptimization(IList<IScheduleMatrixOriginalStateContainer> originalStateContainers, IOptimizationPreferences optimizationPreferences,
			IDictionary<Guid, bool> teamSteadyStateDictionary)
        {
            var schedulingOptionsCreator = new SchedulingOptionsCreator();
            IList<IGroupMoveTimeOptimizer> optimizers = new List<IGroupMoveTimeOptimizer>();
            extractOptimizer(originalStateContainers, optimizationPreferences, optimizers);

            IDeleteSchedulePartService deleteSchedulePartService;
            MainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter;
            IList<IScheduleMatrixPro> allMatrix;
            ISchedulePartModifyAndRollbackService rollbackService;
            IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup;
            var groupPersonBuilderForOptimization = teamOptimizerHelper(originalStateContainers, optimizationPreferences,
                                                                        out deleteSchedulePartService,
                                                                        out mainShiftOptimizeActivitySpecificationSetter,
                                                                        out allMatrix, out rollbackService,
                                                                        out groupOptimizerFindMatrixesForGroup);

            var groupMatrixHelper = new GroupMatrixHelper(_container.Resolve<IGroupMatrixContainerCreator>(),
                                                                         _container.Resolve<IGroupPersonConsistentChecker>(),
                                                                         OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container),
                                                                         _container.Resolve<IResourceOptimizationHelper>(),
                                                                         mainShiftOptimizeActivitySpecificationSetter);
            var groupSchedulingService = _container.Resolve<IGroupSchedulingService>();
            var groupMoveTimeOptimizerExecuter = new GroupMoveTimeOptimizationExecuter(rollbackService,
                                                            deleteSchedulePartService, schedulingOptionsCreator, optimizationPreferences,
                                                            mainShiftOptimizeActivitySpecificationSetter,
                                                            groupMatrixHelper, groupSchedulingService,
                                                            groupPersonBuilderForOptimization, _resourceOptimizationHelper);
            var groupMoveTimeValidatorRunner =
                new GroupMoveTimeValidatorRunner(new GroupOptimizerValidateProposedDatesInSameMatrix(groupOptimizerFindMatrixesForGroup),
                                                 new GroupOptimizerValidateProposedDatesInSameGroup(groupPersonBuilderForOptimization, groupOptimizerFindMatrixesForGroup));
            var service = new GroupMoveTimeOptimizerService(optimizers, groupOptimizerFindMatrixesForGroup, groupMoveTimeOptimizerExecuter, groupMoveTimeValidatorRunner);


			var coherentChecker = new TeamSteadyStateCoherentChecker();
			var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
			var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(groupMatrixHelper, coherentChecker, scheduleMatrixProFinder);
			var teamSteadyStateHolder = new TeamSteadyStateHolder(teamSteadyStateDictionary);

            service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute(allMatrix, teamSteadyStateMainShiftScheduler, teamSteadyStateHolder, _stateHolder.Schedules);
            service.ReportProgress -= resourceOptimizerPersonOptimized;
        }

        private IGroupPersonBuilderForOptimization teamOptimizerHelper(IEnumerable<IScheduleMatrixOriginalStateContainer> originalStateContainers,
                                                                       IOptimizationPreferences optimizationPreferences,
                                                                       out IDeleteSchedulePartService deleteSchedulePartService,
                                                                       out MainShiftOptimizeActivitySpecificationSetter
                                                                           mainShiftOptimizeActivitySpecificationSetter,
                                                                       out IList<IScheduleMatrixPro> allMatrix,
                                                                       out ISchedulePartModifyAndRollbackService rollbackService,
                                                                       out IGroupOptimizerFindMatrixesForGroup
                                                                           groupOptimizerFindMatrixesForGroup)
        {
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
                new GroupPersonBuilderForOptimization(_schedulerState.SchedulingResultState,
                                                      _container.Resolve<IGroupPersonFactory>(),
                                                      _container.Resolve<IGroupPagePerDateHolder>());
            allMatrix = originalStateContainers.Select(container => container.ScheduleMatrix).ToList();
            groupOptimizerFindMatrixesForGroup = new GroupOptimizerFindMatrixesForGroup(groupPersonBuilderForOptimization,
                                                                                        allMatrix);
            rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder,
                                                                       _scheduleDayChangeCallback,
                                                                       new ScheduleTagSetter
                                                                           (optimizationPreferences
                                                                                .General.
                                                                                ScheduleTag));
            deleteSchedulePartService = _container.Resolve<IDeleteSchedulePartService>();
            mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
            return groupPersonBuilderForOptimization;
        }

        private static void extractOptimizer(IEnumerable<IScheduleMatrixOriginalStateContainer> originalStateContainers, IOptimizationPreferences optimizationPreferences, ICollection<IGroupMoveTimeOptimizer> optimizers)
        {
            foreach (var originalStateContainer in originalStateContainers)
            {
                var matrix = originalStateContainer.ScheduleMatrix;

                var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(matrix, new RestrictionChecker(),
                                                                                              optimizationPreferences,
                                                                                              originalStateContainer);
                var dataExtractorProvider = new ScheduleResultDataExtractorProvider(optimizationPreferences.Advanced);
                var personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(matrix);
                IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
                var periodValueCalculator = periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizationPreferences.Advanced, personalSkillsDataExtractor);

                var lockableBitArrayConverter =
                    new ScheduleMatrixLockableBitArrayConverter(matrix);
                
                var optimizer = new GroupMoveTimeOptimizer(periodValueCalculator, lockableBitArrayConverter, new MoveTimeDecisionMaker2(),
                                                           personalSkillsDataExtractor,
                                                           optimizerOverLimitDecider);
                optimizers.Add(optimizer);
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void runIntradayOptimization(IList<IScheduleMatrixOriginalStateContainer> originalStateContainers, IOptimizationPreferences optimizationPreferences)
		{
			var schedulingOptionsCreator = new SchedulingOptionsCreator();
			var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			var restrictionChecker = new RestrictionChecker();
			var decisionMaker = new IntradayDecisionMaker();
			IList<IGroupIntradayOptimizer> optimizers = new List<IGroupIntradayOptimizer>();
			foreach (var originalStateContainer in originalStateContainers)
			{
				var matrix = originalStateContainer.ScheduleMatrix;

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(matrix, restrictionChecker,
				                                                                              optimizationPreferences,
				                                                                              originalStateContainer);
				var relativeDailyStandardDeviationsByAllSkillsExtractor =
					new RelativeDailyStandardDeviationsByAllSkillsExtractor(matrix, schedulingOptions);
				IScheduleMatrixLockableBitArrayConverter lockableBitArrayConverter = new ScheduleMatrixLockableBitArrayConverter(matrix);
				var optimizer = new GroupIntradayOptimizer(lockableBitArrayConverter, decisionMaker,
				                                           relativeDailyStandardDeviationsByAllSkillsExtractor,
				                                           optimizerOverLimitDecider);
				optimizers.Add(optimizer);
			}

			var groupPersonFactory = _container.Resolve<IGroupPersonFactory>();
			var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
        		new GroupPersonBuilderForOptimization(_schedulerState.SchedulingResultState, groupPersonFactory, groupPagePerDateHolder);
			IList<IScheduleMatrixPro> allMatrix = originalStateContainers.Select(container => container.ScheduleMatrix).ToList();
			IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup = new GroupOptimizerFindMatrixesForGroup(groupPersonBuilderForOptimization, allMatrix);
			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder,
			                                                                                                 _scheduleDayChangeCallback,
			                                                                                                 new ScheduleTagSetter
			                                                                                                 	(optimizationPreferences
			                                                                                                 	 	.General.
			                                                                                                 	 	ScheduleTag));
			var deleteSchedulePartService = _container.Resolve<IDeleteSchedulePartService>();
			var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
			var groupMatrixContainerCreator = _container.Resolve<IGroupMatrixContainerCreator>();
			var groupPersonConsistentChecker =
				_container.Resolve<IGroupPersonConsistentChecker>();
			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
			   OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

			IGroupMatrixHelper groupMatrixHelper = new GroupMatrixHelper(groupMatrixContainerCreator,
			                                                             groupPersonConsistentChecker,
			                                                             workShiftBackToLegalStateService,
			                                                             resourceOptimizationHelper, 
																		 mainShiftOptimizeActivitySpecificationSetter);
			var groupSchedulingService = _container.Resolve<IGroupSchedulingService>();
			IGroupIntradayOptimizerExecuter groupIntradayOptimizerExecuter = new GroupIntradayOptimizerExecuter(rollbackService,
			                                                deleteSchedulePartService, schedulingOptionsCreator, optimizationPreferences,
			                                                mainShiftOptimizeActivitySpecificationSetter,
			                                                groupMatrixHelper, groupSchedulingService,
			                                                groupPersonBuilderForOptimization, _resourceOptimizationHelper);
			var service = new GroupIntradayOptimizerService(optimizers, groupOptimizerFindMatrixesForGroup, groupIntradayOptimizerExecuter);

			service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute(allMatrix);
			service.ReportProgress -= resourceOptimizerPersonOptimized;
		}
        private static IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
        {
            IScheduleDayEquator scheduleDayEquator = new ScheduleDayEquator();
            IList<IScheduleMatrixOriginalStateContainer> result =
                matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
                .Cast<IScheduleMatrixOriginalStateContainer>().ToList();
            return result;
        }

		//private void removeShiftCategoryBackToLegalState(
		//    IList<IScheduleMatrixPro> matrixList,
		//    BackgroundWorker backgroundWorker, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences)
		//{
		//    using (PerformanceOutput.ForOperation("ShiftCategoryLimitations"))
		//    {
		//        var backToLegalStateServicePro =
		//            _container.Resolve<ISchedulePeriodListShiftCategoryBackToLegalStateService>();

		//        if (backgroundWorker.CancellationPending)
		//            return;

		//        backToLegalStateServicePro.Execute(matrixList, schedulingOptions, optimizationPreferences);
		//    }
		//}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void runDayOffOptimization(IOptimizationPreferences optimizerPreferences,
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList, DateOnlyPeriod selectedPeriod,
			IDictionary<Guid, bool> teamSteadyStateDictionary)
        {

            if (_backgroundWorker.CancellationPending)
                return;

            IList<IScheduleMatrixPro> matrixList =
                matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

            OptimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, _container);

            IList<IDayOffTemplate> displayList = (from item in _schedulerState.CommonStateHolder.DayOffs
                                                  where ((IDeleteTag)item).IsDeleted == false
                                                  select item).ToList();

            // Do not use DO back to legal when groupscheduling as every agent is treated differently
            //var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots);
            //resourceOptimizerPersonOptimized(this, e);

            //// to make sure we are in legal state before we can do day off optimization
            //((List<IDayOffTemplate>)displayList).Sort(new DayOffTemplateSorter());
            //DaysOffBackToLegalState(matrixOriginalStateContainers, optimizerPreferences, _backgroundWorker,
            //                        displayList[0], false);

            var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.Rescheduling + Resources.ThreeDots);
            resourceOptimizerPersonOptimized(this, e);

            // Schedule White Spots after back to legal state
            var scheduleService = _container.Resolve<IScheduleService>();

			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

            // schedule those are the white spots after back to legal state
            // ??? check if all white spots could be scheduled ??????
			OptimizerHelperHelper.ScheduleBlankSpots(matrixContainerList, scheduleService, _container, rollbackService);

            
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
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, optimizerPreferences.Rescheduling.ConsiderShortBreaks);
                }
            }

            // day off optimization

            optimizeDaysOff(matrixContainerList,
                            optimizerPreferences,
                            displayList[0],
                            selectedPeriod,
                            scheduleService,
							teamSteadyStateDictionary);


            // we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
			//rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			//foreach (IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in matrixContainerList)
			//{
			//    if (!matrixOriginalStateContainer.IsFullyScheduled())
			//        rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService);
			//}
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void optimizeDaysOff(
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
            IOptimizationPreferences optimizerPreferences, 
            IDayOffTemplate dayOffTemplate,
            DateOnlyPeriod selectedPeriod, 
            IScheduleService scheduleService,
			IDictionary<Guid, bool> teamSteadyStateDictionary)
        {
            var rollbackService = _container.Resolve<ISchedulePartModifyAndRollbackService>();
            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict =
                new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));
            IList<IGroupDayOffOptimizerContainer> optimizerContainers = new List<IGroupDayOffOptimizerContainer>();

			IList<IScheduleMatrixPro> allMatrix = matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

        	for (int index = 0; index < matrixContainerList.Count; index++)
            {
                IScheduleMatrixOriginalStateContainer matrixContainer = matrixContainerList[index];
                IScheduleMatrixPro matrix = matrixContainer.ScheduleMatrix;

                IScheduleResultDataExtractor personalSkillsDataExtractor = OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, matrix);
                IPeriodValueCalculator localPeriodValueCalculator = OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);

                IGroupDayOffOptimizerContainer optimizerContainer =
                    createOptimizer(matrixContainer, optimizerPreferences.DaysOff, optimizerPreferences,
                    rollbackService, dayOffTemplate, scheduleService, localPeriodValueCalculator,
					rollbackServiceDayOffConflict, allMatrix, teamSteadyStateDictionary);
                optimizerContainers.Add(optimizerContainer);
            }

            IScheduleResultDataExtractor allSkillsDataExtractor =
                OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder);
            IPeriodValueCalculator periodValueCalculator = OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);
			var groupPersonFactory = _container.Resolve<IGroupPersonFactory>();
			var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
        		new GroupPersonBuilderForOptimization(_schedulerState.SchedulingResultState, groupPersonFactory, groupPagePerDateHolder);
			IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup = new GroupOptimizerFindMatrixesForGroup(groupPersonBuilderForOptimization, allMatrix);
			var service = new GroupDayOffOptimizationService(periodValueCalculator, rollbackService, _resourceOptimizationHelper, groupOptimizerFindMatrixesForGroup);

			service.ReportProgress += resourceOptimizerPersonOptimized;
            service.Execute(optimizerContainers, optimizerPreferences.Extra.KeepSameDaysOffInTeam);
            service.ReportProgress -= resourceOptimizerPersonOptimized;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ResourceOptimizerProgressEventArgs.#ctor(Teleopti.Interfaces.Domain.IPerson,System.Double,System.Double,System.String)")]
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
        private IGroupDayOffOptimizerContainer createOptimizer(
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            IDaysOffPreferences dayOffPreferences,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDayOffTemplate dayOffTemplate,
            IScheduleService scheduleService,
            IPeriodValueCalculator periodValueCalculatorForAllSkills,
            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict,
            IList<IScheduleMatrixPro> allMatrixes,
			IDictionary<Guid, bool> teamSteadyStateDictionary)
        {
            IScheduleMatrixPro scheduleMatrix = originalStateContainer.ScheduleMatrix;

            IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
               OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

            IScheduleMatrixLockableBitArrayConverter scheduleMatrixArrayConverter =
                new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);
			ILockableBitArray scheduleMatrixArray =
				scheduleMatrixArrayConverter.Convert(dayOffPreferences.ConsiderWeekBefore, dayOffPreferences.ConsiderWeekAfter);
            
            // create decisionmakers

            IEnumerable<IDayOffDecisionMaker> decisionMakers =
                OptimizerHelperHelper.CreateDecisionMakers(scheduleMatrixArray, dayOffPreferences, optimizerPreferences);

            IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions = new DayOffBackToLegalStateFunctions();
            ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService
                = new SmartDayOffBackToLegalStateService(dayOffBackToLegalStateFunctions, dayOffPreferences, 25);

            var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, true);
            var dayOffOptimizerConflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrix, scheduleService,
                                                                                    effectiveRestrictionCreator,
                                                                                    rollbackServiceDayOffConflict,
																					resourceCalculateDelayer);

            var dayOffOptimizerValidator = _container.Resolve<IDayOffOptimizerValidator>();
            var resourceCalculateDaysDecider = _container.Resolve<IResourceCalculateDaysDecider>();

            var restrictionChecker = new RestrictionChecker();
            var optimizationPreferences = _container.Resolve<IOptimizationPreferences>();
        	IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider = new ScheduleResultDataExtractorProvider(optimizerPreferences.Advanced);
        	var lockableBitArrayChangesTracker =
        		_container.Resolve<ILockableBitArrayChangesTracker>();
			var groupSchedulingService = _container.Resolve<IGroupSchedulingService>();
        	//var groupMatrixHelper = _container.Resolve<IGroupMatrixHelper>();
        	var groupMatrixContainerCreator = _container.Resolve<IGroupMatrixContainerCreator>();
        	var groupPersonConsistentChecker =
        		_container.Resolve<IGroupPersonConsistentChecker>();
        	var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
        	IGroupMatrixHelper groupMatrixHelper = new GroupMatrixHelper(groupMatrixContainerCreator,
        	                                                             groupPersonConsistentChecker,
        	                                                             workShiftBackToLegalStateService,
        	                                                             resourceOptimizationHelper,
        	                                                             mainShiftOptimizeActivitySpecificationSetter);
			var groupPersonFactory = _container.Resolve<IGroupPersonFactory>();
        	var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();
        	IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
        		new GroupPersonBuilderForOptimization(_schedulerState.SchedulingResultState, groupPersonFactory, groupPagePerDateHolder);
			IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup = new GroupOptimizerFindMatrixesForGroup(groupPersonBuilderForOptimization, allMatrixes);
			IGroupDayOffOptimizerValidateDayOffToRemove groupDayOffOptimizerValidateDayOffToRemove = new GroupDayOffOptimizerValidateDayOffToRemove(groupOptimizerFindMatrixesForGroup);
			IGroupDayOffOptimizerValidateDayOffToAdd groupDayOffOptimizerValidateDayOffToAdd = new GroupDayOffOptimizerValidateDayOffToAdd(groupOptimizerFindMatrixesForGroup);
			IGroupOptimizerValidateProposedDatesInSameMatrix groupOptimizerValidateProposedDatesInSameMatrix = new GroupOptimizerValidateProposedDatesInSameMatrix(groupOptimizerFindMatrixesForGroup);
			IGroupOptimizerValidateProposedDatesInSameGroup groupOptimizerValidateProposedDatesInSameGroup = new GroupOptimizerValidateProposedDatesInSameGroup(groupPersonBuilderForOptimization, groupOptimizerFindMatrixesForGroup);
			IGroupOptimizationValidatorRunner groupOptimizationValidatorRunner = new GroupOptimizationValidatorRunner(groupDayOffOptimizerValidateDayOffToRemove,
				groupDayOffOptimizerValidateDayOffToAdd, groupOptimizerValidateProposedDatesInSameMatrix, groupOptimizerValidateProposedDatesInSameGroup);

        	IGroupDayOffOptimizerCreator groupDayOffOptimizerCreator =
        		new GroupDayOffOptimizerCreator(scheduleResultDataExtractorProvider, lockableBitArrayChangesTracker,
        		                                rollbackService, groupSchedulingService,
												groupMatrixHelper, groupOptimizationValidatorRunner, groupPersonBuilderForOptimization);
            var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrix, restrictionChecker, optimizationPreferences, originalStateContainer);

            var schedulingOptionsCreator = new SchedulingOptionsCreator();
        	var coherentChecker = new TeamSteadyStateCoherentChecker();
        	var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
        	var teamSteadyStateHolder = new TeamSteadyStateHolder(teamSteadyStateDictionary);
        	var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(groupMatrixHelper, coherentChecker, scheduleMatrixProFinder);
			

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
                                                  resourceCalculateDaysDecider,
                                                  dayOffOptimizerValidator,
                                                  dayOffOptimizerConflictHandler, 
                                                  originalStateContainer,
                                                  optimizerOverLimitDecider,
                                                  null,
                                                  schedulingOptionsCreator,
												  mainShiftOptimizeActivitySpecificationSetter,
												  dayOffOptimizerPreMoveResultPredictor
                                                  );

            var optimizerContainer =
                new GroupDayOffOptimizerContainer(scheduleMatrixArrayConverter,
                                             decisionMakers,
                                             optimizerPreferences,
                                             scheduleMatrix,
                                             dayOffDecisionMakerExecuter,
                                             allMatrixes,
                                             groupDayOffOptimizerCreator, 
                                             schedulingOptionsCreator,
											 teamSteadyStateMainShiftScheduler,
											 teamSteadyStateHolder,
											 _stateHolder.Schedules);
            return optimizerContainer;
        }
    }
}