using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TeamBlockOptimizationCommand : ITeamBlockOptimizationCommand
	{
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockClearer _teamBlockCleaner;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private ISchedulingProgress _backgroundWorker;
		private readonly ITeamBlockSchedulingOptions _teamBlockScheudlingOptions;
		private readonly IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private readonly IEqualNumberOfCategoryFairnessService _equalNumberOfCategoryFairness;
		private readonly ITeamBlockSeniorityFairnessOptimizationService _teamBlockSeniorityFairnessOptimizationService;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ITeamBlockDayOffFairnessOptimizationServiceFacade _teamBlockDayOffFairnessOptimizationService;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly ITeamBlockMoveTimeBetweenDaysCommand _teamBlockMoveTimeBetweenDaysCommand;
		private readonly IIntraIntervalOptimizationCommand _intraIntervalOptimizationCommand;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly ITeamBlockDayOffOptimizerService _teamBlockDayOffOptimizerService;
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly DayOffOptimizationDesktopTeamBlock _dayOffOptimizationDesktopTeamBlock;

		public TeamBlockOptimizationCommand(Func<ISchedulerStateHolder> schedulerStateHolder,
			ITeamBlockClearer teamBlockCleaner,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
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
			ITeamBlockMoveTimeBetweenDaysCommand teamBlockMoveTimeBetweenDaysCommand,
			IIntraIntervalOptimizationCommand intraIntervalOptimizationCommand,
			IOptimizerHelperHelper optimizerHelper,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService,
			IResourceCalculationContextFactory resourceCalculationContextFactory,
			TeamInfoFactoryFactory teamInfoFactoryFactory,
			DayOffOptimizationDesktopTeamBlock dayOffOptimizationDesktopTeamBlock)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockCleaner = teamBlockCleaner;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
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
			_teamBlockMoveTimeBetweenDaysCommand = teamBlockMoveTimeBetweenDaysCommand;
			_intraIntervalOptimizationCommand = intraIntervalOptimizationCommand;
			_optimizerHelper = optimizerHelper;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_teamBlockDayOffOptimizerService = teamBlockDayOffOptimizerService;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_dayOffOptimizationDesktopTeamBlock = dayOffOptimizationDesktopTeamBlock;
		}

		public void Execute(ISchedulingProgress backgroundWorker, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation, IScheduleTagSetter tagSetter,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			IList<IScheduleDay> selectedSchedules,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_backgroundWorker = backgroundWorker;
			var args = new ResourceOptimizerProgressEventArgs(0, 0, UserTexts.Resources.CollectingData);
			_backgroundWorker.ReportProgress(1, args);

			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(selectedPeriod);
			if (optimizationPreferences.General.OptimizationStepDaysOff)
			{
				_dayOffOptimizationDesktopTeamBlock.Execute(selectedPeriod, selectedSchedules, _backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider, schedulingOptions.GroupOnGroupPageForTeamBlockPer, null, null);
			}

			using (_resourceCalculationContextFactory.Create(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.Skills))
			{
				var teamInfoFactory = _teamInfoFactoryFactory.Create(schedulingOptions.GroupOnGroupPageForTeamBlockPer);
				var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory, _teamBlockScheudlingOptions);

				if (optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
				{
					var optimizeDayOffs = optimizationPreferences.General.OptimizationStepDaysOff;
					optimizationPreferences.General.OptimizationStepDaysOff = false;
					//should probably use _dayOffOptimizationDesktopTeamBlock here later
					optimizeTeamBlockDaysOff(selectedPeriod, selectedPersons, optimizationPreferences, allMatrixes,
						schedulingOptions, teamInfoFactory, resourceCalculateDelayer, dayOffOptimizationPreferenceProvider);
					optimizationPreferences.General.OptimizationStepDaysOff = optimizeDayOffs;
				}

				if (optimizationPreferences.General.OptimizationStepShiftsWithinDay)
				{
					optimizeTeamBlockIntraday(selectedPeriod, selectedPersons, optimizationPreferences, allMatrixes,
						rollbackServiceWithResourceCalculation, resourceCalculateDelayer, teamBlockGenerator,
						dayOffOptimizationPreferenceProvider);
				}

				if (optimizationPreferences.General.OptimizationStepTimeBetweenDays &&
						!(optimizationPreferences.Extra.UseBlockSameShift && optimizationPreferences.Extra.UseTeamBlockOption))
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
							new ResourceCalculationOnlyScheduleDayChangeCallback(), tagSetter);
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
						_schedulerStateHolder().Schedules, rollbackServiceWithoutResourceCalculation, optimizationPreferences, true,
						dayOffOptimizationPreferenceProvider);

					_teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
				}

				if (optimizationPreferences.General.OptimizationStepIntraInterval)
				{
					_intraIntervalOptimizationCommand.Execute(optimizationPreferences, selectedPeriod, selectedSchedules,
						_schedulerStateHolder().SchedulingResultState, allMatrixes, rollbackServiceWithResourceCalculation,
						resourceCalculateDelayer, _backgroundWorker);
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
		}

		private void optimizeMoveTimeBetweenDays(ISchedulingProgress backgroundWorker, DateOnlyPeriod selectedPeriod,
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
			ISchedulingOptions schedulingOptions,
			ITeamInfoFactory teamInfoFactory,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_optimizerHelper.LockDaysForDayOffOptimization(allMatrixes, optimizationPreferences, selectedPeriod);

			_teamBlockDayOffOptimizerService.OptimizeDaysOff(
				allMatrixes,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulingOptions,
				resourceCalculateDelayer,
				dayOffOptimizationPreferenceProvider,
				teamInfoFactory,
				_backgroundWorker);
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