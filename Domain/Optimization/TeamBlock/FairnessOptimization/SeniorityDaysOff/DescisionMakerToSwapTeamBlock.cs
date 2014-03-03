using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IDescisionMakerToSwapTeamBlock
    {
        bool TrySwapForMostSenior(IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints);
    }

    public class DescisionMakerToSwapTeamBlock : IDescisionMakerToSwapTeamBlock
    {
        private readonly IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
        private readonly ISeniorityCalculatorForTeamBlock _seniorityCalculatorForTeamBlock;
        private readonly ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHighestPoints;
        private readonly ITeamBlockSwapper _teamBlockSwapper;

        public DescisionMakerToSwapTeamBlock(IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks, ISeniorityCalculatorForTeamBlock seniorityCalculatorForTeamBlock, ITeamBlockLocatorWithHighestPoints teamBlockLocatorWithHighestPoints, ITeamBlockSwapper teamBlockSwapper)
        {
            _filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
            _seniorityCalculatorForTeamBlock = seniorityCalculatorForTeamBlock;
            _teamBlockLocatorWithHighestPoints = teamBlockLocatorWithHighestPoints;
            _teamBlockSwapper = teamBlockSwapper;
        }

        public  bool  TrySwapForMostSenior(IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

            var seniorityValueDic = _seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith,weekDayPoints);
            
            var swapSuccess = false;
            if (seniorityValueDic.Count > 0)
            {
                var seniorValue = seniorityValueDic[mostSeniorTeamBlock];
                while (swappableTeamBlocks.Count > 0 && !swapSuccess)
                {
                    var blockToSwapWith = _teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(swappableTeamBlocks, seniorityValueDic);
                    if (seniorValue < seniorityValueDic[blockToSwapWith])
                    {
                        bool success = _teamBlockSwapper.TrySwap(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
                                                                   scheduleDictionary);
                        swapSuccess = success;
                    }

                    swappableTeamBlocks.Remove(blockToSwapWith);
                }
            }

            return swapSuccess;
        }
    }
}
