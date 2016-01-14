using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public static class DateOnlyPeriodExtensions
	{
		public static IEnumerable<DateOnlyPeriod> Split(this DateOnlyPeriod period, int days)
		{
			var startDate = period.StartDate;
			while (startDate <= period.EndDate)
			{
				var endDate = startDate.AddDays(days);
				if (endDate > period.EndDate) endDate = period.EndDate;

				yield return new DateOnlyPeriod(startDate, endDate);

				startDate = endDate.AddDays(1);
			}
		}

		public static DateOnlyPeriod Inflate(this DateOnlyPeriod period, int days)
		{
			return new DateOnlyPeriod(period.StartDate.AddDays(-days), period.EndDate.AddDays(days));
		}
	}
}