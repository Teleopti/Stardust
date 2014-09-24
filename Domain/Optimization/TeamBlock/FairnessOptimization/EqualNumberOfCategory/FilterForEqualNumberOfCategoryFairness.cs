using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterForEqualNumberOfCategoryFairness
	{
		IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockListRaw);
	}

	public class FilterForEqualNumberOfCategoryFairness : IFilterForEqualNumberOfCategoryFairness
	{
		private readonly bool _schedulerHidePointsFairnessSystem28317;
		private readonly bool _schedulerSeniority11111;

		public FilterForEqualNumberOfCategoryFairness(bool Scheduler_HidePointsFairnessSystem_28317, bool SchedulerSeniority11111)
		{
			_schedulerHidePointsFairnessSystem28317 = Scheduler_HidePointsFairnessSystem_28317;
			_schedulerSeniority11111 = SchedulerSeniority11111;
		}

		public IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockListRaw)
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
					if (workflowControlSet.GetFairnessType(_schedulerHidePointsFairnessSystem28317, _schedulerSeniority11111) == FairnessType.EqualNumberOfShiftCategory)
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