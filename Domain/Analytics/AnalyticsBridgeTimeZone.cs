using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Analytics
{
	public interface IAnalyticsBridgeTimeZone
	{
		int DateId { get; set; }
		int IntervalId { get; set; }
		int TimeZoneId { get; set; }
	}

	public class AnalyticsBridgeTimeZonePartial : IAnalyticsBridgeTimeZone
	{
		public virtual int DateId { get; set; }
		public virtual int IntervalId { get; set; }
		public virtual int TimeZoneId { get; set; }

		public AnalyticsBridgeTimeZonePartial()
		{
		}

		protected AnalyticsBridgeTimeZonePartial(int dateId, int intervalId, int timeZoneId) : this()
		{
			DateId = dateId;
			IntervalId = intervalId;
			TimeZoneId = timeZoneId;
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

	[Serializable]
	public class AnalyticsBridgeTimeZone : AnalyticsBridgeTimeZonePartial
	{
		public AnalyticsBridgeTimeZone()
		{
		}

		public AnalyticsBridgeTimeZone(int dateId, int intervalId, int timeZoneId) : base(dateId, intervalId, timeZoneId)
		{
		}

		public virtual int LocalDateId { get; set; }
		public virtual int LocalIntervalId { get; set; }
		protected virtual int DatasourceId { get; set; } = 1;
		protected virtual DateTime InsertDate { get; set; } = DateTime.UtcNow;
		protected virtual DateTime UpdateDate { get; set; } = DateTime.UtcNow;


		public virtual bool FillLocals(IDictionary<TimeSpan, int> intervals, IDictionary<DateTime, int> dates,
			DateTime localTime)
		{
			int localInterval;
			if (!intervals.TryGetValue(localTime.TimeOfDay, out localInterval))
				return false;
			int localDate;
			if (!dates.TryGetValue(localTime.Date, out localDate))
				return false;

			LocalIntervalId = localInterval;
			LocalDateId = localDate;
			return true;
		}

		
	}
}