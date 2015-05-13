using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterForEqualNumberOfCategoryFairness
	{
		IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockListRaw, bool schedulerSeniority11111);
	}

	public class FilterForEqualNumberOfCategoryFairness : IFilterForEqualNumberOfCategoryFairness
	{

		public IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockListRaw, bool schedulerSeniority11111)
		{
			var teamBlockListWithCorrectWorkFlowControlSet = new List<ITeamBlockInfo>();
			foreach (var teamBlockInfo in teamBlockListRaw)
			{
				bool matchInGroupMemberFound = false;
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupMembers)
				{
					if(matchInGroupMemberFound) continue;
					var workflowControlSet = groupMember.WorkflowControlSet;
					if (workflowControlSet == null) continue;
					if (workflowControlSet.GetFairnessType(schedulerSeniority11111) == FairnessType.EqualNumberOfShiftCategory)
					{
						teamBlockListWithCorrectWorkFlowControlSet.Add(teamBlockInfo);
						matchInGroupMemberFound = true;
					}
				}
			}
			return teamBlockListWithCorrectWorkFlowControlSet;
		}
	}
}