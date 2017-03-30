using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class SchedulePeriodBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam, bool TEMPTOGGLE)
		{
			if (!TEMPTOGGLE)
				return findRemoveMe(matrixes);

			var roleModelMatrix = matrixes.First();
			return new BlockInfo(roleModelMatrix.SchedulePeriod.DateOnlyPeriod);
		}

		private static IBlockInfo findRemoveMe(IEnumerable<IScheduleMatrixPro> matrixes)
		{
			var roleModelMatrix = matrixes.First();
			return new BlockInfo(roleModelMatrix.SchedulePeriod.DateOnlyPeriod);
		}
	}
}