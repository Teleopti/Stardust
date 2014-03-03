using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ITeamBlockLocatorWithHighestPoints
    {
        ITeamBlockInfo FindBestTeamBlockToSwapWith(IList<ITeamBlockInfo> swappableTeamBlocks, IDictionary<ITeamBlockInfo, double> seniorityValueDic);
    }

    public class TeamBlockLocatorWithHighestPoints : ITeamBlockLocatorWithHighestPoints
    {
        public ITeamBlockInfo FindBestTeamBlockToSwapWith(IList<ITeamBlockInfo> swappableTeamBlocks, IDictionary<ITeamBlockInfo, double> seniorityValueDic)
        {
            var bestValue = double.MinValue;
            ITeamBlockInfo bestBlock = null;
            foreach (var swappableTeamBlock in swappableTeamBlocks)
            {
                var value = seniorityValueDic[swappableTeamBlock];
                if (value > bestValue)
                {
                    bestValue = value;
                    bestBlock = swappableTeamBlock;
                }
            }

            return bestBlock;
        }
    }
}
