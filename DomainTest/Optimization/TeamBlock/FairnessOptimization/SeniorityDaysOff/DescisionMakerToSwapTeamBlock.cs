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
        private readonly ITeamBlockSwapper _teambBlockSwapper;

        public DescisionMakerToSwapTeamBlock(IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks, ISeniorityCalculatorForTeamBlock seniorityCalculatorForTeamBlock, ITeamBlockLocatorWithHighestPoints teamBlockLocatorWithHighestPoints, ITeamBlockSwapper teambBlockSwapper)
        {
            _filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
            _seniorityCalculatorForTeamBlock = seniorityCalculatorForTeamBlock;
            _teamBlockLocatorWithHighestPoints = teamBlockLocatorWithHighestPoints;
            _teambBlockSwapper = teambBlockSwapper;
        }

        public  bool  TrySwapForMostSenior(IList<ITeamBlockInfo> teamBlocksToWorkWith, ITeamBlockInfo mostSeniorTeamBlock, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IDictionary<DayOfWeek, int> weekDayPoints)
        {
            var swappableTeamBlocks = _filterOnSwapableTeamBlocks.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock);

            var seniorityValueDic = _seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith,weekDayPoints);
            var seniorValue = seniorityValueDic[mostSeniorTeamBlock];

            var swapSuccess = false;
            while (swappableTeamBlocks.Count > 0 && !swapSuccess)
            {
                var blockToSwapWith = _teamBlockLocatorWithHighestPoints.FindBestTeamBlockToSwapWith(swappableTeamBlocks, seniorityValueDic);
                if (seniorValue < seniorityValueDic[blockToSwapWith])
                {
                    bool success = _teambBlockSwapper.TrySwap(mostSeniorTeamBlock, blockToSwapWith, rollbackService,
                                                               scheduleDictionary);
                    swapSuccess = true;
                }

                swappableTeamBlocks.Remove(blockToSwapWith);
            }

            return swapSuccess;
        }
    }
}
