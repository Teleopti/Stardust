using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public static class DateOnlyExtensions
	{
		public static DateOnly ValidDateOnly(this DateOnly value)
		{
			var minDateOnly = new DateOnly(DateHelper.MinSmallDateTime);
			var maxDateOnly = new DateOnly(DateHelper.MaxSmallDateTime);

			var toReturn = value;
			if (toReturn < minDateOnly)
			{
				toReturn = minDateOnly;
			}
			if (toReturn > maxDateOnly)
			{
				toReturn = maxDateOnly;
			}
			return toReturn;
		}

		public static DateTime Utc(this DateOnly dateTime)
		{
			return DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc);
		}

		public static IList<DateOnlyPeriod> SplitToContinuousPeriods(this IList<DateOnly> dayCollection)
		{
			var periodList = new List<DateOnlyPeriod>();
			DateOnly? startDate = null;
			for (var i = 0; i < dayCollection.Count; i++)
			{
				if (!startDate.HasValue)
				{
					startDate = dayCollection[i];
				}
				var nextDate = dayCollection[i].AddDays(1);
				if (dayCollection.Contains(nextDate)) continue;
				periodList.Add(new DateOnlyPeriod(startDate.Value, nextDate.AddDays(-1)));
				startDate = null;
			}
			return periodList;
		}
	}
}