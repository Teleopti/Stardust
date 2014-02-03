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

        public event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;

        public DayOffStep2(ISeniorityExtractor seniorityExtractor, ISeniorTeamBlockLocator seniorTeamBlockLocator, IJuniorTeamBlockExtractor juniorTeamBlockExtractor, 
                        ISuitableDayOffSpotDetector suitableDayOffSpotDetector, IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection, 
                                IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks, IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks)
        {
            _seniorityExtractor = seniorityExtractor;
            _seniorTeamBlockLocator = seniorTeamBlockLocator;
            _juniorTeamBlockExtractor = juniorTeamBlockExtractor;
            _suitableDayOffSpotDetector = suitableDayOffSpotDetector;
            _constructTeamBlock = constructTeamBlock;
            _filterForTeamBlockInSelection = filterForTeamBlockInSelection;
            _filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
            _filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
        }

        public void PerformStep2(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, 
                            IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, 
                                IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences)
        {
            _cancelMe = false;

            var teamBlockList = stepAConstructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            teamBlockList = stepBFilterOutUnwantedBlocks(teamBlockList, selectedPersons, selectedPeriod,scheduleDictionary);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlockList).ToList();
            var originalBlockCount = teamBlockPoints.Count;
            while (teamBlockPoints.Count > 0)
            {
                var mostSeniorTeamBlock = _seniorTeamBlockLocator.FindMostSeniorTeamBlock(teamBlockPoints);
                trySwapForTeamBlock(teamBlockList, mostSeniorTeamBlock, weekDayPoints,selectedPeriod,originalBlockCount ,teamBlockPoints.Count,rollbackService );
                teamBlockPoints.Remove(teamBlockPoints.Find(s => s.TeamBlockInfo.Equals(mostSeniorTeamBlock)));
            }

        }
        
        private void trySwapForTeamBlock(IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, IDictionary<DayOfWeek, int> weekDayPoints, DateOnlyPeriod selectedPeriod, 
                                    int originalBlocksCount, int remainingBlocksCount, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);
            var swappableTeamBlocksPoints = _seniorityExtractor.ExtractSeniority(swappableTeamBlocks).ToList();
            while (swappableTeamBlocksPoints.Count > 0)
            {
                var juniorTeamBlock = _juniorTeamBlockExtractor.GetJuniorTeamBlockInfo(swappableTeamBlocksPoints);
                handlePeriodForSelectedTeamBlocks(selectedPeriod, weekDayPoints, mostSeniorTeamBlock, juniorTeamBlock, originalBlocksCount, remainingBlocksCount, rollbackService);
                swappableTeamBlocksPoints.Remove(swappableTeamBlocksPoints.Find(s => s.TeamBlockInfo.Equals(juniorTeamBlock)));
            }
        }

       private void handlePeriodForSelectedTeamBlocks(DateOnlyPeriod selectedPeriod, IDictionary<DayOfWeek, int> weekDayPoints, ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, int originalBlocksCount,
                                int remainingBlocksCount, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var dayCollection = selectedPeriod.DayCollection();
            while (dayCollection.Count > 0)
            {
                var mostValuableSpot = _suitableDayOffSpotDetector.DetectMostValuableSpot(dayCollection, weekDayPoints);
                //need to think around this part how the swap and all the other stuff is posssible
                trySwapForMostSenior(mostSeniorTeamBlock, mostJuniorTeamBlock, mostValuableSpot, originalBlocksCount, remainingBlocksCount, rollbackService);
                dayCollection.Remove(mostValuableSpot);
            }
        }

        private void trySwapForMostSenior(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, DateOnly mostValuableSpot, int originalBlocksCount, int remainingBlocksCount,
                                ISchedulePartModifyAndRollbackService rollbackService)
        {
            //swapping code will come here
            throw new NotImplementedException();
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
    }
    
    
}
