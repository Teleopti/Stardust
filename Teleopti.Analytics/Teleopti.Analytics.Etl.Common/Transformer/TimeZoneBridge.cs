using System;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class TimeZoneBridge
	{
		private TimeZoneBridge()
		{
		}

		public TimeZoneBridge(DateTime date, TimeZoneInfo timeZoneInfo, int intervalsPerDay)
			: this()
		{
			var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(date, timeZoneInfo);

			Date = date.Date;
			IntervalId = new IntervalBase(date, intervalsPerDay).Id;
			TimeZoneCode = timeZoneInfo.Id;
			LocalDate = localDateTime.Date;
			LocalIntervalId = new IntervalBase(localDateTime, intervalsPerDay).Id;
		}

		public DateTime Date { get; }

		public int IntervalId { get; }

		public string TimeZoneCode { get; }

		public DateTime LocalDate { get; }

		public int LocalIntervalId { get; }
	}
}
