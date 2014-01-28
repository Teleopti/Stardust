using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IFilterForMultipleMatrixPerAgentInTeamBlock
    {
        IList<ITeamBlockInfo> Filter(IEnumerable<ITeamBlockInfo> listOfTeamBlocks);
    }
    public class FilterForMultipleMatrixPerAgentInTeamBlock:IFilterForMultipleMatrixPerAgentInTeamBlock 
    {
        private readonly ITeamBlockMatrixValidator  _teamBlockMatrixValidator;

        public FilterForMultipleMatrixPerAgentInTeamBlock(ITeamBlockMatrixValidator teamBlockMatrixValidator)
        {
            _teamBlockMatrixValidator = teamBlockMatrixValidator;
        }

        public IList<ITeamBlockInfo> Filter(IEnumerable<ITeamBlockInfo> listOfTeamBlocks)
        {
            var result = new List<ITeamBlockInfo>();
            foreach (var teamBlockInfo in result)
            {
                if(_teamBlockMatrixValidator.Validate(teamBlockInfo ))
                    result.Add(teamBlockInfo );
            }
            return result;
        }
    }
}
