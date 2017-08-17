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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockIntradayOptimizationService
	{
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockClearer _teamBlockClearer;
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
			ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
			ITeamBlockClearer teamBlockClearer,
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
			var remainingInfoList = new List<ITeamBlockInfo>(teamBlocks);

			while (remainingInfoList.Count > 0)
			{
				if (_currentIntradayOptimizationCallback.Current().IsCancelled())
					return;

				var teamBlocksToRemove = optimizeOneRound(selectedPeriod, optimizationPreferences,
					schedulingOptions, remainingInfoList,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer,
					skillDays, scheduleDictionary, businessRuleCollection);
				foreach (var teamBlock in teamBlocksToRemove)
				{
					remainingInfoList.Remove(teamBlock);
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

			var sortedTeamBlockInfos = _teamBlockIntradayDecisionMaker.Decide(allTeamBlockInfos, optimizationPreferences,
				schedulingOptions);

			int totalTeamBlockInfos = sortedTeamBlockInfos.Count;
			int runningTeamBlockCounter = 0;
			foreach (var teamBlockInfo in sortedTeamBlockInfos)
			{
				if (callback.IsCancelled())
					return Enumerable.Empty<ITeamBlockInfo>();

				if (!_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
				{
					teamBlockToRemove.Add(teamBlockInfo);
					continue;
				}

				runningTeamBlockCounter++;
				
				schedulePartModifyAndRollbackService.ClearModificationCollection();

				var previousTargetValue = 1d;
				if (teamBlockInfo.TeamInfo.GroupMembers.Count() > 1 || teamBlockInfo.BlockInfo.BlockPeriod.DayCount() > 1)
					previousTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);

				_setMainShiftOptimizeActivitySpecificationForTeamBlock.Execute(optimizationPreferences, teamBlockInfo, schedulingOptions);

				_teamBlockClearer.ClearTeamBlock(schedulingOptions, schedulePartModifyAndRollbackService, teamBlockInfo);
				var firstSelectedDay = selectedPeriod.StartDate;
				var datePoint = firstSelectedDay;
				if (teamBlockInfo.BlockInfo.BlockPeriod.StartDate > firstSelectedDay)
					datePoint = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;

				var success = _teamBlockScheduler.ScheduleTeamBlockDay(new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, datePoint, schedulingOptions,
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

				var newTargetValue = -1d;
				if (teamBlockInfo.TeamInfo.GroupMembers.Count() > 1 || teamBlockInfo.BlockInfo.BlockPeriod.DayCount() > 1)
					newTargetValue = _dailyTargetValueCalculatorForTeamBlock.TargetValue(teamBlockInfo, optimizationPreferences.Advanced);

				if (teamBlockInfo.TeamInfo.GroupMembers.Count() == 1 && teamBlockInfo.BlockInfo.BlockPeriod.DayCount() == 1)
					teamBlockToRemove.Add(teamBlockInfo);

				var isWorse = newTargetValue >= previousTargetValue;
				if (isWorse)
				{
					teamBlockToRemove.Add(teamBlockInfo);
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
				}
			}
			return teamBlockToRemove;
		}
	}
}
