using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class BetweenDayOffBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool TEMPTOGGLE)
		{
			if (!TEMPTOGGLE)
				return findRemoveMe(matrixes, blockOnDate);

			var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();

			return new BlockInfoFactory().Execute(matrixes, blockOnDate, 
				(scheduleMatrixPro, date) => blockPeriodFinderBetweenDayOff.GetBlockPeriod(scheduleMatrixPro, date));
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockPeriod_42836)]
		private static IBlockInfo findRemoveMe(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate)
		{
			var roleModelMatrix = matrixes.First();
			var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();
			var blockPeriod = blockPeriodFinderBetweenDayOff.GetBlockPeriod(roleModelMatrix, blockOnDate);
			return blockPeriod == null ? null : new BlockInfo(blockPeriod.Value);
		}
	}
}