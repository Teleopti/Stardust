﻿

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
		private readonly ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHighestPoints;
		private readonly IWeekDayPointCalculatorForTeamBlock _weekDayPointCalculatorForTeamBlock;
		private readonly ISeniorityTeamBlockSwapper _seniorityTeamBlockSwapper;

		public SeniorityTeamBlockSwapperService(IConstructTeamBlock constructTeamBlock,
		                                        IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
		                                        IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks,
		                                        ISeniorityExtractor seniorityExtractor,
		                                        IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks,
												ITeamBlockLocatorWithHighestPoints teamBlockLocatorWithHighestPoints,
												IWeekDayPointCalculatorForTeamBlock weekDayPointCalculatorForTeamBlock,
												ISeniorityTeamBlockSwapper seniorityTeamBlockSwapper)
		{
			_constructTeamBlock = constructTeamBlock;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
			_seniorityExtractor = seniorityExtractor;
			_filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
			_teamBlockLocatorWithHighestPoints = teamBlockLocatorWithHighestPoints;
			_weekDayPointCalculatorForTeamBlock = weekDayPointCalculatorForTeamBlock;
			_seniorityTeamBlockSwapper = seniorityTeamBlockSwapper;
		}

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                            ISchedulingOptions schedulingOptions,
                            IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, 
							IOptimizationPreferences optimizationPreferences, IDictionary<DayOfWeek, int> weekDayPoints)
		{

			var allTeamBlocks = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, true,
			                                                  BlockFinderType.SchedulePeriod,
			                                                  schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var teamBlocksToWorkWith = _filterForTeamBlockInSelection.Filter(allTeamBlocks,
																		  selectedPersons, selectedPeriod);

			teamBlocksToWorkWith = _filterForFullyScheduledBlocks.IsFullyScheduled(teamBlocksToWorkWith, scheduleDictionary);
			var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith);
			var seniorityInfoDictionary = teamBlockPoints.ToDictionary(teamBlockPoint => teamBlockPoint.TeamBlockInfo, teamBlockPoint => teamBlockPoint.Points);

			while (seniorityInfoDictionary.Count > 0)
			{
				var mostSeniorTeamBlock = findMostSeniorTeamBlock(seniorityInfoDictionary);
				trySwapForMostSenior(weekDayPoints, teamBlocksToWorkWith, mostSeniorTeamBlock, rollbackService, scheduleDictionary, optimizationPreferences);
				seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
			}
			
		}

		private void trySwapForMostSenior(IDictionary<DayOfWeek, int> weekDayPoints, IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
		{
			var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

			var seniorityValueDic = createSeniorityValueDictionary(teamBlocksToWorkWith, weekDayPoints);
			var seniorValue = seniorityValueDic[mostSeniorTeamBlock];

			var swapSuccess = false;
			while (swappableTeamBlocks.Count > 0 && !swapSuccess)
			{
				var blockToSwapWith = _teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(swappableTeamBlocks,
				                                                                                     seniorityValueDic);
				if (seniorValue < seniorityValueDic[blockToSwapWith])
				{
					swapSuccess = _seniorityTeamBlockSwapper.SwapAndValidate(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
					                                                         scheduleDictionary, optimizationPreferences);
					if(!swapSuccess)
						rollbackService.Rollback();
				}

				swappableTeamBlocks.Remove(blockToSwapWith);
			}
		}

		private  IDictionary<ITeamBlockInfo, double> createSeniorityValueDictionary(IEnumerable<ITeamBlockInfo> teamBlocksToWorkWith, IDictionary<DayOfWeek, int> weekDayPoints)
		{
			var retDic = new Dictionary<ITeamBlockInfo, double>();
			foreach (var teamBlockInfo in teamBlocksToWorkWith)
			{
				var senoirityValue = _weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(teamBlockInfo, weekDayPoints);
				retDic.Add(teamBlockInfo, senoirityValue);
			}

			return retDic;
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