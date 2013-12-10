using System.Linq;
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
			var groupPerson = teamInfo.GroupPerson;
			var groupMembers = groupPerson.GroupMembers;

			foreach (var workflowControlSet in groupMembers.Select(groupMember => groupMember.WorkflowControlSet))
			{
				if (workflowControlSet == null) return false;
				if (workflowControlSet.UseShiftCategoryFairness) return false;
			}

			return true;
		}
	}
}
