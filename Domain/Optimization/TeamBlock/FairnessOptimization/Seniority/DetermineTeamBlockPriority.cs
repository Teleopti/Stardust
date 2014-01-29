using System.Collections.Generic;
using System.Linq;
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
	    private readonly IShiftCategoryPointExtractor _shiftCategoryPointInfoExtractor;

        public DetermineTeamBlockPriority(ISeniorityExtractor seniorityExtractor, IShiftCategoryPointExtractor shiftCategoryPointInfoExtractor)
		{
			_seniorityExtractor = seniorityExtractor;
			_shiftCategoryPointInfoExtractor = shiftCategoryPointInfoExtractor;
		}

        public ITeamBlockPriorityDefinitionInfo CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories)
        {
	        var seniorityInfos = _seniorityExtractor.ExtractSeniority(teamBlockInfos);
	        var shiftCategoryPointInfos = _shiftCategoryPointInfoExtractor.ExtractShiftCategoryInfos( teamBlockInfos, shiftCategories);

            var teamBlockPriorityDefinitionInfo = new TeamBlockPriorityDefinitionInfo();
            foreach (var teamBlockInfo in teamBlockInfos)
            {
                var seniority = getTeamBlockPoints(teamBlockInfo, seniorityInfos);
                var shiftCategoryPoints = getTeamBlockPoints(teamBlockInfo, shiftCategoryPointInfos);
                var teamBlockInfoPriority = new TeamBlockInfoPriority(teamBlockInfo, seniority, shiftCategoryPoints);
                teamBlockPriorityDefinitionInfo.AddItem(teamBlockInfoPriority, teamBlockInfoPriority.TeamBlockInfo, teamBlockInfoPriority.ShiftCategoryPriority );
            }

            return teamBlockPriorityDefinitionInfo;
        }

        private double getTeamBlockPoints(ITeamBlockInfo teamBlock, IEnumerable<ITeamBlockPoints> teamBlockPointList)
        {
            var extractedTeamBlock = teamBlockPointList.FirstOrDefault(s => s.TeamBlockInfo.Equals(teamBlock));
            if (extractedTeamBlock != null)
                return extractedTeamBlock.Points;
            return -1;
        } 
    }
}