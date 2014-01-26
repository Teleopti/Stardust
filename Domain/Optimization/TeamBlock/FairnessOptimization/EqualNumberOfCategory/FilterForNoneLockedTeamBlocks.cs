

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterForNoneLockedTeamBlocks
	{
		List<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> blocksToWorkWith);
	}

	public class FilterForNoneLockedTeamBlocks : IFilterForNoneLockedTeamBlocks
	{
		public List<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> blocksToWorkWith)
		{
			var noneLockedBlocks = new List<ITeamBlockInfo>();
			foreach (var teamBlockInfo in blocksToWorkWith)
			{
				var blockLocked = false;
				foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
				{
					foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
					{
						var matrix = teamBlockInfo.TeamInfo.MatrixForMemberAndDate(person, dateOnly);
						var scheduleDayPro = matrix.GetScheduleDayByKey(dateOnly);
						if (!matrix.UnlockedDays.Contains(scheduleDayPro))
						{
							blockLocked = true;
							break;
						}
					}
					if (blockLocked)
						break;
				}

				if (!blockLocked)
					noneLockedBlocks.Add(teamBlockInfo);
			}
			return noneLockedBlocks;
		} 
	}
}