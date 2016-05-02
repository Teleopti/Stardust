using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ReturnPeriodBasedOnDayBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam)
		{
			return new BlockInfo(new DateOnlyPeriod(blockOnDate, blockOnDate));
		}
	}
}