using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamMemberCountValidator
	{
		bool ValidateMemberCount(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2);
	}

	public class TeamMemberCountValidator : ITeamMemberCountValidator
	{
		public bool ValidateMemberCount(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			return teamBlockInfo1.TeamInfo.GroupMembers.Count().Equals(teamBlockInfo2.TeamInfo.GroupMembers.Count());
		}
	}
}
