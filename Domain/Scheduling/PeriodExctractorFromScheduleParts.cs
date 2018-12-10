using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PeriodExtractorFromScheduleParts
	{
		public DateOnlyPeriod? ExtractPeriod(IEnumerable<IScheduleDay> scheduleDays)
		{
			DateOnly minDate = DateOnly.MaxValue;
			DateOnly maxDate = DateOnly.MinValue;
			foreach (var scheduleDay in scheduleDays)
			{
				if (scheduleDay.DateOnlyAsPeriod.DateOnly < minDate)
					minDate = scheduleDay.DateOnlyAsPeriod.DateOnly;

				if (scheduleDay.DateOnlyAsPeriod.DateOnly > maxDate)
					maxDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
			}

			return minDate == DateOnly.MaxValue ? (DateOnlyPeriod?)null : new DateOnlyPeriod(minDate, maxDate);
		}
	}
}