using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TeamBlockOptimizationCommand : ITeamBlockOptimizationCommand
	{
		private readonly IDayOffBackToLegalStateFunctions _dayOffBackToLegalStateFunctions;
		private readonly IDayOffDecisionMaker _dayOffDecisionMaker;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockClearer _teamBlockCleaner;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private IBackgroundWorkerWrapper _backgroundWorker;
		private readonly ITeamBlockSchedulingOptions _teamBlockScheudlingOptions;
		private readonly IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private readonly IEqualNumberOfCategoryFairnessService _equalNumberOfCategoryFairness;
		private readonly ITeamBlockSeniorityFairnessOptimizationService _teamBlockSeniorityFairnessOptimizationService;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ITeamBlockDayOffFairnessOptimizationServiceFacade _teamBlockDayOffFairnessOptimizationService;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private readonly ITeamBlockMoveTimeBetweenDaysCommand _teamBlockMoveTimeBetweenDaysCommand;
		private readonly IScheduleCommandToggle _toggleManager;
		private readonly IIntraIntervalOptimizationCommand _intraIntervalOptimizationCommand;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly ITeamBlockDayOffsInPeriodValidator _teamBlockDayOffsInPeriodValidator;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;

		public TeamBlockOptimizationCommand(Func<ISchedulerStateHolder> schedulerStateHolder,
			ITeamBlockClearer teamBlockCleaner,
			IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions,
			IDayOffDecisionMaker dayOffDecisionMaker,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
			ILockableBitArrayFactory lockableBitArrayFactory,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ILockableBitArrayChangesTracker lockableBitArrayChangesTracker,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifier teamDayOffModifier,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
			ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
			IMatrixListFactory matrixListFactory,
			ITeamBlockSchedulingOptions teamBlockScheudlingOptions,
			IDailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock,
			IEqualNumberOfCategoryFairnessService equalNumberOfCategoryFairness,
			ITeamBlockSeniorityFairnessOptimizationService teamBlockSeniorityFairnessOptimizationService,
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits,
			ITeamBlockDayOffFairnessOptimizationServiceFacade teamBlockDayOffFairnessOptimizationService,
			ITeamBlockScheduler teamBlockScheduler, IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IAllTeamMembersInSelectionSpecification allTeamMembersInSelectionSpecification,
			ITeamBlockMoveTimeBetweenDaysCommand teamBlockMoveTimeBetweenDaysCommand,
			IScheduleCommandToggle toggleManager,
			IIntraIntervalOptimizationCommand intraIntervalOptimizationCommand,
			IOptimizerHelperHelper optimizerHelper,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			ITeamBlockDayOffsInPeriodValidator teamBlockDayOffsInPeriodValidator,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockCleaner = teamBlockCleaner;
			_dayOffBackToLegalStateFunctions = dayOffBackToLegalStateFunctions;
			_dayOffDecisionMaker = dayOffDecisionMaker;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamDayOffModifier = teamDayOffModifier;
			_teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_matrixListFactory = matrixListFactory;
			_teamBlockScheudlingOptions = teamBlockScheudlingOptions;
			_dailyTargetValueCalculatorForTeamBlock = dailyTargetValueCalculatorForTeamBlock;
			_equalNumberOfCategoryFairness = equalNumberOfCategoryFairness;
			_teamBlockSeniorityFairnessOptimizationService = teamBlockSeniorityFairnessOptimizationService;
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
			_teamBlockDayOffFairnessOptimizationService = teamBlockDayOffFairnessOptimizationService;
			_teamBlockScheduler = teamBlockScheduler;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_allTeamMembersInSelectionSpecification = allTeamMembersInSelectionSpecification;
			_teamBlockMoveTimeBetweenDaysCommand = teamBlockMoveTimeBetweenDaysCommand;
			_toggleManager = toggleManager;
			_intraIntervalOptimizationCommand = intraIntervalOptimizationCommand;
			_optimizerHelper = optimizerHelper;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_teamBlockDayOffsInPeriodValidator = teamBlockDayOffsInPeriodValidator;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
		}

		public void Execute(IBackgroundWorkerWrapper backgroundWorker, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation, IScheduleTagSetter tagSetter,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			IList<IScheduleDay> selectedSchedules,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{

			_backgroundWorker = backgroundWorker;
			var args = new ResourceOptimizerProgressEventArgs(0, 0, UserTexts.Resources.CollectingData);
			_backgroundWorker.ReportProgress(1, args);

			IList<IScheduleMatrixPro> allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(selectedPeriod);

			_groupPersonBuilderWrapper.Reset();
			var groupPageType = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type;
			if (groupPageType == GroupPageType.SingleAgent)
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			else
				_groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);

			var teamInfoFactory = new TeamInfoFactory(_groupPersonBuilderWrapper);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,
				_teamBlockScheudlingOptions);

			if (optimizationPreferences.General.OptimizationStepDaysOff)
				optimizeTeamBlockDaysOff(selectedPeriod, selectedPersons, optimizationPreferences,
					allMatrixes, rollbackServiceWithResourceCalculation,
					schedulingOptions, teamInfoFactory, resourceCalculateDelayer, dayOffOptimizationPreferenceProvider);

			if (optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime && _toggleManager.IsEnabled(Toggles.Scheduler_OptimizeFlexibleDayOffs_22409))
			{
				var optimizeDayOffs = optimizationPreferences.General.OptimizationStepDaysOff;
				optimizationPreferences.General.OptimizationStepDaysOff = false;
				optimizeTeamBlockDaysOff(selectedPeriod, selectedPersons, optimizationPreferences, allMatrixes, rollbackServiceWithResourceCalculation, 
										schedulingOptions, teamInfoFactory, resourceCalculateDelayer, dayOffOptimizationPreferenceProvider);
				optimizationPreferences.General.OptimizationStepDaysOff = optimizeDayOffs;
			}

			if (optimizationPreferences.General.OptimizationStepShiftsWithinDay)
				optimizeTeamBlockIntraday(selectedPeriod, selectedPersons, optimizationPreferences, allMatrixes,
					rollbackServiceWithResourceCalculation, resourceCalculateDelayer, teamBlockGenerator, dayOffOptimizationPreferenceProvider);

			if (optimizationPreferences.General.OptimizationStepTimeBetweenDays && !(optimizationPreferences.Extra.UseBlockSameShift && optimizationPreferences.Extra.UseTeamBlockOption))
			{
				var matrixesOnSelectedperiod = _matrixListFactory.CreateMatrixListForSelection(selectedSchedules);
				optimizeMoveTimeBetweenDays(backgroundWorker, selectedPeriod, selectedPersons, optimizationPreferences,
					rollbackServiceWithResourceCalculation, schedulingOptions, resourceCalculateDelayer, matrixesOnSelectedperiod,
					allMatrixes);
			}

			if (optimizationPreferences.General.OptimizationStepFairness)
			{
				var rollbackServiceWithoutResourceCalculation =
					new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState,
						new DoNothingScheduleDayChangeCallBack(), tagSetter);
				_optimizerHelper.LockDaysForDayOffOptimization(allMatrixes, optimizationPreferences, selectedPeriod);

				_equalNumberOfCategoryFairness.ReportProgress += resourceOptimizerPersonOptimized;
				_equalNumberOfCategoryFairness.Execute(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions,
					_schedulerStateHolder().Schedules, rollbackServiceWithoutResourceCalculation,
					optimizationPreferences, dayOffOptimizationPreferenceProvider);
				_equalNumberOfCategoryFairness.ReportProgress -= resourceOptimizerPersonOptimized;

				_teamBlockDayOffFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
				_teamBlockDayOffFairnessOptimizationService.Execute(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions,
					_schedulerStateHolder().Schedules, rollbackServiceWithoutResourceCalculation, optimizationPreferences,
					_schedulerStateHolder().SchedulingResultState.SeniorityWorkDayRanks, dayOffOptimizationPreferenceProvider);
				_teamBlockDayOffFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;


				_teamBlockSeniorityFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
				_teamBlockSeniorityFairnessOptimizationService.Execute(allMatrixes, selectedPeriod, selectedPersons,
					schedulingOptions, _schedulerStateHolder().CommonStateHolder.ShiftCategories.ToList(),
					_schedulerStateHolder().Schedules, rollbackServiceWithoutResourceCalculation, optimizationPreferences, true, dayOffOptimizationPreferenceProvider);

				_teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
			}

			if (optimizationPreferences.General.OptimizationStepIntraInterval)
			{
				_intraIntervalOptimizationCommand.Execute(optimizationPreferences, selectedPeriod, selectedSchedules, _schedulerStateHolder().SchedulingResultState, allMatrixes, rollbackServiceWithResourceCalculation, resourceCalculateDelayer, _backgroundWorker);
			}

			allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(selectedPeriod);

			solveWeeklyRestViolations(selectedPeriod, selectedPersons, optimizationPreferences, resourceCalculateDelayer,
				rollbackServiceWithResourceCalculation, allMatrixes,
				_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences), dayOffOptimizationPreferenceProvider);

			//run twice to se if it will solve the problem detected by sikuli tests
			solveWeeklyRestViolations(selectedPeriod, selectedPersons, optimizationPreferences, resourceCalculateDelayer,
				rollbackServiceWithResourceCalculation, allMatrixes,
				_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences), dayOffOptimizationPreferenceProvider);

		}

		private void optimizeMoveTimeBetweenDays(IBackgroundWorkerWrapper backgroundWorker, DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation, ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> matrixesOnSelectedperiod,
			IList<IScheduleMatrixPro> allMatrixes)
		{
			IScheduleResultDataExtractor allSkillsDataExtractor =
				_optimizerHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod,
					_schedulerStateHolder().SchedulingResultState);
			IPeriodValueCalculator periodValueCalculatorForAllSkills =
				_optimizerHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced,
					allSkillsDataExtractor);
			_teamBlockMoveTimeBetweenDaysCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons,
				rollbackServiceWithResourceCalculation, resourceCalculateDelayer, selectedPeriod, allMatrixes, backgroundWorker,
				periodValueCalculatorForAllSkills,
				_schedulerStateHolder().SchedulingResultState, matrixesOnSelectedperiod);
		}

		private void solveWeeklyRestViolations(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulePartModifyAndRollbackService rollbackService,
			IList<IScheduleMatrixPro> allMatrixes,
			ISchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService,
				resourceCalculateDelayer, selectedPeriod, allMatrixes, _backgroundWorker, dayOffOptimizationPreferenceProvider);
		}

		private void optimizeTeamBlockDaysOff(DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IList<IScheduleMatrixPro> allMatrixes,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			ISchedulingOptions schedulingOptions,
			ITeamInfoFactory teamInfoFactory,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{

			_optimizerHelper.LockDaysForDayOffOptimization(allMatrixes, optimizationPreferences, selectedPeriod);

			ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService
				= new SmartDayOffBackToLegalStateService(
					_dayOffBackToLegalStateFunctions,
					100,
					_dayOffDecisionMaker);

			IScheduleResultDataExtractor allSkillsDataExtractor =
				_optimizerHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod,
					_schedulerStateHolder().SchedulingResultState);
			IPeriodValueCalculator periodValueCalculatorForAllSkills =
				_optimizerHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced,
					allSkillsDataExtractor);
			ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder =
				new TeamBlockDaysOffMoveFinder(_scheduleResultDataExtractorProvider,
					dayOffBackToLegalStateService,
					_dayOffOptimizationDecisionMakerFactory);

			ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService =
				new TeamBlockDayOffOptimizerService(
					teamInfoFactory,
					_lockableBitArrayFactory,
					_lockableBitArrayChangesTracker,
					_teamBlockScheduler,
					_teamBlockInfoFactory,
					periodValueCalculatorForAllSkills,
					_safeRollbackAndResourceCalculation,
					_teamDayOffModifier,
					_teamBlockSteadyStateValidator,
					_teamBlockCleaner,
					_teamBlockOptimizationLimits,
					_teamBlockMaxSeatChecker,
					teamBlockDaysOffMoveFinder,
					_teamBlockScheudlingOptions, _allTeamMembersInSelectionSpecification,
					_teamBlockShiftCategoryLimitationValidator,
					_teamBlockDayOffsInPeriodValidator
					);

			IList<IDayOffTemplate> dayOffTemplates = (from item in _schedulerStateHolder().CommonStateHolder.DayOffs
				where ((IDeleteTag)item).IsDeleted == false
				select item).ToList();

			((List<IDayOffTemplate>)dayOffTemplates).Sort(new DayOffTemplateSorter());

			teamBlockDayOffOptimizerService.ReportProgress += resourceOptimizerPersonOptimized;
			schedulingOptions.DayOffTemplate = dayOffTemplates[0];
			teamBlockDayOffOptimizerService.OptimizeDaysOff(
				allMatrixes,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulePartModifyAndRollbackService,
				schedulingOptions,
				resourceCalculateDelayer,
				_schedulerStateHolder().SchedulingResultState,
				dayOffOptimizationPreferenceProvider);
			teamBlockDayOffOptimizerService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void optimizeTeamBlockIntraday(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IList<IScheduleMatrixPro> allMatrixes,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockGenerator teamBlockGenerator,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			ITeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService =
				new TeamBlockIntradayOptimizationService(
					teamBlockGenerator,
					_teamBlockScheduler,
					_schedulingOptionsCreator,
					_safeRollbackAndResourceCalculation,
					_teamBlockIntradayDecisionMaker,
					_teamBlockOptimizationLimits,
					_teamBlockCleaner,
					_teamBlockMaxSeatChecker,
					_dailyTargetValueCalculatorForTeamBlock,
					_teamBlockSteadyStateValidator,
					_teamBlockShiftCategoryLimitationValidator
					);

			teamBlockIntradayOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockIntradayOptimizationService.Optimize(
				allMatrixes,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulePartModifyAndRollbackService,
				resourceCalculateDelayer,
				_schedulerStateHolder().SchedulingResultState,
				dayOffOptimizationPreferenceProvider);
			teamBlockIntradayOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void resourceOptimizerPersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			_backgroundWorker.ReportProgress(1, e);
		}
	}
}