using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class PeriodExtensions
	{
		public static IEnumerable<DateTimePeriod> PeriodBlocks(this IEnumerable<IPeriodized> periods)
		{
			var allPeriods = periods.Select(l => l.Period);
			return DateTimePeriod.MergePeriods(allPeriods);
		}

		public static DateTimePeriod? OuterPeriod(this IEnumerable<IPeriodized> periods)
		{
			if (!periods.Any()) return null;
			DateTime max = DateTime.MinValue;
			DateTime min = DateTime.MaxValue;
			foreach (var period in periods)
			{
				if (period.Period.StartDateTime < min)
					min = period.Period.StartDateTime;
				if (period.Period.EndDateTime > max)
					max = period.Period.EndDateTime;
			}
			return new DateTimePeriod(min,max);
		}
	}
}