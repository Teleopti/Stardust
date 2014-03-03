using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Channels;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public void TeamGroupReOptimize(BackgroundWorker backgroundWorker, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences)
		{
			_backgroundWorker = backgroundWorker;

			IDictionary<IPerson, IScheduleRange> allSelectedScheduleRangeClones = new Dictionary<IPerson, IScheduleRange>();
			foreach (var selectedPerson in selectedPersons)
			{
				allSelectedScheduleRangeClones.Add(selectedPerson, _schedulerState.Schedules[selectedPerson]);
			}
			IMaxMovedDaysOverLimitValidator maxMovedDaysOverLimitValidator =
				new MaxMovedDaysOverLimitValidator(allSelectedScheduleRangeClones, _container.Resolve<IScheduleDayEquator>());
			ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator = new TeamBlockRestrictionOverLimitValidator(
				_container.Resolve<IRestrictionOverLimitDecider>(), maxMovedDaysOverLimitValidator);

			if(optimizationPreferences.General.OptimizationStepDaysOff)
				optimizeTeamBlockDaysOff(selectedPeriod, selectedPersons, optimizationPreferences, teamBlockRestrictionOverLimitValidator);
		    if (optimizationPreferences.General.OptimizationStepShiftsWithinDay)
				optimizeTeamBlockIntraday(selectedPeriod, selectedPersons, optimizationPreferences, teamBlockRestrictionOverLimitValidator);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "matrixListForIntradayOptimization"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void ReOptimize(BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays, IList<IScheduleMatrixPro> allMatrixes )
        {
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
            var onlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;
            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
            IList<IPerson> selectedPersons = new List<IPerson>(ScheduleViewBase.AllSelectedPersons(selectedDays));
			var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
			var selectedPeriod = new DateOnlyPeriod(OptimizerHelperHelper.GetStartDateInSelectedDays(selectedDays, currentPersonTimeZone), OptimizerHelperHelper.GetEndDateInSelectedDays(selectedDays, currentPersonTimeZone));
			IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container, selectedPeriod);
			IList<IScheduleMatrixPro> matrixListForDayOffOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container, selectedPeriod);
			IList<IScheduleMatrixPro> matrixListForIntradayOptimization = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container, selectedPeriod);

			//IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization = createMatrixContainerList(matrixListForWorkShiftOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization = createMatrixContainerList(matrixListForDayOffOptimization);
			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization = createMatrixContainerList(matrixListForIntradayOptimization);

			var openPeriod = _schedulerState.RequestedPeriod.DateOnlyPeriod;

            IGroupPageDataProvider groupPageDataProvider = _container.Resolve<IGroupScheduleGroupPageDataProvider>();
            var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();

			groupPagePerDateHolder.GroupPersonGroupPagePerDate = _container.Resolve<IGroupPageCreator>()
					.CreateGroupPagePerDate(openPeriod.DayCollection(), groupPageDataProvider,
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

				var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
				var coherentChecker = new TeamSteadyStateCoherentChecker();
				var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
				var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(coherentChecker, scheduleMatrixProFinder, resourceOptimizationHelper);
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
			var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedDays);

			var matrixListForFairness = OptimizerHelperHelper.CreateMatrixList(selectedDays, _stateHolder, _container, selectedPeriod);
			var fairnessOpt = _container.Resolve<IShiftCategoryFairnessOptimizer>();
			
			fairnessOpt.ReportProgress += resourceOptimizerPersonOptimized;
			fairnessOpt.Execute(_backgroundWorker, selectedPersons, selectedPeriod.DayCollection(), matrixListForFairness, optimizationPreferences, 
				rollbackService, optimizationPreferences.Advanced.UseAverageShiftLengths);
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
			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
            var groupPersonBuilderForOptimization = teamOptimizerHelper(originalStateContainers, optimizationPreferences,
                                                                        out deleteSchedulePartService,
                                                                        out mainShiftOptimizeActivitySpecificationSetter,
                                                                        out allMatrix, out rollbackService,
                                                                        out groupOptimizerFindMatrixesForGroup);

            var groupMatrixHelper = new GroupMatrixHelper(_container.Resolve<IGroupMatrixContainerCreator>(),
                                                                         _container.Resolve<IGroupPersonConsistentChecker>(),
                                                                         OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container),
                                                                         resourceOptimizationHelper,
                                                                         mainShiftOptimizeActivitySpecificationSetter);
            var groupSchedulingService = _container.Resolve<IGroupSchedulingService>();
			var groupMoveTimeResourceHelper = new GroupMoveTimeOptimizationResourceHelper(_resourceOptimizationHelper);
            var groupMoveTimeOptimizerExecuter = new GroupMoveTimeOptimizationExecuter(rollbackService,
                                                            deleteSchedulePartService, schedulingOptionsCreator, optimizationPreferences,
                                                            mainShiftOptimizeActivitySpecificationSetter,
                                                            groupMatrixHelper, groupSchedulingService,
															groupPersonBuilderForOptimization, groupMoveTimeResourceHelper);
            var groupMoveTimeValidatorRunner =
                new GroupMoveTimeValidatorRunner(new GroupOptimizerValidateProposedDatesInSameMatrix(groupOptimizerFindMatrixesForGroup),
                                                 new GroupOptimizerValidateProposedDatesInSameGroup(groupPersonBuilderForOptimization, groupOptimizerFindMatrixesForGroup));
            var service = new GroupMoveTimeOptimizerService(optimizers, groupOptimizerFindMatrixesForGroup, groupMoveTimeOptimizerExecuter, groupMoveTimeValidatorRunner);


			var coherentChecker = new TeamSteadyStateCoherentChecker();
			var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
			var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(coherentChecker, scheduleMatrixProFinder, resourceOptimizationHelper);
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
                var dataExtractorProvider = new ScheduleResultDataExtractorProvider();
				var personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);
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
                var relativeDailyValueByPersonalSkillsExtractor = new RelativeDailyValueByPersonalSkillsExtractor(matrix,
                                                                                                          optimizationPreferences
                                                                                                              .Advanced,
																	_container.Resolve<ISkillStaffPeriodToSkillIntervalDataMapper>(),
																	 _container.Resolve<ISkillIntervalDataDivider>(),
																	 _container.Resolve<ISkillIntervalDataAggregator>());
                
                 var optimizer = new GroupIntradayOptimizer(lockableBitArrayConverter, decisionMaker,
                                                           relativeDailyStandardDeviationsByAllSkillsExtractor,
                                                           optimizerOverLimitDecider, relativeDailyValueByPersonalSkillsExtractor);
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
            var deleteAndResourceCalculateService = _container.Resolve<IDeleteAndResourceCalculateService >();
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
                                                            deleteAndResourceCalculateService, schedulingOptionsCreator, optimizationPreferences,
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

            OptimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, _container, selectedPeriod);

            IList<IDayOffTemplate> displayList = (from item in _schedulerState.CommonStateHolder.DayOffs
                                                  where ((IDeleteTag)item).IsDeleted == false
                                                  select item).ToList();

            var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.Rescheduling + Resources.ThreeDots);
            resourceOptimizerPersonOptimized(this, e);

            // Schedule White Spots after back to legal state
            var scheduleService = _container.Resolve<IScheduleService>();

			//ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			//// schedule those are the white spots after back to legal state
			//// ??? check if all white spots could be scheduled ??????
			//OptimizerHelperHelper.ScheduleBlankSpots(matrixContainerList, scheduleService, _container, rollbackService);

            
			//bool found = false;
			//foreach (IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in matrixContainerList)
			//{
			//    if (!matrixOriginalStateContainer.IsFullyScheduled())
			//    {
			//        found = true;
			//        rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService);
			//    }
			//}

			//if (found)
			//{
			//    foreach (var dateOnly in selectedPeriod.DayCollection())
			//    {
			//        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, optimizerPreferences.Rescheduling.ConsiderShortBreaks);
			//    }
			//}

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

		private IGroupPersonBuilderForOptimization callGroupPage(ISchedulingOptions schedulingOptions)
		{
			IGroupPageDataProvider groupPageDataProvider = _container.Resolve<IGroupScheduleGroupPageDataProvider>();
			var groupPagePerDateHolder = _container.Resolve<IGroupPagePerDateHolder>();
			if (_schedulerState.LoadedPeriod != null)
			{
				IList<DateOnly> dates =
					_schedulerState.LoadedPeriod.Value.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
						DayCollection();
				groupPagePerDateHolder.GroupPersonGroupPagePerDate =
					_container.Resolve<IGroupPageCreator>().CreateGroupPagePerDate(dates,
																				   groupPageDataProvider,
																				   schedulingOptions.GroupOnGroupPageForTeamBlockPer,
																				   true);
			}
			IGroupPersonFactory groupPersonFactory = new GroupPersonFactory();
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
				new GroupPersonBuilderForOptimization(_schedulerState.SchedulingResultState, groupPersonFactory,
													  groupPagePerDateHolder);
			return groupPersonBuilderForOptimization;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void optimizeTeamBlockDaysOff(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			var allMatrixes = OptimizerHelperHelper.CreateMatrixListAll(_schedulerState, _container, selectedPeriod);  //this one handles userlocks as well
			OptimizerHelperHelper.LockDaysForDayOffOptimization(allMatrixes, _container, selectedPeriod);

			var schedulingOptionsCreator = new SchedulingOptionsCreator();
			var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_container.Resolve<IResourceOptimizationHelper>(), 1, true,
																		schedulingOptions.ConsiderShortBreaks);
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
				new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback,
														 new ScheduleTagSetter(
															 schedulingOptions.TagToUseOnScheduling));
			var teamScheduling = new TeamScheduling(resourceCalculateDelayer, schedulePartModifyAndRollbackService);
			var teamBlockCleaner = _container.Resolve<ITeamBlockClearer>();
			ITeamBlockScheduler teamBlockScheduler =
				new TeamBlockScheduler(_container.Resolve<ISkillDayPeriodIntervalDataGenerator>(),
									   _container.Resolve<IRestrictionAggregator>(),
									   _container.Resolve<IWorkShiftFilterService>(), 
									   teamScheduling,
									   _container.Resolve<IWorkShiftSelector>(),
									   _container.Resolve<IOpenHoursToEffectiveRestrictionConverter>(),
									   teamBlockCleaner,
									   schedulePartModifyAndRollbackService);

			ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService
				= new SmartDayOffBackToLegalStateService(
					_container.Resolve<IDayOffBackToLegalStateFunctions>(), 
					optimizationPreferences.DaysOff,
					100, 
					_container.Resolve<IDayOffDecisionMaker>());

			var groupPersonBuilderForOptimization = callGroupPage(schedulingOptions);
			ITeamInfoFactory teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);

			IScheduleResultDataExtractor allSkillsDataExtractor =
				OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod, _stateHolder);
			IPeriodValueCalculator periodValueCalculatorForAllSkills =
				OptimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, allSkillsDataExtractor);
			

			ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService = 
				new TeamBlockDayOffOptimizerService(
					teamInfoFactory,
					_container.Resolve<ILockableBitArrayFactory>(),
					_container.Resolve<IScheduleResultDataExtractorProvider>(),
					dayOffBackToLegalStateService,
					_container.Resolve<ISchedulingOptionsCreator>(),
					_container.Resolve<ILockableBitArrayChangesTracker>(),
					teamBlockScheduler,
					_container.Resolve<ITeamBlockInfoFactory>(),
					periodValueCalculatorForAllSkills,
					_container.Resolve<IDayOffOptimizationDecisionMakerFactory>(),
					_container.Resolve<ISafeRollbackAndResourceCalculation>(),
					_container.Resolve<ITeamDayOffModifier>(),
					_container.Resolve<IBlockSteadyStateValidator>(),
					teamBlockCleaner,
					teamBlockRestrictionOverLimitValidator
					);

			IList<IDayOffTemplate> dayOffTemplates = (from item in _schedulerState.CommonStateHolder.DayOffs
												  where ((IDeleteTag)item).IsDeleted == false
												  select item).ToList();

			((List<IDayOffTemplate>)dayOffTemplates).Sort(new DayOffTemplateSorter());

			teamBlockDayOffOptimizerService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockDayOffOptimizerService.OptimizeDaysOff(
				allMatrixes, 
				selectedPeriod, 
				selectedPersons, 
				optimizationPreferences,
				schedulePartModifyAndRollbackService,
				dayOffTemplates[0]);
			teamBlockDayOffOptimizerService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void optimizeTeamBlockIntraday(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
        {
            var allMatrixes = OptimizerHelperHelper.CreateMatrixListAll(_schedulerState, _container, selectedPeriod);
		
            var schedulingOptionsCreator = new SchedulingOptionsCreator();
            var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

            var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
                                                                        schedulingOptions.ConsiderShortBreaks);
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
                new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback,
                                                         new ScheduleTagSetter(
                                                             schedulingOptions.TagToUseOnScheduling));
            var teamScheduling = new TeamScheduling(resourceCalculateDelayer, schedulePartModifyAndRollbackService);
			var teamBlockCleaner = _container.Resolve<ITeamBlockClearer>();
            ITeamBlockScheduler teamBlockScheduler =
                new TeamBlockScheduler(_container.Resolve<ISkillDayPeriodIntervalDataGenerator>(),
                                       _container.Resolve<IRestrictionAggregator>(),
                                       _container.Resolve<IWorkShiftFilterService>(), teamScheduling,
                                       _container.Resolve<IWorkShiftSelector>(),
									   _container.Resolve<IOpenHoursToEffectiveRestrictionConverter>(),
									   teamBlockCleaner,
									   schedulePartModifyAndRollbackService);
    
            var groupPersonBuilderForOptimization = callGroupPage(schedulingOptions);
            var teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _container.Resolve<ITeamBlockInfoFactory>());

            ITeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService =
                new TeamBlockIntradayOptimizationService(
                    teamBlockGenerator,
                    teamBlockScheduler,
                    _container.Resolve<ISchedulingOptionsCreator>(),
					_container.Resolve<ISafeRollbackAndResourceCalculation>(),
					_container.Resolve<ITeamBlockIntradayDecisionMaker>(),
					teamBlockRestrictionOverLimitValidator,
					teamBlockCleaner,
					_container.Resolve<IStandardDeviationSumCalculator>()
                    );

	        teamBlockIntradayOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
            teamBlockIntradayOptimizationService.Optimize(
                allMatrixes,
                selectedPeriod,
                selectedPersons,
                optimizationPreferences,
				schedulePartModifyAndRollbackService
                );
	        teamBlockIntradayOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
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
			IGroupDayOffOptimizationResourceHelper groupDayOffOptimizationResourceHelper = new GroupDayOffOptimizationResourceHelper(_resourceOptimizationHelper);
			var service = new GroupDayOffOptimizationService(periodValueCalculator, rollbackService, groupOptimizerFindMatrixesForGroup, optimizerPreferences.DaysOff, groupDayOffOptimizationResourceHelper);

			service.ReportProgress += resourceOptimizerPersonOptimized;
            service.Execute(optimizerContainers, optimizerPreferences.Extra.KeepSameDaysOffInTeam);
            service.ReportProgress -= resourceOptimizerPersonOptimized;
        }

		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ResourceOptimizerProgressEventArgs.#ctor(Teleopti.Interfaces.Domain.IPerson,System.Double,System.Double,System.String)")]
		//private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService)
		//{
		//    var e = new ResourceOptimizerProgressEventArgs(null, 0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name);
		//    resourceOptimizerPersonOptimized(this, e);

		//    rollbackService.ClearModificationCollection();
		//    foreach (IScheduleDayPro scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
		//    {
		//        IScheduleDay originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
		//        rollbackService.Modify(originalPart);
		//    }
		//}

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
                OptimizerHelperHelper.CreateDecisionMakers(scheduleMatrixArray, optimizerPreferences, _container);

            IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions = new DayOffBackToLegalStateFunctions();
			IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(new OfficialWeekendDays(), new TrueFalseRandomizer());
            ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService
                = new SmartDayOffBackToLegalStateService(dayOffBackToLegalStateFunctions, dayOffPreferences, 25, cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);

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
	        var scheduleResultDataExtractorProvider = _container.Resolve<IScheduleResultDataExtractorProvider>();
        	var lockableBitArrayChangesTracker = _container.Resolve<ILockableBitArrayChangesTracker>();
			var groupSchedulingService = _container.Resolve<IGroupSchedulingService>();
        	var groupMatrixContainerCreator = _container.Resolve<IGroupMatrixContainerCreator>();
        	var groupPersonConsistentChecker = _container.Resolve<IGroupPersonConsistentChecker>();
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
        	var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(coherentChecker, scheduleMatrixProFinder, resourceOptimizationHelper);
			

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