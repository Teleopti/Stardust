using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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

		public static DateOnlyPeriod? MaximumPeriod(this DateOnlyPeriod? period1, DateOnlyPeriod? period2)
		{
			if (!period2.HasValue)
				return period1;
			if (!period1.HasValue)
				return period2;

			var minStart = new DateOnly(new DateTime(Math.Min(period1.Value.StartDate.Date.Ticks, period2.Value.StartDate.Date.Ticks)));
			var maxEnd = new DateOnly(new DateTime(Math.Max(period1.Value.EndDate.Date.Ticks, period2.Value.EndDate.Date.Ticks)));
			return new DateOnlyPeriod(minStart, maxEnd);
		}
	}
}