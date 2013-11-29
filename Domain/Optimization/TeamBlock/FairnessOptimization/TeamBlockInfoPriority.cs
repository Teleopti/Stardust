using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public  class TeamBlockInfoPriority
    {
        public TeamBlockInfoPriority(ITeamBlockInfo teamBlockInfo, int agentPriority, int shiftCategoryPriority)
        {
            TeamBlockInfo = teamBlockInfo;
            AgentPriority = agentPriority;
            ShiftCategoryPriority = shiftCategoryPriority;
        }
        
        public ITeamBlockInfo TeamBlockInfo { get; set; }
        public int AgentPriority { get; set; }
        public int ShiftCategoryPriority { get; set; }
    }
}