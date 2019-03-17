using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.UndoRedo;
using DateOnly = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockIntradayOptimizationService
	{
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly TeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly DailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private readonly ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly SetMainShiftOptimizeActivitySpecificationForTeamBlock _setMainShiftOptimizeActivitySpecificationForTeamBlock;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly ICurrentOptimizationCallback _currentOptimizationCallback;
		private readonly BlockPreferencesMapper _blockPreferencesMapper;

		public TeamBlockIntradayOptimizationService(TeamBlockScheduler teamBlockScheduler,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			TeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
			TeamBlockClearer teamBlockClearer,
			DailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock,
			ITeamBlockSteadyStateValidator teamTeamBlockSteadyStateValidator,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			SetMainShiftOptimizeActivitySpecificationForTeamBlock setMainShiftOptimizeActivitySpecificationForTeamBlock,
			IOptimizerHelperHelper optimizerHelperHelper,
			ICurrentOptimizationCallback currentOptimizationCallback, 
			BlockPreferencesMapper blockPreferencesMapper)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_dailyTargetValueCalculatorForTeamBlock = dailyTargetValueCalculatorForTeamBlock;
			_teamTeamBlockSteadyStateValidator = teamTeamBlockSteadyStateValidator;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_setMainShiftOptimizeActivitySpecificationForTeamBlock = setMainShiftOptimizeActivitySpecificationForTeamBlock;
			_optimizerHelperHelper = optimizerHelperHelper;
			_currentOptimizationCallback = currentOptimizationCallback;
			_blockPreferencesMapper = blockPreferencesMapper;
		}

		public void Optimize(
			IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod, 
			IEnumerable<IPerson> selectedPersons, 
			IOptimizationPreferences optimizationPreferences, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			IScheduleDictionary scheduleDictionary,
			IEnumerable<IPerson> personsInOrganization, 
			INewBusinessRuleCollection businessRuleCollection, 
			ITeamBlockGenerator teamBlockGenerator, 
			IBlockPreferenceProvider blockPreferenceProvider)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			_optimizerHelperHelper.LockDaysForIntradayOptimization(allPersonMatrixList, selectedPeriod);
			var teamBlocks = teamBlockGenerator.Generate(personsInOrganization, allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions, blockPreferenceProvider);
			
			while (teamBlocks.Count > 0)
			{
				if (_currentOptimizationCallback.Current().IsCancelled())
					return;

				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, optimizationPreferences,
					schedulingOptions, teamBlocks,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer,
					skillDays, scheduleDictionary, businessRuleCollection, blockPreferenceProvider);
				foreach (var teamBlock in teamBlocksToRemove)
				{
					teamBlocks.Remove(teamBlock);
				}
			}
		}

		private IEnumerable<ITeamBlockInfo> optimizeOneRound(
			DateOnlyPeriod selectedPeriod, 
			IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions,
			IList<ITeamBlockInfo> allTeamBlockInfos, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			IScheduleDictionary scheduleDictionary,
			INewBusinessRuleCollection businessRuleCollection,
			IBlockPreferenceProvider blockPreferenceProvider)
		{
			var teamBlockToRemove = new List<ITeamBlockInfo>();
			var skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDays);
			var callback = _currentOptimizationCallback.Current();
			var sortedTeamBlockInfos = _teamBlockIntradayDecisionMaker.Decide(allTeamBlockInfos, schedulingOptions);
			var totalTeamBlockInfos = sortedTeamBlockInfos.Count;
			var runningTeamBlockCounter = 0;
			foreach (var teamBlockInfo in sortedTeamBlockInfos)
			{
				if (callback.IsCancelled())
					return Enumerable.Empty<ITeamBlockInfo>();

				runningTeamBlockCounter++;

				var blockPreferences = blockPreferenceProvider.ForAgents(teamBlockInfo.TeamInfo.GroupMembers,
					teamBlockInfo.BlockInfo.BlockPeriod.StartDate).ToArray();
				_blockPreferencesMapper.UpdateSchedulingOptionsFromExtraPreferences(schedulingOptions, blockPreferences);
				_blockPreferencesMapper.UpdateOptimizationPreferenceFromSchedulingOptions(optimizationPreferences, schedulingOptions);

				if (teamBlockInfo.AllIsLocked() || !_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
				{
					teamBlockToRemove.Add(teamBlockInfo);
					continue;
				}
				
				schedulePartModifyAndRollbackService.ClearModificationCollection();
				var datePoint = teamBlockInfo.BlockInfo.BlockPeriod.StartDate > selectedPeriod.StartDate ? 
					teamBlockInfo.BlockInfo.BlockPeriod.StartDate : 
					selectedPeriod.StartDate;
				var undoResCalcChanges = createRollbackState(skillDays, datePoint);
				var prefLimits = new PreferenceLimits();
				prefLimits.MeasureBefore(optimizationPreferences, teamBlockInfo, datePoint);
				
				//should probably be deleted, if so IDailyTargetValueCalculatorForTeamBlock can be deleted as well
				var previousTargetValue = 1d;
				if (teamBlockInfo.TeamInfo.GroupMembers.Count() > 1 || teamBlockInfo.BlockInfo.BlockPeriod.DayCount() > 1)
					previousTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);
				//

				_setMainShiftOptimizeActivitySpecificationForTeamBlock.Execute(optimizationPreferences, teamBlockInfo, schedulingOptions);

				//if needed due to perf, only fetch members in team that Agda has choosen. Also, make a dic based on date + agent
				var orgAssignmentsForTeamBlock = scheduleDictionary.SchedulesForPeriod(teamBlockInfo.BlockInfo.BlockPeriod, teamBlockInfo.TeamInfo.GroupMembers.ToArray())
					.Select(x => x.PersonAssignment()).Where(x => x != null).ToArray();
				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);
				
				var resCalcData = new ResourceCalculationData(scheduleDictionary, skillDays, skillStaffPeriodHolder, schedulingOptions.ConsiderShortBreaks, false);
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(orgAssignmentsForTeamBlock, new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, datePoint, schedulingOptions,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer, skillDays, scheduleDictionary, resCalcData, new ShiftNudgeDirective(), businessRuleCollection, _groupPersonSkillAggregator);

				callback.Optimizing(new OptimizationCallbackInfo(teamBlockInfo, success, totalTeamBlockInfos, runningTeamBlockCounter));

				if (!success ||
					!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences) ||
					!prefLimits.WithinLimit())
				{
					teamBlockToRemove.Add(teamBlockInfo);
					rollbackChanges(undoResCalcChanges, schedulePartModifyAndRollbackService);
					continue;
				}

				//should probably be deleted, if so IDailyTargetValueCalculatorForTeamBlock can be deleted as well
				var newTargetValue = -1d;
				if (teamBlockInfo.TeamInfo.GroupMembers.Count() > 1 || teamBlockInfo.BlockInfo.BlockPeriod.DayCount() > 1)
					newTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);
				//

				if (teamBlockInfo.TeamInfo.GroupMembers.Count() == 1 && teamBlockInfo.BlockInfo.BlockPeriod.DayCount() == 1)
					teamBlockToRemove.Add(teamBlockInfo);

				//should probably be deleted
				var isWorse = newTargetValue >= previousTargetValue;
				if (isWorse)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					rollbackChanges(undoResCalcChanges, schedulePartModifyAndRollbackService);
				}
				//
			}
			return teamBlockToRemove;
		}

		

		private static UndoRedoContainer createRollbackState(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnly datePoint)
		{
			var undoResCalcChanges = new UndoRedoContainer();
			undoResCalcChanges.FillWith(skillDays.FilterOnDates(new HashSet<DateOnly> {datePoint.AddDays(-1), datePoint, datePoint.AddDays(1)}));
			return undoResCalcChanges;
		}

		private static void rollbackChanges(UndoRedoContainer undoRedoContainer, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			schedulePartModifyAndRollbackService.Rollback();
			undoRedoContainer.UndoAll();
		}
	}
}
