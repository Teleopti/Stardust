﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IDayOffStep1
    {
        void PerformStep1(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                            ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences, 
                                    ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator);
        event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;
    }

    public class DayOffStep1 : IDayOffStep1
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
        private readonly IFilterForFullyScheduledBlocks  _filterForFullyScheduledBlocks;
        private readonly ISeniorityExtractor  _seniorityExtractor;
        private readonly ISeniorTeamBlockLocator _seniorTeamBlockLocator;
        private bool _cancelMe;
        private readonly IFilterOnSwapableTeamBlocks  _filterOnSwapableTeamBlocks;
        private readonly ISeniorityCalculatorForTeamBlock  _seniorityCalculatorForTeamBlock;
        private readonly ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHighestPoints;
        private readonly ISeniorityTeamBlockSwapper _seniorityTeamBlockSwapper;

        public DayOffStep1( IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
                                IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks, ISeniorityExtractor seniorityExtractor, 
                                    ISeniorTeamBlockLocator seniorTeamBlockLocator, IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks, ISeniorityCalculatorForTeamBlock seniorityCalculatorForTeamBlock, 
                                    ITeamBlockLocatorWithHighestPoints teamBlockLocatorWithHighestPoints, ISeniorityTeamBlockSwapper seniorityTeamBlockSwapper)
        {
            _constructTeamBlock = constructTeamBlock;
            _filterForTeamBlockInSelection = filterForTeamBlockInSelection;
            _filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
            _seniorityExtractor = seniorityExtractor;
            _seniorTeamBlockLocator = seniorTeamBlockLocator;
            _filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
            _seniorityCalculatorForTeamBlock = seniorityCalculatorForTeamBlock;
            _teamBlockLocatorWithHighestPoints = teamBlockLocatorWithHighestPoints;
            _seniorityTeamBlockSwapper = seniorityTeamBlockSwapper;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;

        public void PerformStep1(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                                ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences,
                                    ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
        {
            _cancelMe = false;

            var teamBlocksToWorkWith = stepAConstructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            teamBlocksToWorkWith = stepBFilterOutUnwantedBlocks(teamBlocksToWorkWith, selectedPersons, selectedPeriod, scheduleDictionary);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith).ToList();
            var seniorityInfoDictionary = teamBlockPoints.ToDictionary(teamBlockPoint => teamBlockPoint.TeamBlockInfo, teamBlockPoint => teamBlockPoint);
            var originalBlockCount = teamBlockPoints.Count;
            while (seniorityInfoDictionary.Count > 0 && !_cancelMe)
            {
                var mostSeniorTeamBlock = _seniorTeamBlockLocator.FindMostSeniorTeamBlock(seniorityInfoDictionary.Values);
                teamBlocksToWorkWith = new List<ITeamBlockInfo>(seniorityInfoDictionary.Keys);
                trySwapForMostSenior(weekDayPoints, teamBlocksToWorkWith, mostSeniorTeamBlock, rollbackService, scheduleDictionary,
                                     optimizationPreferences, originalBlockCount, seniorityInfoDictionary.Count, teamBlockRestrictionOverLimitValidator);

                seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
            }
            
        }
        protected virtual void OnBlockSwapped(ResourceOptimizerProgressEventArgs eventArgs)
        {
            EventHandler<ResourceOptimizerProgressEventArgs> temp = BlockSwapped;
            if (temp != null)
            {
                temp(this, eventArgs);
            }
            _cancelMe = eventArgs.Cancel;
        }
        private void trySwapForMostSenior(IDictionary<DayOfWeek, int> weekDayPoints,
                                          IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock,
                                          ISchedulePartModifyAndRollbackService rollbackService,
                                          IScheduleDictionary scheduleDictionary,
                                          IOptimizationPreferences optimizationPreferences, int originalBlocksCount,
                                          int remainingBlocksCount, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

            var dayOffPlacementPointsSenerioty = _seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith, weekDayPoints);
            var seniorValue = dayOffPlacementPointsSenerioty[mostSeniorTeamBlock];

            var swapSuccess = false;
            while (swappableTeamBlocks.Count > 0 && !swapSuccess && !_cancelMe)
            {
                var blockToSwapWith = _teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(swappableTeamBlocks,
                                                                                                     dayOffPlacementPointsSenerioty);
                if (seniorValue < dayOffPlacementPointsSenerioty[blockToSwapWith])
                {
                    swapSuccess = _seniorityTeamBlockSwapper.SwapAndValidate(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
                                                                             scheduleDictionary, optimizationPreferences,teamBlockRestrictionOverLimitValidator);
                }
				else
				{
					swappableTeamBlocks.Clear();
				}

                swappableTeamBlocks.Remove(blockToSwapWith);

                var message = "xxSwap successful";
                if (!swapSuccess)
                    message = "xxSwap not sucessful";

                double percentDone = 1 - (remainingBlocksCount / (double)originalBlocksCount);
                OnBlockSwapped(new ResourceOptimizerProgressEventArgs(1, 1, message + " for " + mostSeniorTeamBlock.TeamInfo.Name + " " + new Percent(percentDone).ToString() + " done "));
            }
        }

        private IList<ITeamBlockInfo> stepBFilterOutUnwantedBlocks(IList<ITeamBlockInfo> listOfAllTeamBlock, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, IScheduleDictionary scheduleDictionary)
        {
            var filteredList =  _filterForTeamBlockInSelection.Filter(listOfAllTeamBlock, selectedPersons, selectedPeriod);
            filteredList = _filterForFullyScheduledBlocks.Filter(filteredList, scheduleDictionary );
            return filteredList;
        }

        private IList<ITeamBlockInfo> stepAConstructTeamBlock(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, 
                                                            DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            return _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, true, BlockFinderType.SchedulePeriod,schedulingOptions.GroupOnGroupPageForTeamBlockPer);
        }

    }
}
