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
		}

		public AnalyticsBridgeTimeZone(int dateId, int intervalId, int timeZoneId) : this()
		{
			DateId = dateId;
			IntervalId = intervalId;
			TimeZoneId = timeZoneId;
		}

		public virtual int DateId { get; set; }
		public virtual int IntervalId { get; set; }
		public virtual int TimeZoneId { get; set; }
		public virtual int LocalDateId { get; set; }
		public virtual int LocalIntervalId { get; set; }
		protected virtual int DatasourceId { get; set; } = 1;
		protected virtual DateTime InsertDate { get; set; } = DateTime.UtcNow;
		protected virtual DateTime UpdateDate { get; set; } = DateTime.UtcNow;


		public virtual bool FillLocals(IDictionary<TimeSpan, AnalyticsInterval> intervals, IDictionary<DateTime, IAnalyticsDate> dates,
			DateTime localTime)
		{
			AnalyticsInterval localInterval;
			if (!intervals.TryGetValue(localTime.TimeOfDay, out localInterval))
				return false;
			IAnalyticsDate localDate;
			if (!dates.TryGetValue(localTime.Date, out localDate))
				return false;

			LocalIntervalId = localInterval.IntervalId;
			LocalDateId = localDate.DateId;
			return true;
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