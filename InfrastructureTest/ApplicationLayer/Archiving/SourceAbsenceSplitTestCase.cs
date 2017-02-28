using System;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Archiving
{
	public class SplitTestCase
	{
		public DateTime AbsenceStartUtc;
		public DateTime AbsenceEndUtc;
		public DateTime AbsenceStartLocal;
		public DateTime AbsenceEndLocal;

		public DateOnly ArchiveStart;
		public DateOnly ArchiveEnd;
		protected TimeZoneInfo TimeZoneInfo;

		public static string DateTimePeriodInTimeZoneToString(DateTimePeriod dateTimePeriod, TimeZoneInfo timeZoneInfo)
		{
			return $"{DateTimeFormat(TimeZoneInfo.ConvertTime(dateTimePeriod.StartDateTime, timeZoneInfo))} - " +
				   $"{DateTimeFormat(TimeZoneInfo.ConvertTime(dateTimePeriod.EndDateTime, timeZoneInfo))}";
		}

		protected static string DateTimeFormat(DateOnly dateOnly)
		{
			return DateTimeFormat(dateOnly.Date);
		}

		protected static string DateTimeFormat(DateTime dateTime)
		{
			if (dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0)
				return dateTime.ToString("yyyy-MM-dd");
			if (dateTime.Second == 0)
				return dateTime.ToString("yyyy-MM-dd HH:mm");
			return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public override string ToString()
		{
			return
				$"Archiving {DateTimeFormat(ArchiveStart)} - {DateTimeFormat(ArchiveEnd)} with " +
				$"Absence {DateTimeFormat(AbsenceStartLocal)} - {DateTimeFormat(AbsenceEndLocal)}.";
		}
	}

	public class TargetAbsenceSplitTestCase : SplitTestCase
	{
		public Action<SplitTestCase, IList<IPersonAbsence>, IList<IPersonAbsence>> Asserts;
	}

	public class SourceAbsenceSplitTestCase : SplitTestCase
	{
		public Expectations ExpectedOutcome;
		public enum Expectations
		{
			NothingArchived,
			OneArchived
		}

		public void Setup(TimeZoneInfo timeZoneInfo)
		{
			TimeZoneInfo = timeZoneInfo;
			AbsenceStartLocal = DateTime.SpecifyKind(AbsenceStartLocal, DateTimeKind.Unspecified);
			AbsenceEndLocal = DateTime.SpecifyKind(AbsenceEndLocal, DateTimeKind.Unspecified);
			AbsenceStartUtc = TimeZoneInfo.ConvertTimeToUtc(AbsenceStartLocal, TimeZoneInfo);
			AbsenceEndUtc = TimeZoneInfo.ConvertTimeToUtc(AbsenceEndLocal, TimeZoneInfo);

			// Extra checks to make sure the tests are working because timezones are hard.
			TimeZoneInfo.ConvertTime(AbsenceStartUtc, TimeZoneInfo).Should().Be.EqualTo(AbsenceStartLocal);
			TimeZoneInfo.ConvertTime(AbsenceEndUtc, TimeZoneInfo).Should().Be.EqualTo(AbsenceEndLocal);
		}

		public TimeZoneInfo TimeZone()
		{
			return TimeZoneInfo;
		}

		public DateTime ExpectedStart()
		{
			var agentArchiveStart = TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(ArchiveStart.Date, DateTimeKind.Unspecified), TimeZoneInfo), TimeZoneInfo);
			return agentArchiveStart < AbsenceStartLocal ? AbsenceStartLocal : agentArchiveStart;
		}

		public DateTime ExpectedEnd()
		{
			var agentArchiveEnd = TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(ArchiveEnd.Date, DateTimeKind.Unspecified), TimeZoneInfo), TimeZoneInfo).AddHours(23).AddMinutes(59);
			return agentArchiveEnd < AbsenceEndLocal ? agentArchiveEnd : AbsenceEndLocal;
		}

		public override string ToString()
		{
			return base.ToString() + $" Expected {ExpectedOutcome}.";
		}
	}
}