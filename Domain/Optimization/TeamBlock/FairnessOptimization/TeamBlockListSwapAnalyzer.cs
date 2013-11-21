using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ITeamBlockListSwapAnalyzer
    {
        void AnalyzeTeamBlock(IList<ITeamBlockInfo> teamBlockList, IList<IShiftCategory> shiftCategories);
    }

    public class TeamBlockListSwapAnalyzer : ITeamBlockListSwapAnalyzer
    {
        private readonly IDetermineTeamBlockPriority _determineTeamBlockPriority;

        public TeamBlockListSwapAnalyzer(IDetermineTeamBlockPriority determineTeamBlockPriority)
        {
            _determineTeamBlockPriority = determineTeamBlockPriority;
        }

        public void AnalyzeTeamBlock(IList<ITeamBlockInfo> teamBlockList, IList<IShiftCategory> shiftCategories)
        {
            var teamBlockPriorityDefinition = new TeamBlockPriorityDefinitionInfo(_determineTeamBlockPriority.CalculatePriority(teamBlockList, shiftCategories));
            foreach (int higherPriority in teamBlockPriorityDefinition.HighToLowAgentPriorityList)
            {
                foreach (int lowerPriority in teamBlockPriorityDefinition.LowToHighAgentPriorityList)
                {
                    ITeamBlockInfo higherPriorityBlock = teamBlockPriorityDefinition.BlockOnAgentPriority(higherPriority);
                    int lowestShiftCategoryPrioirty =
                        teamBlockPriorityDefinition.GetShiftCategoryPriorityOfBlock(higherPriorityBlock);
                    if (
                        teamBlockPriorityDefinition.HighToLowShiftCategoryPriorityList.Any(
                            higherShiftCategoryPriority => higherShiftCategoryPriority > lowestShiftCategoryPrioirty))
                    {
                        ITeamBlockInfo lowestPriorityBlock =
                            teamBlockPriorityDefinition.BlockOnAgentPriority(lowerPriority);
                        if (validateBlock(higherPriorityBlock, lowestPriorityBlock))
                            swapBlock(higherPriorityBlock, lowestPriorityBlock);
                    }
                }
            }
        }

        private void swapBlock(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock)
        {

        }

        private bool validateBlock(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock)
        {
            return true;
        }
    }
}
