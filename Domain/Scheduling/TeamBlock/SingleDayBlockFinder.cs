using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class SingleDayBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool TEMPTOGGLE)
		{
			return matrixes.Select(matrix => matrix.GetScheduleDayByKey(blockOnDate).DaySchedulePart())
				.Any(scheduleDay => !scheduleDay.HasDayOff()) ? 
				new BlockInfo(new DateOnlyPeriod(blockOnDate, blockOnDate)) : 
				null;
		}
	}
}