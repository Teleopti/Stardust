using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class BlockInfoFactory
	{
		public BlockInfo Execute(IEnumerable<IScheduleMatrixPro> allMatrixes,
			Func<IScheduleMatrixPro, DateOnlyPeriod?> periodForOneMatrix)
		{
			DateOnlyPeriod? totalPeriod=null;

			foreach (var scheduleMatrixPro in allMatrixes)
			{
				var period = periodForOneMatrix(scheduleMatrixPro);
				if (period.HasValue)
				{
					if (totalPeriod == null)
					{
						totalPeriod = period;
					}
					else
					{
						//make a method on dateonlyperiod if correct...
						var minStart = new DateOnly(new DateTime(Math.Min(period.Value.StartDate.Date.Ticks, totalPeriod.Value.StartDate.Date.Ticks)));
						var maxEnd = new DateOnly(new DateTime(Math.Max(period.Value.EndDate.Date.Ticks, totalPeriod.Value.EndDate.Date.Ticks)));
						totalPeriod = new DateOnlyPeriod(minStart, maxEnd);
					}
				}
			}

			return totalPeriod.HasValue ? new BlockInfo(totalPeriod.Value) : null;
		}
	}
}