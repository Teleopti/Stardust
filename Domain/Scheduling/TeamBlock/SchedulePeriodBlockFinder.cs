using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class SchedulePeriodBlockFinder : IBlockFinder
	{
		public IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate)
		{
			return new BlockInfoFactory().Execute(matrixes, blockOnDate, 
				(scheduleMatrixPro, date) => scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod);
		}
	}
}