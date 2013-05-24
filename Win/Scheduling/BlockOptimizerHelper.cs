using System;
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
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Commands;
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
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        public BlockOptimizerHelper(ILifetimeScope container, ScheduleOptimizerHelper scheduleOptimizerHelper)
        {
            _container = container;
            _scheduleOptimizerHelper = scheduleOptimizerHelper;
            _stateHolder = _container.Resolve<ISchedulingResultStateHolder>();
            _schedulerStateHolder = _container.Resolve<ISchedulerStateHolder>();
            _scheduleDayChangeCallback = _container.Resolve<IScheduleDayChangeCallback>();
            _allResults = _container.Resolve<IWorkShiftFinderResultHolder>();
            _resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
            _groupPersonBuilderForOptimization = _container.Resolve<IGroupPersonBuilderForOptimization>();
        }

        public ISchedulingResultStateHolder SchedulingStateHolder
        {
            get { return _stateHolder; }
        }

        private void optimizeDaysOff(IList<IScheduleMatrixPro> matrixList,
            IDayOffTemplate dayOffTemplate,
            DateOnlyPeriod selectedPeriod, 
            IScheduleService scheduleService,
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers)
        {
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

            var rollbackService = _container.Resolve<ISchedulePartModifyAndRollbackService>();

            ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict =
                new SchedulePartModifyAndRollbackService(
                    SchedulingStateHolder, 
                    _scheduleDayChangeCallback, new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

            IPeriodValueCalculator periodValueCalculator = null;
            IList<IBlockDayOffOptimizerContainer> optimizerContainers = new List<IBlockDayOffOptimizerContainer>();

            for (int index = 0; index < matrixList.Count; index++)
            {

                IScheduleMatrixPro matrix = matrixList[index];
                IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer = matrixOriginalStateContainers[index];

                IScheduleResultDataExtractor personalSkillsDataExtractor =
                    OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, matrix);
                periodValueCalculator = 
                    OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);
                IBlockDayOffOptimizerContainer optimizerContainer =
                    createOptimizer(matrix, optimizerPreferences,
                    rollbackService, dayOffTemplate, scheduleService, periodValueCalculator,
                    rollbackServiceDayOffConflict, matrixOriginalStateContainer);
                optimizerContainers.Add(optimizerContainer);
            }

            IScheduleResultDataExtractor allSkillsDataExtractor =
                OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, SchedulingStateHolder);
            OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);
        	ISchedulingOptions schedulingOptions =
        		new SchedulingOptionsCreator().CreateSchedulingOptions(optimizerPreferences);
            var service = new BlockDayOffOptimizationService(periodValueCalculator, rollbackService, optimizerPreferences.DaysOff);
            service.ReportProgress += resourceOptimizer_PersonOptimized;
            service.Execute(optimizerContainers, schedulingOptions);
            service.ReportProgress -= resourceOptimizer_PersonOptimized;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void daysOffBackToLegalState(IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
                                    BackgroundWorker backgroundWorker,
                                    IDayOffTemplate dayOffTemplate,
                                    bool reschedule)
        {
            _allResults = new WorkShiftFinderResultHolder();
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

            IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers =
                OptimizerHelperHelper.CreateSmartDayOffSolverContainers(matrixOriginalStateContainers, optimizerPreferences.DaysOff);

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
					{
						OptimizerHelperHelper.SyncSmartDayOffContainerWithMatrix(
							backToLegalStateSolverContainer,
							dayOffTemplate,
							optimizerPreferences.DaysOff,
							_scheduleDayChangeCallback,
							new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

						var restrictionChecker = new RestrictionChecker();
						var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
						var originalStateContainer = backToLegalStateSolverContainer.MatrixOriginalStateContainer;
						var optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(matrix,
																													  restrictionChecker,
																													  optimizerPreferences,
																													  originalStateContainer);
						if (optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit() || optimizationOverLimitByRestrictionDecider.OverLimit().Count > 0)
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

        

        public void ReOptimize(BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays, ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes )
        {
            _backgroundWorker = backgroundWorker;
            var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
            var originalOnlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;
            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;

			var currentPersonTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
	        var selectedPeriod =
		        new DateOnlyPeriod(OptimizerHelperHelper.GetStartDateInSelectedDays(selectedDays, currentPersonTimeZone),
		                           OptimizerHelperHelper.GetEndDateInSelectedDays(selectedDays, currentPersonTimeZone));
			IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization = _container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);
			IList<IScheduleMatrixPro> matrixListForDayOffOptimization = _container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);
			IList<IScheduleMatrixPro> matrixListForIntradayOptimization = _container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);

            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization = createMatrixContainerList(matrixListForWorkShiftOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization = createMatrixContainerList(matrixListForDayOffOptimization);
            IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization = createMatrixContainerList(matrixListForIntradayOptimization);

            

            OptimizerHelperHelper.SetConsiderShortBreaks(
                ScheduleViewBase.AllSelectedPersons(selectedDays), 
                selectedPeriod, 
                optimizerPreferences.Rescheduling, 
                _container);

            var tagSetter = _container.Resolve<IScheduleTagSetter>();
            tagSetter.ChangeTagToSet(optimizerPreferences.General.ScheduleTag);

            using (PerformanceOutput.ForOperation("Optimizing " + matrixListForWorkShiftOptimization.Count + " matrixes"))
            {
                if (optimizerPreferences.General.OptimizationStepDaysOff)
                    runDayOffOptimization(optimizerPreferences, matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod);

                var originalKeepShiftCategories = optimizerPreferences.Shifts.KeepShiftCategories;
                optimizerPreferences.Shifts.KeepShiftCategories = true;

				IList<IScheduleMatrixPro> matrixListForWorkShiftAndIntradayOptimization = _container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);
                IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization =
                    createMatrixContainerList(matrixListForWorkShiftAndIntradayOptimization);

                if (optimizerPreferences.General.OptimizationStepTimeBetweenDays)
                    _scheduleOptimizerHelper.RunWorkShiftOptimization(
                        optimizerPreferences, 
                        matrixOriginalStateContainerListForWorkShiftOptimization, 
                        workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
                        selectedPeriod, 
                        _backgroundWorker);

                if (optimizerPreferences.General.OptimizationStepShiftsWithinDay)
                    _scheduleOptimizerHelper.RunIntradayOptimization(
                        optimizerPreferences, 
                        matrixOriginalStateContainerListForIntradayOptimization, 
                        workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
                        backgroundWorker);

                optimizerPreferences.Shifts.KeepShiftCategories = originalKeepShiftCategories;
            }


            if (optimizerPreferences.General.UseShiftCategoryLimitations)
            {
                removeShiftCategoryBackToLegalState(matrixListForWorkShiftOptimization, backgroundWorker, schedulingOptions, optimizerPreferences, selectedPeriod, allMatrixes);
            }
            //set back
            optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = originalOnlyShiftsWhenUnderstaffed;
        }

        private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
        {
			IScheduleDayEquator scheduleDayEquator = _container.Resolve<IScheduleDayEquator>();
            IList<IScheduleMatrixOriginalStateContainer> result =
                matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
                .Cast<IScheduleMatrixOriginalStateContainer>().ToList();
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void runDayOffOptimization(IOptimizationPreferences optimizerPreferences,
            IList<IScheduleMatrixOriginalStateContainer> matrixContainerList, DateOnlyPeriod selectedPeriod)
        {

            if (_backgroundWorker.CancellationPending)
                return;

            IList<IScheduleMatrixPro> matrixList =
               matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

            OptimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, _container);

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
            // bug 18271 this one does not know about blocks and we don't need it because the white spots will be scheduled later, i hope
            //OptimizerHelperHelper.ScheduleBlankSpots(optimizerPreferences.SchedulingOptions, matrixContainerList, scheduleService, _container);

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
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, optimizerPreferences.Rescheduling.ConsiderShortBreaks);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void removeShiftCategoryBackToLegalState(
            IList<IScheduleMatrixPro> matrixList,
			BackgroundWorker backgroundWorker, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
            using (PerformanceOutput.ForOperation("ShiftCategoryLimitations"))
            {
                if (schedulingOptions.UseGroupScheduling)
                {
                    var backToLegalStateServicePro =
                    _container.Resolve<IGroupListShiftCategoryBackToLegalStateService>();

                    if (backgroundWorker.CancellationPending)
                        return;
                    var groupOptimizerFindMatrixesForGroup =
                        new GroupOptimizerFindMatrixesForGroup(_groupPersonBuilderForOptimization, matrixList);

					var coherentChecker = new TeamSteadyStateCoherentChecker();
					var scheduleMatrixProFinder = new TeamSteadyStateScheduleMatrixProFinder();
					var teamSteadyStateMainShiftScheduler = new TeamSteadyStateMainShiftScheduler(coherentChecker, scheduleMatrixProFinder, _resourceOptimizationHelper, new EditorShiftMapper());
					var groupPersonsBuilder = _container.Resolve<IGroupPersonsBuilder>();
					var targetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
                	var teamSteadyStateRunner = new TeamSteadyStateRunner(allMatrixes, targetTimeCalculator);
					var teamSteadyStateCreator = new TeamSteadyStateDictionaryCreator(teamSteadyStateRunner, allMatrixes, groupPersonsBuilder, schedulingOptions);
					var teamSteadyStateDictionary = teamSteadyStateCreator.Create(selectedPeriod);
                	var teamSteadyStateHolder = new TeamSteadyStateHolder(teamSteadyStateDictionary);
					IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization = new GroupPersonBuilderForOptimization(_schedulerStateHolder.SchedulingResultState, _container.Resolve<IGroupPersonFactory>(), _container.Resolve<IGroupPagePerDateHolder>());

					backToLegalStateServicePro.Execute(matrixList, schedulingOptions, optimizationPreferences, groupOptimizerFindMatrixesForGroup, teamSteadyStateHolder, teamSteadyStateMainShiftScheduler, groupPersonBuilderForOptimization);
                }
                else
                {
                    var backToLegalStateServicePro =
                    _container.Resolve<ISchedulePeriodListShiftCategoryBackToLegalStateService>();

                    if (backgroundWorker.CancellationPending)
                        return;

                    backToLegalStateServicePro.Execute(matrixList, schedulingOptions, optimizationPreferences);
                }
            }
        }

        #region Local

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private IBlockDayOffOptimizerContainer createOptimizer(
            IScheduleMatrixPro scheduleMatrix,
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

            IScheduleMatrixLockableBitArrayConverter scheduleMatrixArrayConverter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);
            IDaysOffPreferences daysOffPreferences = optimizerPreferences.DaysOff;
            ILockableBitArray scheduleMatrixArray = scheduleMatrixArrayConverter.Convert(daysOffPreferences.ConsiderWeekBefore, daysOffPreferences.ConsiderWeekAfter);

            IEnumerable<IDayOffDecisionMaker> decisionMakers =
                OptimizerHelperHelper.CreateDecisionMakers(scheduleMatrixArray, optimizerPreferences, _container);
            IScheduleResultDataExtractor scheduleResultDataExtractor =
                OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, scheduleMatrix);

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
            var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();

            var restrictionChecker = new RestrictionChecker();
            var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrix, restrictionChecker, optimizerPreferences, originalStateContainer);

            var schedulingOptionsSyncronizer = new SchedulingOptionsCreator();
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
                                                  null, 
                                                  schedulingOptionsSyncronizer,
												  mainShiftOptimizeActivitySpecificationSetter,
                                                  dayOffOptimizerPreMoveResultPredictor);

            var blockSchedulingService = _container.Resolve<IBlockSchedulingService>();
            var blockCleaner = _container.Resolve<IBlockOptimizerBlockCleaner>();
            var blockDayOffOptimizer = new BlockDayOffOptimizer(scheduleMatrixArrayConverter,
                                                                scheduleResultDataExtractor,
                                                                optimizerPreferences.DaysOff,
                                                                dayOffDecisionMakerExecuter, blockSchedulingService,
                                                                blockCleaner, new LockableBitArrayChangesTracker(),
                                                                resourceOptimizationHelper);

            IBlockDayOffOptimizerContainer optimizerContainer =
                new BlockDayOffOptimizerContainer(
                    decisionMakers,
                    scheduleMatrix,
                    originalStateContainer,
                    blockDayOffOptimizer);
            return optimizerContainer;
        }

        #endregion

    }
}
