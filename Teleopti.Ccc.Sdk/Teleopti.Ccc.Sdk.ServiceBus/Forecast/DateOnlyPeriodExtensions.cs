using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
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
	}
}