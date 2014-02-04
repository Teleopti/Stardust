using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ISeniorTeamBlockLocator
    {
        ITeamBlockInfo FindMostSeniorTeamBlock(IEnumerable<ITeamBlockPoints> seniorityInfoDictionary);
    }

    public class SeniorTeamBlockLocator : ISeniorTeamBlockLocator
    {
        public ITeamBlockInfo FindMostSeniorTeamBlock(IEnumerable<ITeamBlockPoints> seniorityInfoDictionary)
        {
            var maxValue = double.MinValue;
            ITeamBlockInfo mostSeniorBlock = null;
            foreach (var seniorityInfo in seniorityInfoDictionary)
            {
                if (seniorityInfo.Points > maxValue)
                {
                    maxValue = seniorityInfo.Points;
                    mostSeniorBlock = seniorityInfo.TeamBlockInfo;
                }
            }
            return mostSeniorBlock;
        }
    }
}
