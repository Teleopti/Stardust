using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockListSwapAnalyzer
    {
		void AnalyzeTeamBlock(IList<ITeamBlockInfo> teamBlockList, IList<IShiftCategory> shiftCategories, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, ITeamBlockSwap teamBlockSwap);
    }

    public class TeamBlockListSwapAnalyzer : ITeamBlockListSwapAnalyzer
    {
        private readonly IDetermineTeamBlockPriority _determineTeamBlockPriority;
       
        public TeamBlockListSwapAnalyzer(IDetermineTeamBlockPriority determineTeamBlockPriority)
        {
            _determineTeamBlockPriority = determineTeamBlockPriority;
        }

        public void AnalyzeTeamBlock(IList<ITeamBlockInfo> teamBlockList, IList<IShiftCategory> shiftCategories, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, ITeamBlockSwap teamBlockSwap)
        {
            var teamBlockPriorityDefinitionInfo = _determineTeamBlockPriority.CalculatePriority(teamBlockList, shiftCategories);
	        var analyzedTeamBlocks = new List<ITeamBlockInfo>();

			foreach (var teamBlockInfoHighSeniority in teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo)
			{
				foreach (var teamBlockInfoLowSeniority in teamBlockPriorityDefinitionInfo.LowToHighSeniorityListBlockInfo)
				{
					var highSeniorityPoints = teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(teamBlockInfoHighSeniority);
					var lowSeniorityPoints = teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(teamBlockInfoLowSeniority);

					if (analyzedTeamBlocks.Contains(teamBlockInfoLowSeniority)) continue;
					if (teamBlockInfoHighSeniority.Equals(teamBlockInfoLowSeniority)) continue;
					if (highSeniorityPoints > lowSeniorityPoints) continue;
					if (!teamBlockSwap.Swap(teamBlockInfoHighSeniority, teamBlockInfoLowSeniority, rollbackService, scheduleDictionary)) continue;

					teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(teamBlockInfoHighSeniority, lowSeniorityPoints);
					teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(teamBlockInfoLowSeniority, highSeniorityPoints);	
				}

				analyzedTeamBlocks.Add(teamBlockInfoHighSeniority);
	        }
        }
    }
}
