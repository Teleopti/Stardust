using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.ClassicLegacy;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleOptimizerHelper
	{
		private Func<IWorkShiftFinderResultHolder> _allResults;
		private ISchedulingProgress _backgroundWorker;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly MoveTimeOptimizerCreator _moveTimeOptimizerCreator;
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		private readonly IEqualNumberOfCategoryFairnessService _equalNumberOfCategoryFairnessService;
		private readonly OptimizeIntradayIslandsDesktop _optimizeIntradayDesktop;
		private readonly ExtendReduceTimeHelper _extendReduceTimeHelper;
		private readonly ExtendReduceDaysOffHelper _extendReduceDaysOffHelper;
		private readonly Func<ISchedulingResultStateHolder> _stateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private ResourceOptimizerProgressEventArgs _progressEvent;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IDayOffOptimizationDesktop _dayOffOptimizationDesktop;
		private readonly DaysOffBackToLegalState _daysOffBackToLegalState;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ITeamBlockDayOffFairnessOptimizationServiceFacade _teamBlockDayOffFairnessOptimizationServiceFacade;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly ITeamBlockSeniorityFairnessOptimizationService _teamBlockSeniorityFairnessOptimizationService;
		private readonly IntraIntervalOptimizationCommand _intraIntervalOptimizationCommand;
		private readonly MaxSeatOptimization _maxSeatOptimization;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;

		public ScheduleOptimizerHelper(MatrixListFactory matrixListFactory,
				MoveTimeOptimizerCreator moveTimeOptimizerCreator,
				RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak,
				IEqualNumberOfCategoryFairnessService equalNumberOfCategoryFairnessService,
				OptimizeIntradayIslandsDesktop optimizeIntradayIslandsDesktop,
				Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
				ExtendReduceTimeHelper extendReduceTimeHelper,
				ExtendReduceDaysOffHelper extendReduceDaysOffHelper,
				Func<ISchedulerStateHolder> schedulerStateHolder,
				Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
				IResourceCalculation resourceOptimizationHelper,
				IOptimizerHelperHelper optimizerHelperHelper,
				CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory,
				IDayOffOptimizationDesktop dayOffOptimizationDesktop,
				DaysOffBackToLegalState daysOffBackToLegalState,
				IUserTimeZone userTimeZone,
				ITeamBlockDayOffFairnessOptimizationServiceFacade teamBlockDayOffFairnessOptimizationServiceFacade,
				IScheduleDayEquator scheduleDayEquator,
				ITeamBlockSeniorityFairnessOptimizationService teamBlockSeniorityFairnessOptimizationService,
				IntraIntervalOptimizationCommand intraIntervalOptimizationCommand,
				MaxSeatOptimization maxSeatOptimization,
				IWeeklyRestSolverCommand weeklyRestSolverCommand)
		{
			_matrixListFactory = matrixListFactory;
			_moveTimeOptimizerCreator = moveTimeOptimizerCreator;
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
			_equalNumberOfCategoryFairnessService = equalNumberOfCategoryFairnessService;
			_optimizeIntradayDesktop = optimizeIntradayIslandsDesktop;
			_allResults = workShiftFinderResultHolder;
			_extendReduceTimeHelper = extendReduceTimeHelper;
			_extendReduceDaysOffHelper = extendReduceDaysOffHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_optimizerHelperHelper = optimizerHelperHelper;
			_resourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_dayOffOptimizationDesktop = dayOffOptimizationDesktop;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_userTimeZone = userTimeZone;
			_teamBlockDayOffFairnessOptimizationServiceFacade = teamBlockDayOffFairnessOptimizationServiceFacade;
			_scheduleDayEquator = scheduleDayEquator;
			_teamBlockSeniorityFairnessOptimizationService = teamBlockSeniorityFairnessOptimizationService;
			_intraIntervalOptimizationCommand = intraIntervalOptimizationCommand;
			_maxSeatOptimization = maxSeatOptimization;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_stateHolder = () => _schedulerStateHolder().SchedulingResultState;
		}

		private void optimizeWorkShifts(
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerList,
			IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			IScheduleResultDataExtractor allSkillsDataExtractor =
				_optimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder());
			IPeriodValueCalculator periodValueCalculator =
				_optimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

			var optimizers = _moveTimeOptimizerCreator.Create(scheduleMatrixOriginalStateContainerList,
					workShiftOriginalStateContainerList,
					optimizerPreferences,
					dayOffOptimizationPreferenceProvider,
					rollbackService);
			var service = new MoveTimeOptimizerContainer(optimizers, periodValueCalculator);

			service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute();
			service.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		public IWorkShiftFinderResultHolder WorkShiftFinderResultHolder => _allResults();

		public void ResetWorkShiftFinderResults()
		{
			_allResults().Clear();
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveBackToLegalStateGui_44333)]
		public void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			ISchedulingProgress backgroundWorker,
			IDayOffTemplate dayOffTemplate,
			SchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IOptimizationPreferences optimizationPreferences)
		{
			if (schedulingOptions == null) throw new ArgumentNullException(nameof(schedulingOptions));

			_allResults = () => new WorkShiftFinderResultHolder();
			_backgroundWorker = backgroundWorker;
			_daysOffBackToLegalState.Execute(matrixOriginalStateContainers, _backgroundWorker, dayOffTemplate,schedulingOptions, dayOffOptimizationPreferenceProvider, optimizationPreferences, _allResults, resourceOptimizerPersonOptimized);
		}

		public void ReOptimize(ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			SchedulingOptions schedulingOptions, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, 
			IOptimizationPreferences optimizationPreferences, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulePartModifyAndRollbackService rollbackService)
		{
			_backgroundWorker = backgroundWorker;
			_progressEvent = null;
			var onlyShiftsWhenUnderstaffed = optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;

			optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
			var tagSetter = new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag);

			optimizationPreferences.Rescheduling.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedAgents, selectedPeriod);
		
			var continuedStep = false;

			if (optimizationPreferences.General.OptimizationStepDaysOff)
			{
				runDayOffOptimization(optimizationPreferences, selectedAgents, selectedPeriod, dayOffOptimizationPreferenceProvider);

				continuedStep = true;
			}

#pragma warning disable 618
			using (_resourceCalculationContextFactory.Create(_stateHolder().Schedules, _stateHolder().Skills, false))
#pragma warning restore 618
			{
				var matrixListForWorkShiftAndIntradayOptimization = _matrixListFactory.CreateMatrixListForSelection(_stateHolder().Schedules, selectedAgents, selectedPeriod);

				if (optimizationPreferences.General.OptimizationStepTimeBetweenDays)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						var matrixListForWorkShiftOptimization =
							_matrixListFactory.CreateMatrixListForSelection(_stateHolder().Schedules, selectedAgents, selectedPeriod);
						IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization =
							createMatrixContainerList(matrixListForWorkShiftOptimization);

						runWorkShiftOptimization(
							optimizationPreferences,
							matrixOriginalStateContainerListForWorkShiftOptimization,
							createMatrixContainerList(matrixListForWorkShiftAndIntradayOptimization),
							selectedPeriod,
							_backgroundWorker,
							dayOffOptimizationPreferenceProvider);
						continuedStep = true;
					}
				}

				continuedStep = runFlexibleTime(optimizationPreferences, continuedStep, selectedPeriod, selectedAgents,
					dayOffOptimizationPreferenceProvider, _matrixListFactory.CreateMatrixListForSelection(_stateHolder().Schedules, selectedAgents, selectedPeriod));
			}

			if (optimizationPreferences.General.OptimizationStepShiftsWithinDay)
			{
				recalculateIfContinuedStep(continuedStep, selectedPeriod);

				if (_progressEvent == null || !_progressEvent.Cancel)
				{
					runIntradayOptimization(
						optimizationPreferences,
						selectedAgents,
						backgroundWorker,
						selectedPeriod);
					continuedStep = true;
				}
			}

#pragma warning disable 618
			using (_resourceCalculationContextFactory.Create(_stateHolder().Schedules, _stateHolder().Skills, false))
#pragma warning restore 618
			{
				if (optimizationPreferences.General.OptimizationStepFairness)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						runFairness(tagSetter, selectedAgents, schedulingOptions, selectedPeriod, optimizationPreferences,
							dayOffOptimizationPreferenceProvider);
						continuedStep = true;
					}
				}

				if (optimizationPreferences.General.OptimizationStepIntraInterval)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);
					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						runIntraInterval(schedulingOptions, optimizationPreferences, selectedPeriod, selectedAgents, tagSetter);
					}
				}

				//set back
				optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = onlyShiftsWhenUnderstaffed;

				var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_stateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization, selectedPeriod);
				runWeeklyRestSolver(optimizationPreferences, schedulingOptions, selectedPeriod, allMatrixes,
					selectedAgents.ToArray(), rollbackService, resourceCalculateDelayer, backgroundWorker,
					dayOffOptimizationPreferenceProvider);

				_maxSeatOptimization.Optimize(backgroundWorker, selectedPeriod, selectedAgents, _stateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.AllSkillDays(), optimizationPreferences, new DesktopMaxSeatCallback(_schedulerStateHolder()));
			}
		}

		private void runWeeklyRestSolver(IOptimizationPreferences optimizationPreferences, SchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod,
			IEnumerable<IScheduleMatrixPro> allMatrixes, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService,
								IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingProgress backgroundWorker,
								IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var singleAgentEntry = GroupPageLight.SingleAgentGroup(String.Empty);
			optimizationPreferences.Extra.TeamGroupPage = singleAgentEntry;
			optimizationPreferences.Extra.BlockTypeValue = BlockFinderType.SingleDay;
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService, resourceCalculateDelayer,
											selectedPeriod, allMatrixes, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}

		private bool runFlexibleTime(IOptimizationPreferences optimizerPreferences, bool continuedStep,
			DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedAgents,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IEnumerable<IScheduleMatrixPro> matrixListForIntradayOptimization)
		{
			bool runned = false;
			if (!(optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime ||
				optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime))
				return false;

			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForMoveMax =
				createMatrixContainerList(matrixListForIntradayOptimization);

			if (optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime)
			{
				recalculateIfContinuedStep(continuedStep, selectedPeriod);

				if (_progressEvent == null || !_progressEvent.Cancel)
				{
					_extendReduceTimeHelper.RunExtendReduceTimeOptimization(optimizerPreferences, _backgroundWorker,
						selectedAgents, _stateHolder(),
						selectedPeriod,
						matrixOriginalStateContainerListForMoveMax, dayOffOptimizationPreferenceProvider);

					runned = true;
				}
			}

			if (optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
			{
				recalculateIfContinuedStep(continuedStep, selectedPeriod);

				if (_progressEvent == null || !_progressEvent.Cancel)
				{
					_extendReduceDaysOffHelper.RunExtendReduceDayOffOptimization(optimizerPreferences, _backgroundWorker,
						selectedAgents, _schedulerStateHolder(),
						selectedPeriod,
						matrixOriginalStateContainerListForMoveMax,
						dayOffOptimizationPreferenceProvider);

					runned = true;
				}
			}

			return runned;
		}

		private void recalculateIfContinuedStep(bool continuedStep, DateOnlyPeriod selectedPeriod)
		{
			if (continuedStep)
			{
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					_resourceOptimizationHelper.ResourceCalculate(dateOnly, _stateHolder().ToResourceOptimizationData(true, false));
				}
			}
		}

		private void runFairness(IScheduleTagSetter tagSetter, IEnumerable<IPerson> selectedPersons,
			SchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var matrixListForFairness = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_stateHolder().Schedules, _stateHolder().PersonsInOrganization, selectedPeriod);
			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForFairness, optimizationPreferences, selectedPeriod);
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(),
				_scheduleDayChangeCallback(), tagSetter);

			var equalNumberOfCategoryFairnessService = _equalNumberOfCategoryFairnessService;
			equalNumberOfCategoryFairnessService.ReportProgress += resourceOptimizerPersonOptimized;
			equalNumberOfCategoryFairnessService.Execute(matrixListForFairness, selectedPeriod, selectedPersons,
				schedulingOptions, _schedulerStateHolder().Schedules, rollbackService,
				optimizationPreferences, dayOffOptimizationPreferenceProvider);
			equalNumberOfCategoryFairnessService.ReportProgress -= resourceOptimizerPersonOptimized;
			var teamBlockDayOffFairnessOptimizationService = _teamBlockDayOffFairnessOptimizationServiceFacade;
			teamBlockDayOffFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockDayOffFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons,
				schedulingOptions,
				_schedulerStateHolder().Schedules, rollbackService, optimizationPreferences, _stateHolder().SeniorityWorkDayRanks,
				dayOffOptimizationPreferenceProvider);
			teamBlockDayOffFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;

			var teamBlockSeniorityFairnessOptimizationService = _teamBlockSeniorityFairnessOptimizationService;
			teamBlockSeniorityFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockSeniorityFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons,
				schedulingOptions, _stateHolder().ShiftCategories.ToList(),
				_schedulerStateHolder().Schedules, rollbackService, optimizationPreferences, true,
				dayOffOptimizationPreferenceProvider);
			teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void runIntraInterval(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences,
			DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedAgents, IScheduleTagSetter tagSetter)
		{
			var args = new ResourceOptimizerProgressEventArgs(0, 0, Resources.CollectingData, optimizationPreferences.Advanced.RefreshScreenInterval);
			_backgroundWorker.ReportProgress(1, args);
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_schedulerStateHolder().Schedules, _stateHolder().PersonsInOrganization, selectedPeriod);
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(),
				tagSetter);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
				schedulingOptions.ConsiderShortBreaks, _stateHolder(), _userTimeZone);
			_intraIntervalOptimizationCommand.Execute(optimizationPreferences, selectedPeriod, selectedAgents,
				_schedulerStateHolder().SchedulingResultState, allMatrixes, rollbackService, resourceCalculateDelayer,
				_backgroundWorker);
		}

		private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(
			IEnumerable<IScheduleMatrixPro> matrixList)
		{
			var scheduleDayEquator = _scheduleDayEquator;
			IList<IScheduleMatrixOriginalStateContainer> result =
				matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();
			return result;
		}

		private void runIntradayOptimization(
			IOptimizationPreferences optimizerPreferences,
			IEnumerable<IPerson> scheduleAgents,
			ISchedulingProgress backgroundWorker,
			DateOnlyPeriod selectedPeriod)
		{
			_backgroundWorker = backgroundWorker;
			using (PerformanceOutput.ForOperation("Running new intraday optimization"))
			{
				if (_backgroundWorker.CancellationPending)
					return;

				if (_progressEvent != null && _progressEvent.Cancel) return;

				_optimizeIntradayDesktop.Optimize(scheduleAgents, selectedPeriod, optimizerPreferences, new IntradayOptimizationCallback(_backgroundWorker));
			}
		}

		private void runWorkShiftOptimization(
			IOptimizationPreferences optimizerPreferences,
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workshiftOriginalStateContainerList,
			DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_backgroundWorker = backgroundWorker;
			using (PerformanceOutput.ForOperation("Running move time optimization"))
			{
				if (_backgroundWorker.CancellationPending)
					return;

				if (_progressEvent != null && _progressEvent.Cancel) return;

				IList<IScheduleMatrixPro> matrixList =
					scheduleMatrixOriginalStateContainerList.Select(container => container.ScheduleMatrix).ToList();

				_optimizerHelperHelper.LockDaysForIntradayOptimization(matrixList, selectedPeriod);

				optimizeWorkShifts(scheduleMatrixOriginalStateContainerList, workshiftOriginalStateContainerList,
					optimizerPreferences, selectedPeriod, dayOffOptimizationPreferenceProvider);

				// we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
				ISchedulePartModifyAndRollbackService rollbackService =
					new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(),
						new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
				foreach (
					IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in scheduleMatrixOriginalStateContainerList)
				{
					if (!matrixOriginalStateContainer.IsFullyScheduled())
					{

						rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService, optimizerPreferences);
					}
				}
			}
		}

		private void runDayOffOptimization(IOptimizationPreferences optimizerPreferences,
			IEnumerable<IPerson> selectedAgents,
											DateOnlyPeriod selectedPeriod,
											IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (_backgroundWorker.CancellationPending)
				return;

			if (_progressEvent != null && _progressEvent.Cancel) return;
			_allResults = () => new WorkShiftFinderResultHolder();

			_dayOffOptimizationDesktop.Execute(selectedPeriod, selectedAgents, _backgroundWorker, optimizerPreferences, dayOffOptimizationPreferenceProvider, new GroupPageLight("_", GroupPageType.SingleAgent),  _allResults, resourceOptimizerPersonOptimized);
		}

		private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences)
		{
			var e = new ResourceOptimizerProgressEventArgs(0, 0,
				Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name, optimizationPreferences.Advanced.RefreshScreenInterval);
			resourceOptimizerPersonOptimized(this, e);
			if (e.Cancel) return;

			rollbackService.ClearModificationCollection();
			foreach (IScheduleDayPro scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
			{
				IScheduleDay originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
				rollbackService.Modify(originalPart);
			}
		}

		private void resourceOptimizerPersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			_backgroundWorker.ReportProgress(1, e);

			if (_progressEvent != null && _progressEvent.Cancel) return;
			_progressEvent = e;
		}
	}
}