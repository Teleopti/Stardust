using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockPeriodValidator
	{
		bool ValidatePeriod(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2);
	}

	public class TeamBlockPeriodValidator : ITeamBlockPeriodValidator
	{
		public bool ValidatePeriod(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			return teamBlockInfo1.BlockInfo.BlockPeriod.Equals(teamBlockInfo2.BlockInfo.BlockPeriod);
		}
	}
}
