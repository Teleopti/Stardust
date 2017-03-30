using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class BlockInfoFactory
	{
		public BlockInfo Execute(IEnumerable<IScheduleMatrixPro> allMatrixes, DateOnly startDate, Func<IScheduleMatrixPro, DateOnly, DateOnlyPeriod?> periodForOneMatrix)
		{
			var currentPeriod = periodForDate(allMatrixes, startDate, periodForOneMatrix);
			if (!currentPeriod.HasValue)
				return null;

			while (true)
			{
				var startPeriod = periodForDate(allMatrixes, currentPeriod.Value.StartDate, periodForOneMatrix);
				var endPeriod = periodForDate(allMatrixes, currentPeriod.Value.EndDate, periodForOneMatrix);
				var newPeriod = MaximumPeriod(startPeriod, endPeriod);
				if (!newPeriod.HasValue || newPeriod.Value == currentPeriod.Value)
				{
					return new BlockInfo(currentPeriod.Value);
				}
				currentPeriod = newPeriod;
			}
			throw new Exception("If you end up here, the world has gone crazy");
		}

		private static DateOnlyPeriod? periodForDate(IEnumerable<IScheduleMatrixPro> allMatrixes, DateOnly startDate, Func<IScheduleMatrixPro, DateOnly, DateOnlyPeriod?> periodForOneMatrix)
		{
			DateOnlyPeriod? totalPeriod = null;

			foreach (var scheduleMatrixPro in allMatrixes)
			{
				var period = periodForOneMatrix(scheduleMatrixPro, startDate);
				totalPeriod = MaximumPeriod(period, totalPeriod);
			}
			return totalPeriod;
		}

		private static DateOnlyPeriod? MaximumPeriod(DateOnlyPeriod? period1, DateOnlyPeriod? period2)
		{
			if (!period2.HasValue)
				return period1;
			if (!period1.HasValue)
				return period2;

			//make a method on dateonlyperiod if correct...
			var minStart = new DateOnly(new DateTime(Math.Min(period1.Value.StartDate.Date.Ticks, period2.Value.StartDate.Date.Ticks)));
			var maxEnd = new DateOnly(new DateTime(Math.Max(period1.Value.EndDate.Date.Ticks, period2.Value.EndDate.Date.Ticks)));
			return new DateOnlyPeriod(minStart, maxEnd);
		}
	}
}