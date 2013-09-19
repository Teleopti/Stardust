﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class PeriodExtensions
	{
		public static IEnumerable<DateTimePeriod> PeriodBlocks(this IEnumerable<IPeriodized> periods)
		{
			var allPeriods = periods.Select(l => l.Period);
			return DateTimePeriod.MergePeriods(allPeriods);
		}
	}
}