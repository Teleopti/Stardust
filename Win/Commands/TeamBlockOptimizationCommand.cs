﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Commands
{
	public interface ITeamBlockOptimizationCommand
	{
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		void Execute(BackgroundWorker backgroundWorker, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			IScheduleTagSetter tagSetter, ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleDay> selectedSchedules);
	}

	public class TeamBlockOptimizationCommand : ITeamBlockOptimizationCommand
	{
		private readonly IDayOffBackToLegalStateFunctions _dayOffBackToLegalStateFunctions;
		private readonly IDayOffDecisionMaker _dayOffDecisionMaker;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockClearer _teamBlockCleaner;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private BackgroundWorker _backgroundWorker;
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
		private readonly IToggleManager _toggleManager;
		private readonly IIntraIntervalOptimizationCommand _intraIntervalOptimizationCommand;

		public TeamBlockOptimizationCommand(ISchedulerStateHolder schedulerStateHolder,
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
			IRestrictionExtractor restrictionExtractor,
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
			IToggleManager toggleManager,
			IIntraIntervalOptimizationCommand intraIntervalOptimizationCommand)
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
			_restrictionExtractor = restrictionExtractor;
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
		}

		public void Execute(BackgroundWorker backgroundWorker, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation, IScheduleTagSetter tagSetter,
			ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer,
			IList<IScheduleDay> selectedSchedules)
		{

			_backgroundWorker = backgroundWorker;
			var args = new ResourceOptimizerProgressEventArgs(0, 0, LanguageResourceHelper.Translate("XXCollectingData"));
			_backgroundWorker.ReportProgress(1, args);

			IList<IScheduleMatrixPro> allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);

			var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
			var teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,
				_teamBlockScheudlingOptions);

			if (optimizationPreferences.General.OptimizationStepDaysOff)
				optimizeTeamBlockDaysOff(selectedPeriod, selectedPersons, optimizationPreferences,
					allMatrixes, rollbackServiceWithResourceCalculation,
					schedulingOptions, teamInfoFactory, resourceCalculateDelayer);

			if (optimizationPreferences.General.OptimizationStepShiftsWithinDay)
				optimizeTeamBlockIntraday(selectedPeriod, selectedPersons, optimizationPreferences, allMatrixes,
					rollbackServiceWithResourceCalculation, resourceCalculateDelayer, teamBlockGenerator);

			if (optimizationPreferences.General.OptimizationStepTimeBetweenDays && !(optimizationPreferences.Extra.UseBlockSameShift && optimizationPreferences.Extra.UseTeamBlockOption))
			{
				var matrixesOnSelectedperiod = _matrixListFactory.CreateMatrixList(selectedSchedules, selectedPeriod);
				optimizeMoveTimeBetweenDays(backgroundWorker, selectedPeriod, selectedPersons, optimizationPreferences,
					rollbackServiceWithResourceCalculation, schedulingOptions, resourceCalculateDelayer, matrixesOnSelectedperiod,
					allMatrixes);
			}

			if (optimizationPreferences.General.OptimizationStepFairness)
			{
				var rollbackServiceWithoutResourceCalculation =
					new SchedulePartModifyAndRollbackService(_schedulerStateHolder.SchedulingResultState,
						new DoNothingScheduleDayChangeCallBack(), tagSetter);
				OptimizerHelperHelper.LockDaysForDayOffOptimization(allMatrixes, _restrictionExtractor,
					optimizationPreferences, selectedPeriod);

				_equalNumberOfCategoryFairness.ReportProgress += resourceOptimizerPersonOptimized;
				_equalNumberOfCategoryFairness.Execute(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions,
					_schedulerStateHolder.Schedules, rollbackServiceWithoutResourceCalculation,
					optimizationPreferences, _toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317), _toggleManager.IsEnabled(Toggles.Scheduler_Seniority_11111));
				_equalNumberOfCategoryFairness.ReportProgress -= resourceOptimizerPersonOptimized;

				if (_toggleManager.IsEnabled(Toggles.Scheduler_Seniority_11111))
				{
					_teamBlockDayOffFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
					_teamBlockDayOffFairnessOptimizationService.Execute(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions,
						_schedulerStateHolder.Schedules,
						rollbackServiceWithoutResourceCalculation, optimizationPreferences, true);
					_teamBlockDayOffFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;


					_teamBlockSeniorityFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
					_teamBlockSeniorityFairnessOptimizationService.Execute(allMatrixes, selectedPeriod, selectedPersons,
						schedulingOptions, _schedulerStateHolder.CommonStateHolder.ShiftCategories.ToList(),
						_schedulerStateHolder.Schedules, rollbackServiceWithoutResourceCalculation, optimizationPreferences, true);

					_teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
				}
			}

			if (optimizationPreferences.General.OptimizationStepIntraInterval)
			{
				_intraIntervalOptimizationCommand.Execute(schedulingOptions, selectedPeriod, selectedSchedules, _schedulerStateHolder.SchedulingResultState, allMatrixes, rollbackServiceWithResourceCalculation, resourceCalculateDelayer, _backgroundWorker);	
			}

			solveWeeklyRestViolations(selectedPeriod, selectedPersons, optimizationPreferences, resourceCalculateDelayer,
				rollbackServiceWithResourceCalculation, allMatrixes,
				_schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences));

		}

		private void optimizeMoveTimeBetweenDays(BackgroundWorker backgroundWorker, DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation, ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> matrixesOnSelectedperiod,
			IList<IScheduleMatrixPro> allMatrixes)
		{
			IScheduleResultDataExtractor allSkillsDataExtractor =
				OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod,
					_schedulerStateHolder.SchedulingResultState);
			IPeriodValueCalculator periodValueCalculatorForAllSkills =
				OptimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced,
					allSkillsDataExtractor);
			_teamBlockMoveTimeBetweenDaysCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons,
				rollbackServiceWithResourceCalculation, resourceCalculateDelayer, selectedPeriod, allMatrixes, backgroundWorker,
				periodValueCalculatorForAllSkills,
				_schedulerStateHolder.SchedulingResultState, matrixesOnSelectedperiod);
		}

		private void solveWeeklyRestViolations(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulePartModifyAndRollbackService rollbackService,
			IList<IScheduleMatrixPro> allMatrixes,
			ISchedulingOptions schedulingOptions)
		{
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService,
				resourceCalculateDelayer, selectedPeriod, allMatrixes, _backgroundWorker);
		}

		private void optimizeTeamBlockDaysOff(DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IList<IScheduleMatrixPro> allMatrixes,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			ISchedulingOptions schedulingOptions,
			ITeamInfoFactory teamInfoFactory,
			IResourceCalculateDelayer resourceCalculateDelayer)
		{

			OptimizerHelperHelper.LockDaysForDayOffOptimization(allMatrixes, _restrictionExtractor,
				optimizationPreferences, selectedPeriod);

			ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService
				= new SmartDayOffBackToLegalStateService(
					_dayOffBackToLegalStateFunctions,
					optimizationPreferences.DaysOff,
					100,
					_dayOffDecisionMaker);

			IScheduleResultDataExtractor allSkillsDataExtractor =
				OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod,
					_schedulerStateHolder.SchedulingResultState);
			IPeriodValueCalculator periodValueCalculatorForAllSkills =
				OptimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced,
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
					_teamBlockScheudlingOptions, _allTeamMembersInSelectionSpecification
					);

			IList<IDayOffTemplate> dayOffTemplates = (from item in _schedulerStateHolder.CommonStateHolder.DayOffs
				where ((IDeleteTag) item).IsDeleted == false
				select item).ToList();

			((List<IDayOffTemplate>) dayOffTemplates).Sort(new DayOffTemplateSorter());

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
				_schedulerStateHolder.SchedulingResultState);
			teamBlockDayOffOptimizerService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void optimizeTeamBlockIntraday(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			IList<IScheduleMatrixPro> allMatrixes,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ITeamBlockGenerator teamBlockGenerator)
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
					//this shouldn't be here.
					//should be two different impl of the interface instead
					_toggleManager.IsEnabled(Toggles.Scheduler_TeamBlockAdhereWithMaxSeatRule_23419)
					);

			teamBlockIntradayOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockIntradayOptimizationService.Optimize(
				allMatrixes,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulePartModifyAndRollbackService,
				resourceCalculateDelayer,
				_schedulerStateHolder.SchedulingResultState);
			teamBlockIntradayOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void resourceOptimizerPersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
				e.UserCancel = true;
			}
			_backgroundWorker.ReportProgress(1, e);
		}
	}
}