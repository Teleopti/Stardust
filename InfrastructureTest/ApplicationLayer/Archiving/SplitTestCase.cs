using System;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Archiving
{
	public class SplitTestCase
	{
		public enum Expectations
		{
			NothingArchived,
			OneArchived
		}

		public DateTime AbsenceStartUtc;
		public DateTime AbsenceEndUtc;
		public DateTime AbsenceStartLocal;
		public DateTime AbsenceEndLocal;

		public DateOnly ArchiveStart;
		public DateOnly ArchiveEnd;
		public Expectations ExpectedOutcome;
		private TimeZoneInfo _timeZoneInfo;

		public void Setup(TimeZoneInfo timeZoneInfo)
		{
			_timeZoneInfo = timeZoneInfo;
			AbsenceStartLocal = DateTime.SpecifyKind(AbsenceStartLocal, DateTimeKind.Unspecified);
			AbsenceEndLocal = DateTime.SpecifyKind(AbsenceEndLocal, DateTimeKind.Unspecified);
			AbsenceStartUtc = TimeZoneInfo.ConvertTimeToUtc(AbsenceStartLocal, _timeZoneInfo);
			AbsenceEndUtc = TimeZoneInfo.ConvertTimeToUtc(AbsenceEndLocal, _timeZoneInfo);

			// Extra checks to make sure the tests are working because timezones are hard.
			TimeZoneInfo.ConvertTime(AbsenceStartUtc, _timeZoneInfo).Should().Be.EqualTo(AbsenceStartLocal);
			TimeZoneInfo.ConvertTime(AbsenceEndUtc, _timeZoneInfo).Should().Be.EqualTo(AbsenceEndLocal);
		}

		public TimeZoneInfo TimeZone()
		{
			return _timeZoneInfo;
		}

		public DateTime ExpectedStart()
		{
			var agentArchiveStart = TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(ArchiveStart.Date, DateTimeKind.Unspecified), _timeZoneInfo), _timeZoneInfo);
			return agentArchiveStart < AbsenceStartLocal ? AbsenceStartLocal : agentArchiveStart;
		}

		public DateTime ExpectedEnd()
		{
			var agentArchiveEnd = TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(ArchiveEnd.Date, DateTimeKind.Unspecified), _timeZoneInfo), _timeZoneInfo).AddHours(23).AddMinutes(59);
			return agentArchiveEnd < AbsenceEndLocal ? agentArchiveEnd : AbsenceEndLocal;
		}

		public override string ToString()
		{
			return
				$"Archiving {dateTimeFormat(ArchiveStart)} - {dateTimeFormat(ArchiveEnd)} with " +
				$"Absence {dateTimeFormat(AbsenceStartLocal)} - {dateTimeFormat(AbsenceEndLocal)}. " +
				$"Expected {ExpectedOutcome}.";
		}

		public static string DateTimePeriodInTimeZoneToString(DateTimePeriod dateTimePeriod, TimeZoneInfo timeZoneInfo)
		{
			return $"{dateTimeFormat(TimeZoneInfo.ConvertTime(dateTimePeriod.StartDateTime, timeZoneInfo))} - " +
				   $"{dateTimeFormat(TimeZoneInfo.ConvertTime(dateTimePeriod.EndDateTime, timeZoneInfo))}";
		}

		private static string dateTimeFormat(DateOnly dateOnly)
		{
			return dateTimeFormat(dateOnly.Date);
		}

		private static string dateTimeFormat(DateTime dateTime)
		{
			if (dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0)
				return dateTime.ToString("yyyy-MM-dd");
			if (dateTime.Second == 0)
				return dateTime.ToString("yyyy-MM-dd HH:mm");
			return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
		}
	}
}