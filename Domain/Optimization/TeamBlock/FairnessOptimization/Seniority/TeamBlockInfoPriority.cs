using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public  class TeamBlockInfoPriority
    {
        public TeamBlockInfoPriority(ITeamBlockInfo teamBlockInfo, double seniority, int shiftCategoryPriority)
        {
            TeamBlockInfo = teamBlockInfo;
            Seniority = seniority;
            ShiftCategoryPriority = shiftCategoryPriority;
        }
        
        public ITeamBlockInfo TeamBlockInfo { get; set; }
        public double Seniority { get; set; }
        public int ShiftCategoryPriority { get; set; }
    }
}