using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class BetweenDayOffBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate)
		{
			var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();

			return new BlockInfoFactory().Execute(matrixes, blockOnDate, 
				(scheduleMatrixPro, date) => blockPeriodFinderBetweenDayOff.GetBlockPeriod(scheduleMatrixPro, date));
		}
	}
}