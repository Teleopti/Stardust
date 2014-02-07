using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IDayOffStep2
    {
        void PerformStep2(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                                ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences);
        event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;
    }

    public class DayOffStep2:IDayOffStep2
    {
        private readonly ISeniorityExtractor _seniorityExtractor;
        private readonly ISeniorTeamBlockLocator  _seniorTeamBlockLocator;
        private readonly ISuitableDayOffSpotDetector _suitableDayOffSpotDetector;
        private bool _cancelMe;
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly IFilterForTeamBlockInSelection  _filterForTeamBlockInSelection;
        private readonly IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
        private readonly IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
        private readonly IJuniorTeamBlockExtractor _juniorTeamBlockExtractor;
        private ITeamBlockDayOffDaySwapper _teamBlockDayOffDaySwapper;
        private ISuitableDayOffsToGiveAway  _suitableDayOffsToGiveAway;

        public event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;

        public DayOffStep2(ISeniorityExtractor seniorityExtractor, ISeniorTeamBlockLocator seniorTeamBlockLocator, IJuniorTeamBlockExtractor juniorTeamBlockExtractor, 
                        ISuitableDayOffSpotDetector suitableDayOffSpotDetector, IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection, 
                                IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks, IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks, ITeamBlockDayOffDaySwapper teamBlockDayOffDaySwapper, ISuitableDayOffsToGiveAway suitableDayOffsToGiveAway)
        {
            _seniorityExtractor = seniorityExtractor;
            _seniorTeamBlockLocator = seniorTeamBlockLocator;
            _juniorTeamBlockExtractor = juniorTeamBlockExtractor;
            _suitableDayOffSpotDetector = suitableDayOffSpotDetector;
            _constructTeamBlock = constructTeamBlock;
            _filterForTeamBlockInSelection = filterForTeamBlockInSelection;
            _filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
            _filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
            _teamBlockDayOffDaySwapper = teamBlockDayOffDaySwapper;
            _suitableDayOffsToGiveAway = suitableDayOffsToGiveAway;
        }

        public void PerformStep2(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, 
                            IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, 
                                IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences)
        {
            _cancelMe = false;

            var teamBlocksToWorkWith = stepAConstructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            teamBlocksToWorkWith = stepBFilterOutUnwantedBlocks(teamBlocksToWorkWith, selectedPersons, selectedPeriod, scheduleDictionary);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith).ToList();
            var seniorityInfoDictionary = teamBlockPoints.ToDictionary(teamBlockPoint => teamBlockPoint.TeamBlockInfo, teamBlockPoint => teamBlockPoint);
            var originalBlockCount = teamBlockPoints.Count;
            while (teamBlockPoints.Count > 0)
            {
                var mostSeniorTeamBlock = _seniorTeamBlockLocator.FindMostSeniorTeamBlock(seniorityInfoDictionary.Values);
                teamBlocksToWorkWith = new List<ITeamBlockInfo>(seniorityInfoDictionary.Keys);
                trySwapForTeamBlock(teamBlocksToWorkWith, mostSeniorTeamBlock, weekDayPoints, selectedPeriod, originalBlockCount, seniorityInfoDictionary.Count, rollbackService,scheduleDictionary,optimizationPreferences);
                seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
            }

        }
        
        private void trySwapForTeamBlock(IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, IDictionary<DayOfWeek, int> weekDayPoints, DateOnlyPeriod selectedPeriod, int originalBlocksCount, int remainingBlocksCount, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);
            var swappableTeamBlocksPoints = _seniorityExtractor.ExtractSeniority(swappableTeamBlocks).ToList();
            while (swappableTeamBlocksPoints.Count > 0)
            {
                var juniorTeamBlock = _juniorTeamBlockExtractor.GetJuniorTeamBlockInfo(swappableTeamBlocksPoints);
                if(!juniorTeamBlock.Equals(mostSeniorTeamBlock) )
                    handlePeriodForSelectedTeamBlocks(selectedPeriod, weekDayPoints, mostSeniorTeamBlock, juniorTeamBlock, originalBlocksCount, remainingBlocksCount, rollbackService,scheduleDictionary,optimizationPreferences);
                swappableTeamBlocksPoints.Remove(swappableTeamBlocksPoints.Find(s => s.TeamBlockInfo.Equals(juniorTeamBlock)));
            }
        }

       private void handlePeriodForSelectedTeamBlocks(DateOnlyPeriod selectedPeriod, IDictionary<DayOfWeek, int> weekDayPoints, ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, int originalBlocksCount, int remainingBlocksCount, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
        {
            var dayCollection = selectedPeriod.DayCollection();
            var successfullSwap = false;
            while (dayCollection.Count > 0 && !_cancelMe && !successfullSwap)
            {
                var mostValuableSpot = _suitableDayOffSpotDetector.DetectMostValuableSpot(dayCollection, weekDayPoints);
                var daysToGiveAway = _suitableDayOffsToGiveAway.DetectMostValuableSpot(dayCollection, weekDayPoints);
                successfullSwap = trySwapForMostSenior(mostSeniorTeamBlock, mostJuniorTeamBlock, mostValuableSpot, rollbackService, daysToGiveAway,scheduleDictionary,optimizationPreferences);

                var message = "xxSwap successful";
                if (!successfullSwap)
                    message = "xxSwap not sucessful";

                double percentDone = 1 - (remainingBlocksCount / (double)originalBlocksCount);
                OnBlockSwapped(new ResourceOptimizerProgressEventArgs(1, 1, message + " for " + mostSeniorTeamBlock.TeamInfo.Name + " " + new Percent(percentDone).ToString() + " done "));

                dayCollection.Remove(mostValuableSpot);
            }
        }

        private bool  trySwapForMostSenior(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, DateOnly mostValuableSpot, ISchedulePartModifyAndRollbackService rollbackService, IList<DateOnly> daysToGiveAway, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
        {
            return _teamBlockDayOffDaySwapper.TrySwap(mostValuableSpot, mostSeniorTeamBlock, mostJuniorTeamBlock,
                                               rollbackService, scheduleDictionary, optimizationPreferences,
                                               daysToGiveAway);
        }

        private IList<ITeamBlockInfo> stepAConstructTeamBlock(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList,
                                                            DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            return _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, true, BlockFinderType.SchedulePeriod, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
        }

        private IList<ITeamBlockInfo> stepBFilterOutUnwantedBlocks(IList<ITeamBlockInfo> listOfAllTeamBlock, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, IScheduleDictionary scheduleDictionary)
        {
            var filteredList = _filterForTeamBlockInSelection.Filter(listOfAllTeamBlock, selectedPersons, selectedPeriod);
            filteredList = _filterForFullyScheduledBlocks.Filter(filteredList,scheduleDictionary);
            return filteredList;
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
    }
    
    
}
