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
    public interface IDayOffStep1
    {
        void PerformStep1(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, 
			IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
        event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;
    }

    public class DayOffStep1 : IDayOffStep1
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
        private readonly IFilterForFullyScheduledBlocks  _filterForFullyScheduledBlocks;
        private readonly ISeniorityExtractor  _seniorityExtractor;
        private readonly ISeniorTeamBlockLocator _seniorTeamBlockLocator;
        private readonly IFilterOnSwapableTeamBlocks  _filterOnSwapableTeamBlocks;
        private readonly ISeniorityCalculatorForTeamBlock  _seniorityCalculatorForTeamBlock;
        private readonly ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHighestPoints;
        private readonly ISeniorityTeamBlockSwapper _seniorityTeamBlockSwapper;
	    private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
	    private readonly ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
	    private readonly IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
	    
        public DayOffStep1(IConstructTeamBlock constructTeamBlock,
	                       IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
	                       IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks,
	                       ISeniorityExtractor seniorityExtractor,
	                       ISeniorTeamBlockLocator seniorTeamBlockLocator,
	                       IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks,
	                       ISeniorityCalculatorForTeamBlock seniorityCalculatorForTeamBlock,
	                       ITeamBlockLocatorWithHighestPoints teamBlockLocatorWithHighestPoints,
	                       ISeniorityTeamBlockSwapper seniorityTeamBlockSwapper,
							ISchedulingOptionsCreator schedulingOptionsCreator,
							ITeamBlockSeniorityValidator teamBlockSeniorityValidator,
							IFilterForNoneLockedTeamBlocks filterForNoneLockedTeamBlocks)
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
		    _schedulingOptionsCreator = schedulingOptionsCreator;
		    _teamBlockSeniorityValidator = teamBlockSeniorityValidator;
	        _filterForNoneLockedTeamBlocks = filterForNoneLockedTeamBlocks;
	    }

	    public event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;

        public void PerformStep1(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, 
			IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
        {
	        var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
            var teamBlocksToWorkWith = constructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            teamBlocksToWorkWith = filterOutUnwantedBlocks(teamBlocksToWorkWith, selectedPersons, selectedPeriod, scheduleDictionary);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith).ToList();
            var seniorityInfoDictionary = teamBlockPoints.ToDictionary(teamBlockPoint => teamBlockPoint.TeamBlockInfo, teamBlockPoint => teamBlockPoint);
            var originalBlockCount = teamBlockPoints.Count;
	        var cancelMe = false;
            while (seniorityInfoDictionary.Count > 0 && !cancelMe)
            {
                var mostSeniorTeamBlock = _seniorTeamBlockLocator.FindMostSeniorTeamBlock(seniorityInfoDictionary.Values);
                teamBlocksToWorkWith = new List<ITeamBlockInfo>(seniorityInfoDictionary.Keys);

				removeTeamBlockOfEqualSeniority(mostSeniorTeamBlock, teamBlocksToWorkWith, seniorityInfoDictionary);

                trySwapForMostSenior(weekDayPoints, teamBlocksToWorkWith, mostSeniorTeamBlock, rollbackService, scheduleDictionary,
                                     optimizationPreferences, originalBlockCount, seniorityInfoDictionary.Count, ()=>
                                     {
	                                     cancelMe = true;
                                     },
									 dayOffOptimizationPreferenceProvider);

                seniorityInfoDictionary.Remove(mostSeniorTeamBlock);
            }
            
        }

	    private void removeTeamBlockOfEqualSeniority(ITeamBlockInfo mostSeniorTeamBlockInfo, IList<ITeamBlockInfo> teamBlockInfos, IDictionary<ITeamBlockInfo, ITeamBlockPoints> infoDictionary )
	    {
			var mostSeniorSeniority = infoDictionary[mostSeniorTeamBlockInfo].Points;

		    for (var i = teamBlockInfos.Count - 1; i >= 0; i--)
		    {
				var juniorSeniority = infoDictionary[teamBlockInfos[i]].Points;
				if(mostSeniorSeniority.Equals(juniorSeniority) && !mostSeniorTeamBlockInfo.Equals(teamBlockInfos[i])) teamBlockInfos.RemoveAt(i);
		    }
	    }

        private CancelSignal onBlockSwapped(ResourceOptimizerProgressEventArgs args)
        {
            var handler = BlockSwapped;
            if (handler != null)
            {
				handler(this, args);
	            if (args.Cancel) return new CancelSignal {ShouldCancel = true};
            }
			return new CancelSignal();
        }

	    private void trySwapForMostSenior(IDictionary<DayOfWeek, int> weekDayPoints,
		    IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock,
		    ISchedulePartModifyAndRollbackService rollbackService,
		    IScheduleDictionary scheduleDictionary,
		    IOptimizationPreferences optimizationPreferences, int originalBlocksCount,
		    int remainingBlocksCount,
		    Action cancelAction,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
	    {
		    var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

		    var dayOffPlacementPointsSenerioty =
			    _seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith, weekDayPoints);
		    var seniorValue = dayOffPlacementPointsSenerioty[mostSeniorTeamBlock];

		    var swapSuccess = false;
		    while (swappableTeamBlocks.Count > 0 && !swapSuccess)
		    {
			    var blockToSwapWith = _teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(swappableTeamBlocks,
				    dayOffPlacementPointsSenerioty);
			    if (seniorValue < dayOffPlacementPointsSenerioty[blockToSwapWith])
			    {
				    swapSuccess = _seniorityTeamBlockSwapper.SwapAndValidate(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
					    scheduleDictionary, optimizationPreferences, dayOffOptimizationPreferenceProvider);
			    }
			    else
			    {
				    swappableTeamBlocks.Clear();
			    }

			    swappableTeamBlocks.Remove(blockToSwapWith);

			    string message = swapSuccess
				    ? "Day off fairness Step1: Swap successful"
				    : "Day off fairness Step1: Swap not successful";
			    double percentDone = 1 - (remainingBlocksCount/(double) originalBlocksCount);
			    var progressResult =
				    onBlockSwapped(new ResourceOptimizerProgressEventArgs(1, 1,
					    message + " for " + mostSeniorTeamBlock.TeamInfo.Name + " " + new Percent(percentDone) + " done ", optimizationPreferences.Advanced.RefreshScreenInterval, cancelAction));
			    if (progressResult.ShouldCancel)
			    {
				    cancelAction();
				    return;
			    }
		    }
	    }

	    private IList<ITeamBlockInfo> filterOutUnwantedBlocks(IEnumerable<ITeamBlockInfo> listOfAllTeamBlock, IEnumerable<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, IScheduleDictionary scheduleDictionary)
        {
			IList<ITeamBlockInfo> filteredList = listOfAllTeamBlock.Where(s => _teamBlockSeniorityValidator.ValidateSeniority(s)).ToList();
			filteredList = _filterForTeamBlockInSelection.Filter(filteredList, selectedPersons, selectedPeriod);
            filteredList = _filterForFullyScheduledBlocks.Filter(filteredList, scheduleDictionary );
			filteredList = _filterForNoneLockedTeamBlocks.Filter(filteredList);		
            return filteredList;
        }

        private IList<ITeamBlockInfo> constructTeamBlock(SchedulingOptions schedulingOptions, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, 
                                                            DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons)
        {
            return _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions.BlockFinder(), schedulingOptions.GroupOnGroupPageForTeamBlockPer);
        }
    }
}
