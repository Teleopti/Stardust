using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ISwapScheduleDays
    {
        bool Swap(ITeamBlockInfo highPriorityTeamBlock, ITeamBlockInfo lowPriorityTeamBlock);
    }

    public class SwapScheduleDays : ISwapScheduleDays
    {
        public bool Swap(ITeamBlockInfo highPriorityTeamBlock, ITeamBlockInfo lowPriorityTeamBlock)
        {
            return true;
        }
    }
}