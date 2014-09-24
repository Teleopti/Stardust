using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ITeamBlockSeniorityValidator
	{
		bool ValidateSeniority(ITeamBlockInfo teamBlockInfo);
	}

	public class TeamBlockSeniorityValidator : ITeamBlockSeniorityValidator
	{
		private readonly bool _schedulerSeniority11111;

		public TeamBlockSeniorityValidator(bool Scheduler_Seniority_11111)
		{
			_schedulerSeniority11111 = Scheduler_Seniority_11111;
		}

		public bool ValidateSeniority(ITeamBlockInfo teamBlockInfo)
		{
			var teamInfo = teamBlockInfo.TeamInfo;
			var groupMembers = teamInfo.GroupMembers;

			foreach (var workflowControlSet in groupMembers.Select(groupMember => groupMember.WorkflowControlSet))
			{
				if (workflowControlSet == null) return false;
				if (workflowControlSet.GetFairnessType(true, _schedulerSeniority11111) != FairnessType.Seniority) return false;
			}

			return true;
		}
	}
}
