using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class BetweenDayOffBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam, bool TEMPTOGGLE)
		{
			if (!TEMPTOGGLE)
				findRemoveMe(matrixes, blockOnDate, singleAgentTeam);

			var roleModelMatrix = matrixes.First();
			var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();
			var blockPeriod = blockPeriodFinderBetweenDayOff.GetBlockPeriod(roleModelMatrix, blockOnDate, singleAgentTeam);
			return blockPeriod == null ? null : new BlockInfo(blockPeriod.Value);
		}


		public IBlockInfo findRemoveMe(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam)
		{
			var roleModelMatrix = matrixes.First();
			var blockPeriodFinderBetweenDayOff = new BlockPeriodFinderBetweenDayOff();
			var blockPeriod = blockPeriodFinderBetweenDayOff.GetBlockPeriod(roleModelMatrix, blockOnDate, singleAgentTeam);
			return blockPeriod == null ? null : new BlockInfo(blockPeriod.Value);
		}
	}
}