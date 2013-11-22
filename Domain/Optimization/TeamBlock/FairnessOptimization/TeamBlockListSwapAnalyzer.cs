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
        private readonly ISwapScheduleDays _swapScheduleDays;
        private readonly IValidateScheduleDays _validateScheduleDay;

        public TeamBlockListSwapAnalyzer(IDetermineTeamBlockPriority determineTeamBlockPriority, ISwapScheduleDays swapScheduleDays, IValidateScheduleDays validateScheduleDay)
        {
            _determineTeamBlockPriority = determineTeamBlockPriority;
            _swapScheduleDays = swapScheduleDays;
            _validateScheduleDay = validateScheduleDay;
        }

        public void AnalyzeTeamBlock(IList<ITeamBlockInfo> teamBlockList, IList<IShiftCategory> shiftCategories)
        {
            var teamBlockPriorityDefinitionInfo = _determineTeamBlockPriority.CalculatePriority(teamBlockList, shiftCategories);
            foreach (int higherPriority in teamBlockPriorityDefinitionInfo.HighToLowAgentPriorityList)
            {
                foreach (int lowerPriority in teamBlockPriorityDefinitionInfo.LowToHighAgentPriorityList)
                {
                    ITeamBlockInfo higherPriorityBlock = teamBlockPriorityDefinitionInfo.BlockOnAgentPriority(higherPriority);
                    int lowestShiftCategoryPrioirty = teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(higherPriorityBlock);
                    if (teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriorityList.Any(higherShiftCategoryPriority => higherShiftCategoryPriority > lowestShiftCategoryPrioirty))
                    {
                        ITeamBlockInfo lowestPriorityBlock = teamBlockPriorityDefinitionInfo.BlockOnAgentPriority(lowerPriority);
                        if (validateBlock(higherPriorityBlock, lowestPriorityBlock))
                            swapBlock(higherPriorityBlock, lowestPriorityBlock);
                    }
                }
            }
        }

        private void swapBlock(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock)
        {
            _swapScheduleDays.Swap(higherPriorityBlock, lowestPriorityBlock);
        }

        private bool validateBlock(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock)
        {
            return _validateScheduleDay.Validate(higherPriorityBlock, lowestPriorityBlock);
        }
    }
}
