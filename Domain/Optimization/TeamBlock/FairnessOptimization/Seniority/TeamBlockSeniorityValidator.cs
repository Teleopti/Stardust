using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ITeamBlockSeniorityValidator
	{
		bool ValidateSeniority(ITeamBlockInfo teamBlockInfo);
	}

	public class TeamBlockSeniorityValidator : ITeamBlockSeniorityValidator
	{

		public bool ValidateSeniority(ITeamBlockInfo teamBlockInfo)
		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var groupMembers = teamInfo.GroupMembers;

			foreach (var workflowControlSet in groupMembers.Select(groupMember => groupMember.WorkflowControlSet).Distinct())
			{
				if (workflowControlSet?.GetFairnessType() != FairnessType.Seniority) return false;
			}

			return true;
		}
	}
}
