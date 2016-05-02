using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class SchedulePeriodBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam)
		{
			var roleModelMatrix = matrixes.First();
			return new BlockInfo(roleModelMatrix.SchedulePeriod.DateOnlyPeriod);
		}
	}
}