

using System.Collections.Generic;
using System.Linq;
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
					var teamInfo = teamBlockInfo.TeamInfo;
					foreach (var person in teamInfo.GroupMembers)
					{
						var matrix = teamInfo.MatrixForMemberAndDate(person, dateOnly);
						if (matrix == null) continue;
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