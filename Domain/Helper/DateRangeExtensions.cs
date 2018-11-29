using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class DateRangeExtensions
	{
		public static IEnumerable<DateTime> DateRange(this DateTime instance, int days)
		{
			return from i in Enumerable.Range(0, days) select instance.AddDays(i);
		}

		public static IEnumerable<DateOnly> DateRange(this DateOnly instance, int days)
		{
			return from i in Enumerable.Range(0, days) select instance.AddDays(i);
		}

		public static IEnumerable<DateOnly> DateRange(this DateOnly instance, DateOnly toDate)
		{
			return from i in Enumerable.Range(0, toDate.Date.Subtract(instance.Date).Days + 1) select instance.AddDays(i);
		}
	}
}