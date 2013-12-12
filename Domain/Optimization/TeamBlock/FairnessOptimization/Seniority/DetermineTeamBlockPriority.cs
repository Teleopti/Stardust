using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IDetermineTeamBlockPriority
    {
		ITeamBlockPriorityDefinitionInfo CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories);
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

        public ITeamBlockPriorityDefinitionInfo CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories)
        {
	        var seniorityInfos = _seniorityExtractor.ExtractSeniority(teamBlockInfos);
	        var shiftCategoryPointInfos = _shiftCategoryPointInfoExtractor.ExtractShiftCategoryInfos(teamBlockInfos, shiftCategories);

            var teamBlockPriorityDefinitionInfo = new TeamBlockPriorityDefinitionInfo();
            foreach (var teamBlockInfo in teamBlockInfos)
            {
	            var seniorityInfo = seniorityInfos[teamBlockInfo];
	            var shiftCategoryPointInfo = shiftCategoryPointInfos[teamBlockInfo];
				var teamBlockInfoPriority = new TeamBlockInfoPriority(teamBlockInfo, seniorityInfo.Seniority, shiftCategoryPointInfo.Point);
                teamBlockPriorityDefinitionInfo.AddItem(teamBlockInfoPriority);
            }
            return teamBlockPriorityDefinitionInfo;
        }
    }
}