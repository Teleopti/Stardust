

using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ISeniorityTeamBlockSwapperService
	{
		void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
		                             ISchedulingOptions schedulingOptions,
		                             IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, 
		                             IOptimizationPreferences optimizationPreferences, IDictionary<DayOfWeek, int> weekDayPoints);
	}

	public class SeniorityTeamBlockSwapperService : ISeniorityTeamBlockSwapperService
	{
		private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
		private readonly ISeniorityExtractor _seniorityExtractor;
		private readonly IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
		private readonly ITeamBlockSwapper _teambBlockSwapper;

		public SeniorityTeamBlockSwapperService(IConstructTeamBlock constructTeamBlock,
		                                        IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
		                                        IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks,
		                                        ISeniorityExtractor seniorityExtractor,
		                                        IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks,
		                                        ITeamBlockSwapper teambBlockSwapper)
		{
			_constructTeamBlock = constructTeamBlock;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
			_seniorityExtractor = seniorityExtractor;
			_filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
			_teambBlockSwapper = teambBlockSwapper;
		}

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                            ISchedulingOptions schedulingOptions,
                            IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, 
							IOptimizationPreferences optimizationPreferences, IDictionary<DayOfWeek, int> weekDayPoints)
		{
			if (optimizationPreferences.Extra.UseTeamBlockOption && !optimizationPreferences.Extra.KeepSameDaysOffInTeam)
				return;
			
			//Remove this as soon as possible
			schedulingOptions.UseTeamBlockPerOption = true;
			schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
			//refact to take correct params
			var allTeamBlocks = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
			var teamBlocksToWorkWith = _filterForTeamBlockInSelection.Filter(allTeamBlocks,
																		  selectedPersons, selectedPeriod);

			teamBlocksToWorkWith = _filterForFullyScheduledBlocks.IsFullyScheduled(teamBlocksToWorkWith, scheduleDictionary);
			var seniorityInfoDictionary = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith).ToDictionary(k => k.TeamBlockInfo, v => v.Points);

			while (seniorityInfoDictionary.Count > 0)
			{
				var mostSeniorTeamBlock = findMostSeniorTeamBlock(seniorityInfoDictionary);
				trySwapForMostSenior(weekDayPoints, teamBlocksToWorkWith, mostSeniorTeamBlock, rollbackService, scheduleDictionary);
				seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
			}
			
		}

		private void trySwapForMostSenior(IDictionary<DayOfWeek, int> weekDayPoints, IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary)
		{
			var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

			var seniorityValueDic = createSeniorityValueDictionary(teamBlocksToWorkWith, weekDayPoints);
			var seniorValue = seniorityValueDic[mostSeniorTeamBlock];

			var swapSuccess = false;
			while (swappableTeamBlocks.Count > 0 && !swapSuccess)
			{
				var blockToSwapWith = findBestTeamBlockToSwapWith(swappableTeamBlocks, seniorityValueDic);
				if (seniorValue < seniorityValueDic[blockToSwapWith])
				{
					bool success = _teambBlockSwapper.TrySwap(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
					                                           scheduleDictionary);
					swapSuccess = true;
				}

				swappableTeamBlocks.Remove(blockToSwapWith);
			}
		}

		private static ITeamBlockInfo findBestTeamBlockToSwapWith(IList<ITeamBlockInfo> swappableTeamBlocks, IDictionary<ITeamBlockInfo, double> seniorityValueDic)
		{
			var bestValue = double.MinValue;
			ITeamBlockInfo bestBlock = null;
			foreach (var swappableTeamBlock in swappableTeamBlocks)
			{
				var value = seniorityValueDic[swappableTeamBlock];
				if (value > bestValue)
				{
					bestValue = value;
					bestBlock = swappableTeamBlock;
				}
			}

			return bestBlock;
		}

		private static IDictionary<ITeamBlockInfo, double> createSeniorityValueDictionary(IList<ITeamBlockInfo> teamBlocksToWorkWith, IDictionary<DayOfWeek, int> weekDayPoints)
		{
			var retDic = new Dictionary<ITeamBlockInfo, double>();
			foreach (var teamBlockInfo in teamBlocksToWorkWith)
			{
				var senoirityValue = calculateDaysOffSeniorityValue(teamBlockInfo, weekDayPoints);
				retDic.Add(teamBlockInfo, senoirityValue);
			}

			return retDic;
		}

		private static double calculateDaysOffSeniorityValue(ITeamBlockInfo teamBlockInfo, IDictionary<DayOfWeek, int> weekDayPoints)
		{
			//all days off is the same for all team members
			var matrix = teamBlockInfo.MatrixesForGroupAndBlock().First();
			double totalPoints = 0;
			foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
			{
				if (scheduleDayPro.DaySchedulePart().SignificantPart() == SchedulePartView.DayOff)
				{
					var dayOfWeek = scheduleDayPro.Day.DayOfWeek;
					totalPoints += weekDayPoints[dayOfWeek];
				}
			}
			return totalPoints;
		}

		private static ITeamBlockInfo findMostSeniorTeamBlock(IDictionary<ITeamBlockInfo, double> seniorityInfoDictionary)
		{
			var maxValue = double.MinValue;
			ITeamBlockInfo mostSeniorBlock = null;
			foreach (var seniorityInfo in seniorityInfoDictionary)
			{
				if (seniorityInfo.Value > maxValue)
				{
					maxValue = seniorityInfo.Value;
					mostSeniorBlock = seniorityInfo.Key;
				}
			}
			return mostSeniorBlock;
		}
	}
}