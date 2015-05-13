using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ITeamBlockSeniorityValidator
	{
		bool ValidateSeniority(ITeamBlockInfo teamBlockInfo, bool schedulerSeniority11111);
	}

	public class TeamBlockSeniorityValidator : ITeamBlockSeniorityValidator
	{

		public bool ValidateSeniority(ITeamBlockInfo teamBlockInfo, bool schedulerSeniority11111)
		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var groupMembers = teamInfo.GroupMembers;

			foreach (var workflowControlSet in groupMembers.Select(groupMember => groupMember.WorkflowControlSet))
			{
				if (workflowControlSet == null) return false;
				if (workflowControlSet.GetFairnessType(schedulerSeniority11111) != FairnessType.Seniority) return false;
			}

			return true;
		}
	}
}
