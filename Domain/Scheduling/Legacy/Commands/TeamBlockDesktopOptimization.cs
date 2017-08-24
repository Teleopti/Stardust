using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	//legacy class - to be used from fat client only
	public class TeamBlockDesktopOptimization
	{
		private readonly MatrixListFactory _matrixListFactory;
		private readonly TeamBlockIntradayOptimizationService _teamBlockIntradayOptimizationService;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private ISchedulingProgress _backgroundWorker;
		private readonly IEqualNumberOfCategoryFairnessService _equalNumberOfCategoryFairness;
		private readonly ITeamBlockSeniorityFairnessOptimizationService _teamBlockSeniorityFairnessOptimizationService;
		private readonly ITeamBlockDayOffFairnessOptimizationServiceFacade _teamBlockDayOffFairnessOptimizationService;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly ITeamBlockMoveTimeBetweenDaysCommand _teamBlockMoveTimeBetweenDaysCommand;
		private readonly IntraIntervalOptimizationCommand _intraIntervalOptimizationCommand;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly TeamBlockDayOffOptimizer _teamBlockDayOffOptimizer;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly DayOffOptimizationDesktopTeamBlock _dayOffOptimizationDesktopTeamBlock;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly MaxSeatOptimization _maxSeatOptimization;
		private readonly DesktopOptimizationContext _desktopOptimizationContext;

		public TeamBlockDesktopOptimization(TeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			MatrixListFactory matrixListFactory,
			IEqualNumberOfCategoryFairnessService equalNumberOfCategoryFairness,
			ITeamBlockSeniorityFairnessOptimizationService teamBlockSeniorityFairnessOptimizationService,
			ITeamBlockDayOffFairnessOptimizationServiceFacade teamBlockDayOffFairnessOptimizationService,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			ITeamBlockMoveTimeBetweenDaysCommand teamBlockMoveTimeBetweenDaysCommand,
			IntraIntervalOptimizationCommand intraIntervalOptimizationCommand,
			IOptimizerHelperHelper optimizerHelper,
			TeamBlockDayOffOptimizer teamBlockDayOffOptimizer,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			TeamInfoFactoryFactory teamInfoFactoryFactory,
			DayOffOptimizationDesktopTeamBlock dayOffOptimizationDesktopTeamBlock,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			MaxSeatOptimization maxSeatOptimization,
			DesktopOptimizationContext desktopOptimizationContext)
		{
			_teamBlockIntradayOptimizationService = teamBlockIntradayOptimizationService;
			_schedulerStateHolder = schedulerStateHolder;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_matrixListFactory = matrixListFactory;
			_equalNumberOfCategoryFairness = equalNumberOfCategoryFairness;
			_teamBlockSeniorityFairnessOptimizationService = teamBlockSeniorityFairnessOptimizationService;
			_teamBlockDayOffFairnessOptimizationService = teamBlockDayOffFairnessOptimizationService;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_teamBlockMoveTimeBetweenDaysCommand = teamBlockMoveTimeBetweenDaysCommand;
			_intraIntervalOptimizationCommand = intraIntervalOptimizationCommand;
			_optimizerHelper = optimizerHelper;
			_teamBlockDayOffOptimizer = teamBlockDayOffOptimizer;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_dayOffOptimizationDesktopTeamBlock = dayOffOptimizationDesktopTeamBlock;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_maxSeatOptimization = maxSeatOptimization;
			_desktopOptimizationContext = desktopOptimizationContext;
		}

		public void Execute(ISchedulingProgress backgroundWorker, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation, IScheduleTagSetter tagSetter,
			SchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_backgroundWorker = backgroundWorker;
			var args = new ResourceOptimizerProgressEventArgs(0, 0, UserTexts.Resources.CollectingData, optimizationPreferences.Advanced.RefreshScreenInterval);
			_backgroundWorker.ReportProgress(1, args);

			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization, selectedPeriod);
			if (optimizationPreferences.General.OptimizationStepDaysOff)
			{
				_dayOffOptimizationDesktopTeamBlock.Execute(selectedPeriod, selectedPersons, _backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider, schedulingOptions.GroupOnGroupPageForTeamBlockPer, null, null);
			}

			using (_resourceCalculationContextFactory.Create(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.Skills, false, selectedPeriod.Inflate(1)))
			{
				var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().AllPermittedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
				var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory);

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
						rollbackServiceWithResourceCalculation, resourceCalculateDelayer, teamBlockGenerator);
				}

				if (optimizationPreferences.General.OptimizationStepTimeBetweenDays &&
						!(optimizationPreferences.Extra.UseBlockSameShift && optimizationPreferences.Extra.UseTeamBlockOption))
				{
					var matrixesOnSelectedperiod = _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedPersons, selectedPeriod);
					optimizeMoveTimeBetweenDays(backgroundWorker, selectedPeriod, selectedPersons, optimizationPreferences,
						rollbackServiceWithResourceCalculation, schedulingOptions, resourceCalculateDelayer, matrixesOnSelectedperiod,
						allMatrixes);
				}

				if (optimizationPreferences.General.OptimizationStepFairness)
				{
					var rollbackServiceWithoutResourceCalculation =
						new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback, tagSetter);
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
					_intraIntervalOptimizationCommand.Execute(optimizationPreferences, selectedPeriod, selectedPersons,
						rollbackServiceWithResourceCalculation, resourceCalculateDelayer, _backgroundWorker);
				}

				allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization, selectedPeriod);

				solveWeeklyRestViolations(selectedPeriod, selectedPersons, optimizationPreferences, resourceCalculateDelayer,
					rollbackServiceWithResourceCalculation, allMatrixes,
					_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences), dayOffOptimizationPreferenceProvider);

				//run twice to se if it will solve the problem detected by sikuli tests
				solveWeeklyRestViolations(selectedPeriod, selectedPersons, optimizationPreferences, resourceCalculateDelayer,
					rollbackServiceWithResourceCalculation, allMatrixes,
					_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences), dayOffOptimizationPreferenceProvider);
			}

			_maxSeatOptimization.Optimize(backgroundWorker, selectedPeriod, selectedPersons, _schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.AllSkillDays(), optimizationPreferences, new DesktopMaxSeatCallback(_schedulerStateHolder()));
		}

		private void optimizeMoveTimeBetweenDays(ISchedulingProgress backgroundWorker, DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation, SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<IScheduleMatrixPro> matrixesOnSelectedperiod,
			IEnumerable<IScheduleMatrixPro> allMatrixes)
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

		private void solveWeeklyRestViolations(DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulePartModifyAndRollbackService rollbackService,
			IEnumerable<IScheduleMatrixPro> allMatrixes,
			SchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService,
				resourceCalculateDelayer, selectedPeriod, allMatrixes, _backgroundWorker, dayOffOptimizationPreferenceProvider);
		}

		private void optimizeTeamBlockDaysOff(DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IEnumerable<IScheduleMatrixPro> allMatrixes,
			SchedulingOptions schedulingOptions,
			ITeamInfoFactory teamInfoFactory,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_optimizerHelper.LockDaysForDayOffOptimization(allMatrixes, optimizationPreferences, selectedPeriod);

			_teamBlockDayOffOptimizer.OptimizeDaysOff(
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

		private void optimizeTeamBlockIntraday(DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IEnumerable<IScheduleMatrixPro> allMatrixes,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockGenerator teamBlockGenerator)
		{
			//temp hack until we get further with 45508 and use optimizeintradaydesktop here (=islands)
			var tempCmd = new tempCommand {CommandId = Guid.NewGuid()};
			using (_desktopOptimizationContext.Set(tempCmd, null, null, new IntradayOptimizationCallback(_backgroundWorker)))
			{
				using (CommandScope.Create(tempCmd))
				{
					_teamBlockIntradayOptimizationService.Optimize(
						allMatrixes,
						selectedPeriod,
						selectedPersons,
						optimizationPreferences,
						schedulePartModifyAndRollbackService,
						resourceCalculateDelayer,
						_schedulerStateHolder().SchedulingResultState.SkillDays,
						_schedulerStateHolder().Schedules,
						_schedulerStateHolder().SchedulingResultState.PersonsInOrganization,
						NewBusinessRuleCollection.AllForScheduling(_schedulerStateHolder().SchedulingResultState),
						teamBlockGenerator);
				}
			}
		}
		[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
		private class tempCommand : ICommandIdentifier
		{
			public Guid CommandId { get; set; }
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