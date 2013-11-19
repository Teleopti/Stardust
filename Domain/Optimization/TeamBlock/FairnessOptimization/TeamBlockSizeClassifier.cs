using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ITeamBlockSizeClassifier
    {
        HashSet<IList<ITeamBlockInfo>> SplitTeamBlockInfo(IList<ITeamBlockInfo> teamBlockInfoList);
    }

    public class TeamBlockSizeClassifier : ITeamBlockSizeClassifier
    {
        private readonly ITeamBlockWeightExtractor _teamBlockWeightExtractor;

        public TeamBlockSizeClassifier(ITeamBlockWeightExtractor teamBlockWeightExtractor)
        {
            _teamBlockWeightExtractor = teamBlockWeightExtractor;
        }

        public HashSet<IList<ITeamBlockInfo>> SplitTeamBlockInfo(IList<ITeamBlockInfo> teamBlockInfoList)
        {
            var teamBlockListDictionary = new Dictionary<TeamBlockWeight, IList<ITeamBlockInfo>>();
            foreach (ITeamBlockInfo teamBlockInfo in teamBlockInfoList)
            {
                //get team block weight
                TeamBlockWeight teamBlockWeight = _teamBlockWeightExtractor.DetermineWeight(teamBlockInfo);
                if (!teamBlockListDictionary.ContainsKey(teamBlockWeight))
                    teamBlockListDictionary.Add(teamBlockWeight, new List<ITeamBlockInfo>());
                teamBlockListDictionary[teamBlockWeight].Add(teamBlockInfo);
            }
            var finalResult = new HashSet<IList<ITeamBlockInfo>>();
            foreach (var item in teamBlockListDictionary.Values)
            {
                finalResult.Add(item);
            }
            return finalResult;
        }
    }
}