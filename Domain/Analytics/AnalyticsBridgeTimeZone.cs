using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Analytics
{
	[Serializable]
	public class AnalyticsBridgeTimeZone
	{
		public AnalyticsBridgeTimeZone()
		{
			DatasourceId = 1;
			InsertDate = DateTime.UtcNow;
			UpdateDate = DateTime.UtcNow;
		}

		public virtual int DateId { get; set; }
		public virtual int IntervalId { get; set; }
		public virtual int TimeZoneId { get; set; }
		public virtual int LocalDateId { get; set; }
		public virtual int LocalIntervalId { get; set; }
		protected virtual int DatasourceId { get; set; }
		protected virtual DateTime InsertDate { get; set; }
		protected virtual DateTime UpdateDate { get; set; }

		public static AnalyticsBridgeTimeZone Create(int dateId, int intervalId, int timeZoneId, IDictionary<TimeSpan, AnalyticsInterval> intervals, IDictionary<DateTime, IAnalyticsDate> dates, DateTime localTime)
		{
			AnalyticsInterval localInterval;
			if (!intervals.TryGetValue(localTime.TimeOfDay, out localInterval))
				return null;
			IAnalyticsDate localDate;
			if (!dates.TryGetValue(localTime.Date, out localDate))
				return null;

			return new AnalyticsBridgeTimeZone
			{
				TimeZoneId = timeZoneId,
				DateId = dateId,
				IntervalId = intervalId,
				LocalDateId = localDate.DateId,
				LocalIntervalId = localInterval.IntervalId
			};
		}

		public override bool Equals(object obj)
		{
			var analyticsPermission = obj as AnalyticsBridgeTimeZone;
			if (analyticsPermission == null)
				return false;
			return DateId == analyticsPermission.DateId
				   && IntervalId == analyticsPermission.IntervalId
				   && TimeZoneId == analyticsPermission.TimeZoneId;
		}
		public override int GetHashCode()
		{
			return $"{DateId}|{IntervalId}|{TimeZoneId}".GetHashCode();
		}
	}
}