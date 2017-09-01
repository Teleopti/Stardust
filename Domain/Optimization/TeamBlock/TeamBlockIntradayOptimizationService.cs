using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	[RemoveMeWithToggle("merge into base class", Toggles.ResourcePlanner_SpeedUpShiftsWithinDay_45694)]
	public class TeamBlockIntradayOptimizationServiceFewerResCalcAtDelete : TeamBlockIntradayOptimizationService
	{
		private readonly IResourceCalculateAfterDeleteDecider _resouceCalculateAfterDeleteDecider;

		public TeamBlockIntradayOptimizationServiceFewerResCalcAtDelete(IResourceCalculateAfterDeleteDecider resouceCalculateAfterDeleteDecider, 
			TeamBlockScheduler teamBlockScheduler, ISchedulingOptionsCreator schedulingOptionsCreator, ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, TeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker, TeamBlockClearer teamBlockClearer, IDailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock, ITeamBlockSteadyStateValidator teamTeamBlockSteadyStateValidator, ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator, IWorkShiftSelector workShiftSelector, IGroupPersonSkillAggregator groupPersonSkillAggregator, SetMainShiftOptimizeActivitySpecificationForTeamBlock setMainShiftOptimizeActivitySpecificationForTeamBlock, IOptimizerHelperHelper optimizerHelperHelper, ICurrentIntradayOptimizationCallback currentIntradayOptimizationCallback) : base(teamBlockScheduler, schedulingOptionsCreator, safeRollbackAndResourceCalculation, teamBlockIntradayDecisionMaker, teamBlockClearer, dailyTargetValueCalculatorForTeamBlock, teamTeamBlockSteadyStateValidator, teamBlockShiftCategoryLimitationValidator, workShiftSelector, groupPersonSkillAggregator, setMainShiftOptimizeActivitySpecificationForTeamBlock, optimizerHelperHelper, currentIntradayOptimizationCallback)
		{
			_resouceCalculateAfterDeleteDecider = resouceCalculateAfterDeleteDecider;
		}

		protected override void ClearTeamBlock(SchedulingOptions schedulingOptions,ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ITeamBlockInfo teamBlockInfo)
		{
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo, _resouceCalculateAfterDeleteDecider);
		}
	}

	public class TeamBlockIntradayOptimizationService
	{
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly TeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_SpeedUpShiftsWithinDay_45694)]
		protected readonly TeamBlockClearer _teamBlockClearer;
		private readonly IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private readonly ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly SetMainShiftOptimizeActivitySpecificationForTeamBlock _setMainShiftOptimizeActivitySpecificationForTeamBlock;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly ICurrentIntradayOptimizationCallback _currentIntradayOptimizationCallback;

		public TeamBlockIntradayOptimizationService(TeamBlockScheduler teamBlockScheduler,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			TeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
			TeamBlockClearer teamBlockClearer,
			IDailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock,
			ITeamBlockSteadyStateValidator teamTeamBlockSteadyStateValidator,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			SetMainShiftOptimizeActivitySpecificationForTeamBlock setMainShiftOptimizeActivitySpecificationForTeamBlock,
			IOptimizerHelperHelper optimizerHelperHelper,
			ICurrentIntradayOptimizationCallback currentIntradayOptimizationCallback)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_dailyTargetValueCalculatorForTeamBlock = dailyTargetValueCalculatorForTeamBlock;
			_teamTeamBlockSteadyStateValidator = teamTeamBlockSteadyStateValidator;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_setMainShiftOptimizeActivitySpecificationForTeamBlock = setMainShiftOptimizeActivitySpecificationForTeamBlock;
			_optimizerHelperHelper = optimizerHelperHelper;
			_currentIntradayOptimizationCallback = currentIntradayOptimizationCallback;
		}

		public void Optimize(IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			IScheduleDictionary scheduleDictionary,
			IEnumerable<IPerson> personsInOrganization,
			INewBusinessRuleCollection businessRuleCollection,
			ITeamBlockGenerator teamBlockGenerator)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			_optimizerHelperHelper.LockDaysForIntradayOptimization(allPersonMatrixList, selectedPeriod);
			var teamBlocks = teamBlockGenerator.Generate(personsInOrganization, allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			
			while (teamBlocks.Count > 0)
			{
				if (_currentIntradayOptimizationCallback.Current().IsCancelled())
					return;

				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, optimizationPreferences,
					schedulingOptions, teamBlocks,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer,
					skillDays, scheduleDictionary, businessRuleCollection);
				foreach (var teamBlock in teamBlocksToRemove)
				{
					teamBlocks.Remove(teamBlock);
				}
			}
		}


		private IEnumerable<ITeamBlockInfo> optimizeOneRound(DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, SchedulingOptions schedulingOptions,
			IList<ITeamBlockInfo> allTeamBlockInfos, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection businessRuleCollection)
		{
			var teamBlockToRemove = new List<ITeamBlockInfo>();
			var callback = _currentIntradayOptimizationCallback.Current();

			var sortedTeamBlockInfos = _teamBlockIntradayDecisionMaker.Decide(allTeamBlockInfos, schedulingOptions);

			int totalTeamBlockInfos = sortedTeamBlockInfos.Count;
			int runningTeamBlockCounter = 0;
			foreach (var teamBlockInfo in sortedTeamBlockInfos)
			{
				if (callback.IsCancelled())
					return Enumerable.Empty<ITeamBlockInfo>();

				runningTeamBlockCounter++;

				if (teamBlockInfo.AllIsLocked() || !_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
				{
					teamBlockToRemove.Add(teamBlockInfo);
					continue;
				}
				
				schedulePartModifyAndRollbackService.ClearModificationCollection();

				//should probably be deleted, if so IDailyTargetValueCalculatorForTeamBlock can be deleted as well
				var previousTargetValue = 1d;
				if (teamBlockInfo.TeamInfo.GroupMembers.Count() > 1 || teamBlockInfo.BlockInfo.BlockPeriod.DayCount() > 1)
					previousTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);
				//

				_setMainShiftOptimizeActivitySpecificationForTeamBlock.Execute(optimizationPreferences, teamBlockInfo, schedulingOptions);

				//if needed due to perf, only fetch members in team that Agda has choosen. Also, make a dic based on date + agent
				var orgAssignmentsForTeamBlock = scheduleDictionary.SchedulesForPeriod(teamBlockInfo.BlockInfo.BlockPeriod, teamBlockInfo.TeamInfo.GroupMembers.ToArray())
					.Select(x => x.PersonAssignment()).Where(x => x != null).ToArray();
				ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);
				var firstSelectedDay = selectedPeriod.StartDate;
				var datePoint = firstSelectedDay;
				if (teamBlockInfo.BlockInfo.BlockPeriod.StartDate > firstSelectedDay)
					datePoint = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;

				var success = _teamBlockScheduler.ScheduleTeamBlockDay(orgAssignmentsForTeamBlock, new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, datePoint, schedulingOptions,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer, skillDays.ToSkillDayEnumerable(), scheduleDictionary, new ShiftNudgeDirective(), businessRuleCollection, _groupPersonSkillAggregator);

				callback.Optimizing(new IntradayOptimizationCallbackInfo(teamBlockInfo, success, totalTeamBlockInfos - runningTeamBlockCounter));

				if (!success)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					continue;
				}

				if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences))
				{
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
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
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				}
				//
			}
			return teamBlockToRemove;
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpShiftsWithinDay_45694)]
		protected virtual void ClearTeamBlock(SchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ITeamBlockInfo teamBlockInfo)
		{
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo, new AlwaysResourceCalculateAfterDelete());
		}
	}
}
