using System.Collections.Generic;
using System.Linq;
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
            foreach (var higherPriority in teamBlockPriorityDefinitionInfo.HighToLowSeniorityList)
            {
                foreach (var lowerPriority in teamBlockPriorityDefinitionInfo.LowToHighSeniorityList)
                {
                    var higherPriorityBlock = teamBlockPriorityDefinitionInfo.BlockOnSeniority(higherPriority);
                    int lowestShiftCategoryPriority = teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(higherPriorityBlock);
                    if (teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriorityList.Any(higherShiftCategoryPriority => higherShiftCategoryPriority > lowestShiftCategoryPriority))
                    {
                        var lowestPriorityBlock = teamBlockPriorityDefinitionInfo.BlockOnSeniority(lowerPriority);
		                teamBlockSwap.Swap(higherPriorityBlock, lowestPriorityBlock, rollbackService, scheduleDictionary);
                    }
                }
            }
        }
    }
}
