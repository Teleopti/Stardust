using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ITeamBlockWeightExtractor
    {
        TeamBlockWeight DetermineWeight(ITeamBlockInfo teamBlockInfo);
    }

    public class TeamBlockWeightExtractor : ITeamBlockWeightExtractor
    {
        public TeamBlockWeight DetermineWeight(ITeamBlockInfo teamBlockInfo)
        {
            return new TeamBlockWeight
                {
                    TeamWeight = teamBlockInfo.TeamInfo.GroupPerson.GroupMembers.Count(),
                    BlockWeight = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection().Count
                };
        }
    }
}