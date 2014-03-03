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
        private readonly ITeamBlockDayOffDaySwapper _teamBlockDayOffDaySwapper;
        private readonly ISuitableDayOffsToGiveAway  _suitableDayOffsToGiveAway;
        private readonly ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;

        public event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;

        public DayOffStep2(ISeniorityExtractor seniorityExtractor, ISeniorTeamBlockLocator seniorTeamBlockLocator, IJuniorTeamBlockExtractor juniorTeamBlockExtractor, 
                        ISuitableDayOffSpotDetector suitableDayOffSpotDetector, IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection, 
                                IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks, IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks, ITeamBlockDayOffDaySwapper teamBlockDayOffDaySwapper,
                            ISuitableDayOffsToGiveAway suitableDayOffsToGiveAway, ITeamBlockSeniorityValidator teamBlockSeniorityValidator)
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
            _teamBlockSeniorityValidator = teamBlockSeniorityValidator;
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
            while (seniorityInfoDictionary.Count > 0 && !_cancelMe)
            {
                var mostSeniorTeamBlock = _seniorTeamBlockLocator.FindMostSeniorTeamBlock(seniorityInfoDictionary.Values);
                teamBlocksToWorkWith = new List<ITeamBlockInfo>(seniorityInfoDictionary.Keys);
                trySwapForTeamBlock(teamBlocksToWorkWith, mostSeniorTeamBlock, weekDayPoints, selectedPeriod, originalBlockCount, seniorityInfoDictionary.Count, rollbackService,scheduleDictionary,optimizationPreferences);
                seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
            }

        }
        
        private void trySwapForTeamBlock(IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, IDictionary<DayOfWeek, int> weekDayPoints, DateOnlyPeriod selectedPeriod, int originalBlocksCount, 
                                    int remainingBlocksCount, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);
            var swappableTeamBlocksPoints = _seniorityExtractor.ExtractSeniority(swappableTeamBlocks).ToList();
            while (swappableTeamBlocksPoints.Count > 0 && !_cancelMe)
            {
                var juniorTeamBlock = _juniorTeamBlockExtractor.GetJuniorTeamBlockInfo(swappableTeamBlocksPoints);
                var successfullSwap = false;
                if(!juniorTeamBlock.Equals(mostSeniorTeamBlock) )
                    successfullSwap = handlePeriodForSelectedTeamBlocks(selectedPeriod, weekDayPoints, mostSeniorTeamBlock, juniorTeamBlock, rollbackService,scheduleDictionary,optimizationPreferences);
                swappableTeamBlocksPoints.Remove(swappableTeamBlocksPoints.Find(s => s.TeamBlockInfo.Equals(juniorTeamBlock)));
                var message = "Day off fainess Step2: Swap not sucessful";
                if (successfullSwap)
                    message = "Day off fainess Step2: Swap sucessful";

                double percentDone = 1 - (remainingBlocksCount / (double)originalBlocksCount);
                OnBlockSwapped(new ResourceOptimizerProgressEventArgs(1, 1, message + " for " + mostSeniorTeamBlock.TeamInfo.Name + " " + new Percent(percentDone).ToString() + " done "));
            }
        }

       private bool  handlePeriodForSelectedTeamBlocks(DateOnlyPeriod selectedPeriod, IDictionary<DayOfWeek, int> weekDayPoints, ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, 
           ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
        {
           var dayCollection = selectedPeriod.DayCollection();
           var successfullSwap = false;
           while (dayCollection.Count > 0 && !_cancelMe )
            {
                var mostValuableSpot = _suitableDayOffSpotDetector.DetectMostValuableSpot(dayCollection, weekDayPoints);
                var daysToGiveAway = _suitableDayOffsToGiveAway.DetectMostValuableSpot(dayCollection, weekDayPoints);
                var swapResult = trySwapForMostSenior(mostSeniorTeamBlock, mostJuniorTeamBlock, mostValuableSpot, rollbackService, daysToGiveAway,scheduleDictionary,optimizationPreferences);
                if (swapResult) successfullSwap = true;
                dayCollection.Remove(mostValuableSpot);
            }
           return successfullSwap;
        }

        private bool  trySwapForMostSenior(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, DateOnly mostValuableSpot, ISchedulePartModifyAndRollbackService rollbackService, IList<DateOnly> daysToGiveAway, 
                                                IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
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
            IList<ITeamBlockInfo> filteredList = listOfAllTeamBlock.Where(_teamBlockSeniorityValidator.ValidateSeniority).ToList();
            filteredList = _filterForTeamBlockInSelection.Filter(filteredList, selectedPersons, selectedPeriod);
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
            if(_cancelMe )
                _teamBlockDayOffDaySwapper.Cancel();
        }
    }
    
    
}
