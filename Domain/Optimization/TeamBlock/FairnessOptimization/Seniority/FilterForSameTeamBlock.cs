using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IFilterForSameTeamBlock
    {
        IList<ITeamBlockPoints> Filter(ITeamBlockInfo teamBlock, IList<ITeamBlockPoints> listOfTeamBlocks);
    }

    public class FilterForSameTeamBlock : IFilterForSameTeamBlock
    {
        public IList<ITeamBlockPoints> Filter(ITeamBlockInfo teamBlock, IList<ITeamBlockPoints> listOfTeamBlocks)
        {
            return (from o in listOfTeamBlocks
                    where o.TeamBlockInfo.BlockInfo .BlockPeriod.Equals(teamBlock.BlockInfo.BlockPeriod)
                          && o.TeamBlockInfo.TeamInfo.GroupMembers.Count().Equals(teamBlock.TeamInfo.GroupMembers.Count())
                    select o).ToList();
        }
    }
}
