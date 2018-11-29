using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IDayOffStep2
    {
        void PerformStep2(SchedulingOptions schedulingOptions, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, 
						ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, 
						int> weekDayPoints, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
        event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;
    }

    public class DayOffStep2:IDayOffStep2
    {
        private readonly ISeniorityExtractor _seniorityExtractor;
        private readonly ISeniorTeamBlockLocator  _seniorTeamBlockLocator;
        private readonly ISuitableDayOffSpotDetector _suitableDayOffSpotDetector;
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

        public void PerformStep2(SchedulingOptions schedulingOptions, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, 
								ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, 
								IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
        {
            var teamBlocksToWorkWith = stepAConstructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
			teamBlocksToWorkWith = stepBFilterOutUnwantedBlocks(teamBlocksToWorkWith, selectedPersons, selectedPeriod, scheduleDictionary);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith).ToList();
           
            var seniorityInfoDictionary = teamBlockPoints.ToDictionary(teamBlockPoint => teamBlockPoint.TeamBlockInfo, teamBlockPoint => teamBlockPoint);
            var originalBlockCount = teamBlockPoints.Count;
	        var cancelMe = false;
            while (seniorityInfoDictionary.Count > 0 && !cancelMe)
            {
				var mostSeniorTeamBlock = _seniorTeamBlockLocator.FindMostSeniorTeamBlock(seniorityInfoDictionary.Values);
                teamBlocksToWorkWith = new List<ITeamBlockInfo>(seniorityInfoDictionary.Keys);

				removeTeamBlockOfEqualSeniority(mostSeniorTeamBlock, teamBlocksToWorkWith, seniorityInfoDictionary);

				if(teamBlocksToWorkWith.Count > 1) trySwapForTeamBlock(teamBlocksToWorkWith, mostSeniorTeamBlock, weekDayPoints, selectedPeriod, originalBlockCount, 
													seniorityInfoDictionary.Count, rollbackService,scheduleDictionary,optimizationPreferences,
													()=>
													{
														cancelMe = true;
													},
													dayOffOptimizationPreferenceProvider);
                seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
            }
        }

		private void removeTeamBlockOfEqualSeniority(ITeamBlockInfo mostSeniorTeamBlockInfo, IList<ITeamBlockInfo> teamBlockInfos, IDictionary<ITeamBlockInfo, ITeamBlockPoints> infoDictionary)
	    {
			var mostSeniorSeniority = infoDictionary[mostSeniorTeamBlockInfo].Points;

			for (var i = teamBlockInfos.Count - 1; i >= 0; i--)
			{
				var juniorSeniority = infoDictionary[teamBlockInfos[i]].Points;
				if (mostSeniorSeniority.Equals(juniorSeniority) && !mostSeniorTeamBlockInfo.Equals(teamBlockInfos[i])) teamBlockInfos.RemoveAt(i);
			}   
	    }
        
        private void trySwapForTeamBlock(IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, IDictionary<DayOfWeek, int> weekDayPoints, DateOnlyPeriod selectedPeriod, int originalBlocksCount, 
                                    int remainingBlocksCount, ISchedulePartModifyAndRollbackService rollbackService, 
									IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences, 
									Action cancelAction, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);
            var swappableTeamBlocksPoints = _seniorityExtractor.ExtractSeniority(swappableTeamBlocks).ToList();
	        var cancel = false;
	        Action currentCancelAction = () =>
	        {
		        cancel = true;
		        cancelAction();
	        };
            while (!cancel && swappableTeamBlocksPoints.Count > 0)
            {
                var juniorTeamBlock = _juniorTeamBlockExtractor.GetJuniorTeamBlockInfo(swappableTeamBlocksPoints);
                var successfullSwap = false;
                if(!juniorTeamBlock.Equals(mostSeniorTeamBlock) )
                    successfullSwap = handlePeriodForSelectedTeamBlocks(selectedPeriod, weekDayPoints, mostSeniorTeamBlock, juniorTeamBlock, rollbackService,scheduleDictionary,
																		optimizationPreferences, dayOffOptimizationPreferenceProvider);
                swappableTeamBlocksPoints.Remove(swappableTeamBlocksPoints.Find(s => s.TeamBlockInfo.Equals(juniorTeamBlock)));
	            string message = !successfullSwap ? "Day off fairness Step2: Swap not successful" : "Day off fairness Step2: Swap successful";

	            double percentDone = 1 - (remainingBlocksCount / (double)originalBlocksCount);
                var progressResult = onBlockSwapped(new ResourceOptimizerProgressEventArgs(1, 1, message + " for " + mostSeniorTeamBlock.TeamInfo.Name + " " + new Percent(percentDone) + " done ", optimizationPreferences.Advanced.RefreshScreenInterval, currentCancelAction));
	            if (progressResult.ShouldCancel)
	            {
					currentCancelAction();
		            return;
	            }
            }
        }

       private bool  handlePeriodForSelectedTeamBlocks(DateOnlyPeriod selectedPeriod, IDictionary<DayOfWeek, int> weekDayPoints, ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, 
           ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
        {
           var dayCollection = selectedPeriod.DayCollection();
           var successfullSwap = false;
	       while (dayCollection.Count > 0)
	       {
		       var mostValuableSpot = _suitableDayOffSpotDetector.DetectMostValuableSpot(dayCollection, weekDayPoints);
		       var daysToGiveAway = _suitableDayOffsToGiveAway.DetectMostValuableSpot(dayCollection, weekDayPoints);
		       var swapResult = trySwapForMostSenior(mostSeniorTeamBlock, mostJuniorTeamBlock, mostValuableSpot,
			       rollbackService, daysToGiveAway, scheduleDictionary, optimizationPreferences, dayOffOptimizationPreferenceProvider);
		       if (swapResult) successfullSwap = true;
		       dayCollection.Remove(mostValuableSpot);
	       }
	       return successfullSwap;
        }

        private bool  trySwapForMostSenior(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo mostJuniorTeamBlock, DateOnly mostValuableSpot, ISchedulePartModifyAndRollbackService rollbackService, IList<DateOnly> daysToGiveAway, 
                                                IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
        {
            return _teamBlockDayOffDaySwapper.TrySwap(mostValuableSpot, mostSeniorTeamBlock, mostJuniorTeamBlock,
                                               rollbackService, scheduleDictionary, optimizationPreferences,
                                               daysToGiveAway, dayOffOptimizationPreferenceProvider);
        }

        private IList<ITeamBlockInfo> stepAConstructTeamBlock(SchedulingOptions schedulingOptions, IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
                                                            DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons)
        {
            return _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions.BlockFinder(), schedulingOptions.GroupOnGroupPageForTeamBlockPer);
        }

        private IList<ITeamBlockInfo> stepBFilterOutUnwantedBlocks(IEnumerable<ITeamBlockInfo> listOfAllTeamBlock, IEnumerable<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, IScheduleDictionary scheduleDictionary)
        {
			IList<ITeamBlockInfo> filteredList = listOfAllTeamBlock.Where(s => _teamBlockSeniorityValidator.ValidateSeniority(s)).ToList();
            filteredList = _filterForTeamBlockInSelection.Filter(filteredList, selectedPersons, selectedPeriod);
            filteredList = _filterForFullyScheduledBlocks.Filter(filteredList,scheduleDictionary);
            return filteredList;
        }

        private CancelSignal onBlockSwapped(ResourceOptimizerProgressEventArgs args)
        {
            EventHandler<ResourceOptimizerProgressEventArgs> handler = BlockSwapped;
            if (handler != null)
            {
                handler(this, args);
				if (args.Cancel)
				{
					_teamBlockDayOffDaySwapper.Cancel();
					return new CancelSignal { ShouldCancel = true };
				}
			}
            return new CancelSignal();
        }
    }
    
    
}
