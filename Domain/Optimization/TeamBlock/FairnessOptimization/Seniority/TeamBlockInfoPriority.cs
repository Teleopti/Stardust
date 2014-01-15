using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ITeamBlockInfoPriority
	{
		ITeamBlockInfo TeamBlockInfo { get; }
		double Seniority { get; }
		int ShiftCategoryPriority { get; set; }	
	}

	public class TeamBlockInfoPriority : ITeamBlockInfoPriority
    {
		public ITeamBlockInfo TeamBlockInfo { get; private set; }
		public double Seniority { get; private set; }
		public int ShiftCategoryPriority { get; set; }

        public TeamBlockInfoPriority(ITeamBlockInfo teamBlockInfo, double seniority, int shiftCategoryPriority)
        {
            TeamBlockInfo = teamBlockInfo;
            Seniority = seniority;
            ShiftCategoryPriority = shiftCategoryPriority;
        }    
    }
}