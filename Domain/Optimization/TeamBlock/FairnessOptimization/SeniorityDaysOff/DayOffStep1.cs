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
        void PerformStep1(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList,
                          DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                          ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary,
                          IDictionary<DayOfWeek, int> weekDayPoints);
    }

    public class DayOffStep1 : IDayOffStep1
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
        private readonly IFilterForTeamBlockInSelection _filterForFullyScheduledBlocks;
        private readonly ISeniorityExtractor  _seniorityExtractor;
        private readonly ISeniorTeamBlockLocator _seniorTeamBlockLocator;
        private readonly IDescisionMakerToSwapTeamBlock  _descisionMakerToSwapTeamBlock;

        public DayOffStep1( IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection, IFilterForTeamBlockInSelection filterForFullyScheduledBlocks, ISeniorityExtractor seniorityExtractor, ISeniorTeamBlockLocator seniorTeamBlockLocator, IDescisionMakerToSwapTeamBlock descisionMakerToSwapTeamBlock)
        {
            _constructTeamBlock = constructTeamBlock;
            _filterForTeamBlockInSelection = filterForTeamBlockInSelection;
            _filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
            _seniorityExtractor = seniorityExtractor;
            _seniorTeamBlockLocator = seniorTeamBlockLocator;
            _descisionMakerToSwapTeamBlock = descisionMakerToSwapTeamBlock;
        }

        public void PerformStep1(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints)
        {
            var teamBlockList = stepAConstructTeamBlock(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons);
            teamBlockList = stepBFilterOutUnwantedBlocks(teamBlockList, selectedPersons, selectedPeriod);

            var teamBlockPoints = _seniorityExtractor.ExtractSeniority(teamBlockList).ToList();
            while (teamBlockPoints.Count > 0)
            {
                var mostSeniorTeamBlock =_seniorTeamBlockLocator.FindMostSeniorTeamBlock(teamBlockPoints);
                _descisionMakerToSwapTeamBlock.TrySwapForMostSenior( teamBlockList, mostSeniorTeamBlock, rollbackService, scheduleDictionary,weekDayPoints);
                teamBlockPoints.Remove(teamBlockPoints.Find( s=>s.TeamBlockInfo.Equals( mostSeniorTeamBlock  )));
            }
            
        }

        private IList<ITeamBlockInfo> stepBFilterOutUnwantedBlocks(IList<ITeamBlockInfo> listOfAllTeamBlock, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod)
        {
            var filteredList =  _filterForTeamBlockInSelection.Filter(listOfAllTeamBlock, selectedPersons, selectedPeriod);
            filteredList = _filterForFullyScheduledBlocks.Filter(filteredList, selectedPersons, selectedPeriod);
            return filteredList;
        }

        private IList<ITeamBlockInfo> stepAConstructTeamBlock(ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            return _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, true, BlockFinderType.SchedulePeriod,schedulingOptions.GroupOnGroupPageForTeamBlockPer);
        }

    }
}
