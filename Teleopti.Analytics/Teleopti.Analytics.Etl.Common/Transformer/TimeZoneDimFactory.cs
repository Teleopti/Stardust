using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class TimeZoneDimFactory
	{
		public IList<TimeZoneDim> Create(
			TimeZoneInfo etlDefaultTimeZone, 
			IList<TimeZoneInfo> timeZonesUsedByClient, 
			IList<TimeZoneInfo> timeZonesUsedByDataSources)
		{
			var isUtcInUseByClient = timeZonesUsedByClient.Any(t => t.Id == "UTC");
			var combinedList = timeZonesUsedByClient.Union(timeZonesUsedByDataSources).ToList();
			if (!combinedList.Contains(etlDefaultTimeZone))
				combinedList.Add(etlDefaultTimeZone);
			if (!combinedList.Contains(TimeZoneInfo.Utc))
				combinedList.Add(TimeZoneInfo.Utc);

			return combinedList
				.Select(t => new TimeZoneDim(t, t.Id == etlDefaultTimeZone.Id, isUtcInUseByClient && t.Id == "UTC"))
				.ToList();
		}
	}
}