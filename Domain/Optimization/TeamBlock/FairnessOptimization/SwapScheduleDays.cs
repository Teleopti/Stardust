using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
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