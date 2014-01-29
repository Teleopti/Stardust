using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockMatrixValidator
    {
        bool Validate(ITeamBlockInfo teamBlockInfo);
    }

    public class TeamBlockMatrixValidator : ITeamBlockMatrixValidator
    {
        public bool Validate(ITeamBlockInfo teamBlockInfo)
        {
            var sampleMatrixNumber = teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(teamBlockInfo.TeamInfo.GroupMembers.First(),
                                                                                          teamBlockInfo.BlockInfo
                                                                                                       .BlockPeriod);
            foreach (var teamMember in teamBlockInfo.TeamInfo.GroupMembers)
            {
                var numberOfMatrixNumber = teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(teamMember,
                                                                                           teamBlockInfo.BlockInfo
                                                                                                        .BlockPeriod);
                if (numberOfMatrixNumber != sampleMatrixNumber) return false;
            }
            return true;
        }
    }
}
