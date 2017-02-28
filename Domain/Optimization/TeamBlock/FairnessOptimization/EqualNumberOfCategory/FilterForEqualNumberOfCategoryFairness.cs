﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterForEqualNumberOfCategoryFairness
	{
		IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockListRaw);
	}

	public class FilterForEqualNumberOfCategoryFairness : IFilterForEqualNumberOfCategoryFairness
	{

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
					if (workflowControlSet.GetFairnessType() == FairnessType.EqualNumberOfShiftCategory)
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