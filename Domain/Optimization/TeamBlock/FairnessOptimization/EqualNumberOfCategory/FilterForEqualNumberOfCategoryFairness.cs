

using System.Collections.Generic;
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
				var foundCorrect = false;
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
				{
					var wfcs = groupMember.WorkflowControlSet;
					if (wfcs == null || !wfcs.UseShiftCategoryFairness)
					{
						foundCorrect = false;
						break;
					}

					foundCorrect = true;
				}
				if (foundCorrect)
					teamBlockListWithCorrectWorkFlowControlSet.Add(teamBlockInfo);
			}
			return teamBlockListWithCorrectWorkFlowControlSet;
		}
	}
}