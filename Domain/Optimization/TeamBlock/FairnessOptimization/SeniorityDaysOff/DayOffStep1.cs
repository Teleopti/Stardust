using System;
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
        void PerformStep1(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences, bool schedulerSeniority11111);
        event EventHandler<ResourceOptimizerProgressEventArgs> BlockSwapped;
        void OnBlockSwapped(ResourceOptimizerProgressEventArgs eventArgs);
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
	    private bool _cancelMe;
		private ResourceOptimizerProgressEventArgs _progressEvent;

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

        public void PerformStep1(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints, IOptimizationPreferences optimizationPreferences, bool schedulerSeniority11111)
        {
            _cancelMe = false;
	        _progressEvent = null;

	        var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
            var teamBlocksToWorkWith = constructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            teamBlocksToWorkWith = filterOutUnwantedBlocks(teamBlocksToWorkWith, selectedPersons, selectedPeriod, scheduleDictionary, schedulerSeniority11111);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlocksToWorkWith).ToList();
            var seniorityInfoDictionary = teamBlockPoints.ToDictionary(teamBlockPoint => teamBlockPoint.TeamBlockInfo, teamBlockPoint => teamBlockPoint);
            var originalBlockCount = teamBlockPoints.Count;
            while (seniorityInfoDictionary.Count > 0 && !_cancelMe)
            {
				if (_progressEvent != null && _progressEvent.UserCancel)
					break;

                var mostSeniorTeamBlock = _seniorTeamBlockLocator.FindMostSeniorTeamBlock(seniorityInfoDictionary.Values);
                teamBlocksToWorkWith = new List<ITeamBlockInfo>(seniorityInfoDictionary.Keys);

				removeTeamBlockOfEqualSeniority(mostSeniorTeamBlock, teamBlocksToWorkWith, seniorityInfoDictionary);

                trySwapForMostSenior(weekDayPoints, teamBlocksToWorkWith, mostSeniorTeamBlock, rollbackService, scheduleDictionary,
                                     optimizationPreferences, originalBlockCount, seniorityInfoDictionary.Count);

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

        public virtual void OnBlockSwapped(ResourceOptimizerProgressEventArgs eventArgs)
        {
            EventHandler<ResourceOptimizerProgressEventArgs> temp = BlockSwapped;
            if (temp != null)
            {
                temp(this, eventArgs);
            }
            if(eventArgs.Cancel )
                _cancelMe = true;

			if (_progressEvent != null && _progressEvent.UserCancel) return;
			_progressEvent = eventArgs;
        }
        private void trySwapForMostSenior(IDictionary<DayOfWeek, int> weekDayPoints,
                                          IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock,
                                          ISchedulePartModifyAndRollbackService rollbackService,
                                          IScheduleDictionary scheduleDictionary,
                                          IOptimizationPreferences optimizationPreferences, int originalBlocksCount,
                                          int remainingBlocksCount)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

            var dayOffPlacementPointsSenerioty = _seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith, weekDayPoints);
            var seniorValue = dayOffPlacementPointsSenerioty[mostSeniorTeamBlock];

            var swapSuccess = false;
            while (swappableTeamBlocks.Count > 0 && !swapSuccess && !_cancelMe)
            {
	            if (_progressEvent != null && _progressEvent.UserCancel)
		            break;

                var blockToSwapWith = _teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(swappableTeamBlocks,
                                                                                                     dayOffPlacementPointsSenerioty);
                if (seniorValue < dayOffPlacementPointsSenerioty[blockToSwapWith])
                {
                    swapSuccess = _seniorityTeamBlockSwapper.SwapAndValidate(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
                                                                             scheduleDictionary, optimizationPreferences);
                }
				else
				{
					swappableTeamBlocks.Clear();
				}

                swappableTeamBlocks.Remove(blockToSwapWith);

                var message = "Day off fainess Step1: Swap successful";
                if (!swapSuccess)
                    message = "Day off fainess Step1: Swap not sucessful";

                double percentDone = 1 - (remainingBlocksCount / (double)originalBlocksCount);
                OnBlockSwapped(new ResourceOptimizerProgressEventArgs(1, 1, message + " for " + mostSeniorTeamBlock.TeamInfo.Name + " " + new Percent(percentDone).ToString() + " done "));
            }
        }

        private IList<ITeamBlockInfo> filterOutUnwantedBlocks(IEnumerable<ITeamBlockInfo> listOfAllTeamBlock, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, IScheduleDictionary scheduleDictionary, bool schedulerSeniority11111)
        {
			IList<ITeamBlockInfo> filteredList = listOfAllTeamBlock.Where(s => _teamBlockSeniorityValidator.ValidateSeniority(s, schedulerSeniority11111)).ToList();
			filteredList = _filterForTeamBlockInSelection.Filter(filteredList, selectedPersons, selectedPeriod);
            filteredList = _filterForFullyScheduledBlocks.Filter(filteredList, scheduleDictionary );
			filteredList = _filterForNoneLockedTeamBlocks.Filter(filteredList);		
            return filteredList;
        }

        private IList<ITeamBlockInfo> constructTeamBlock(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, 
                                                            DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            return _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, BlockFinderType.SchedulePeriod,schedulingOptions.GroupOnGroupPageForTeamBlockPer);
        }

    }
}
