using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IDetermineTeamBlockPriority
    {
        ITeamBlockPriorityDefinitionInfo CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos);
    }

    public class DetermineTeamBlockPriority : IDetermineTeamBlockPriority
    {
	    private readonly ISeniorityExtractor _seniorityExtractor;
	    private readonly IShiftCategoryPointInfoExtractor _shiftCategoryPointInfoExtractor;

		public DetermineTeamBlockPriority(ISeniorityExtractor seniorityExtractor, IShiftCategoryPointInfoExtractor shiftCategoryPointInfoExtractor)
		{
			_seniorityExtractor = seniorityExtractor;
			_shiftCategoryPointInfoExtractor = shiftCategoryPointInfoExtractor;
		}

        public ITeamBlockPriorityDefinitionInfo CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos)
        {
	        var teamInfos = teamBlockInfos.Select(teamBlockInfo => teamBlockInfo.TeamInfo).ToList();
	        var seniorityInfos = _seniorityExtractor.ExtractSeniority(teamInfos);
	        var shiftCategoryPointInfos = _shiftCategoryPointInfoExtractor.ExtractShiftCategoryInfos(teamBlockInfos);

            var teamBlockPriorityDefinitionInfo = new TeamBlockPriorityDefinitionInfo();
            foreach (var teamBlockInfo in teamBlockInfos)
            {
				var seniorityInfo = seniorityInfos[teamBlockInfo.TeamInfo];
	            var shiftCategoryPointInfo = shiftCategoryPointInfos[teamBlockInfo];
				var teamBlockInfoPriority = new TeamBlockInfoPriority(teamBlockInfo, seniorityInfo.Seniority, shiftCategoryPointInfo.Point);
                teamBlockPriorityDefinitionInfo.AddItem(teamBlockInfoPriority);
            }
            return teamBlockPriorityDefinitionInfo;
        }
    }
}