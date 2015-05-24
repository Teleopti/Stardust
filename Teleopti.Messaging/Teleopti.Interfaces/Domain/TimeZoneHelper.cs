using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Convert date and time between current sessions time zone and UTC.
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2007-10-23
	/// </remarks>
	public static class TimeZoneHelper
	{
		/// <summary>
		/// Converts to UTC, using the logged on users TimeZone.
		/// </summary>
		/// <param name="localDateTime">The local date time.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-23
		/// </remarks>
		public static DateTime ConvertToUtc(DateTime localDateTime)
		{
			TimeZoneInfo sourceTimeZone = CurrentSessionTimeZone;
			return sourceTimeZone.SafeConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified));
		}

		/// <summary>
		/// Converts to UTC.
		/// </summary>
		/// <param name="localDateTime">The local date time.</param>
		/// <param name="sourceTimeZone">The source time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-23
		/// </remarks>
		public static DateTime ConvertToUtc(DateTime localDateTime, TimeZoneInfo sourceTimeZone)
		{
			return sourceTimeZone.SafeConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified));
		}

		/// <summary>
		/// Converts from UTC.
		/// </summary>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-23
		/// </remarks>
		public static DateTime ConvertFromUtc(DateTime utcDateTime)
		{
			TimeZoneInfo targetTimeZone = CurrentSessionTimeZone;
			return ConvertFromUtc(utcDateTime, targetTimeZone);
		}

		/// <summary>
		/// Converts from UTC.
		/// </summary>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <param name="sourceTimeZone">The source time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-11-26
		/// </remarks>
		public static DateTime ConvertFromUtc(DateTime utcDateTime, TimeZoneInfo sourceTimeZone)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified),
											  sourceTimeZone);
		}

		/// <summary>
		/// Creates a new DateTimePeriod from local date time.
		/// </summary>
		/// <param name="localDateTimeStart">The local date time start.</param>
		/// <param name="localDateTimeEnd">The local date time end.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-23
		/// </remarks>
		public static DateTimePeriod NewUtcDateTimePeriodFromLocalDateTime(DateTime localDateTimeStart, DateTime localDateTimeEnd)
		{
			return new DateTimePeriod(ConvertToUtc(localDateTimeStart), ConvertToUtc(localDateTimeEnd));
		}

		/// <summary>
		/// Gets the current sessions time zone.
		/// </summary>
		/// <value>The current sessions time zone.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-23
		/// </remarks>
		public static TimeZoneInfo CurrentSessionTimeZone
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone; }
		}

		/// <summary>
		/// Creates a new UTC format date time period from local date time.
		/// </summary>
		/// <param name="localStartDateTime">The local start date time.</param>
		/// <param name="localEndDateTime">The local end date time.</param>
		/// <param name="timeZone">The time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-29
		/// </remarks>
		public static DateTimePeriod NewUtcDateTimePeriodFromLocalDateTime(DateTime localStartDateTime, DateTime localEndDateTime, TimeZoneInfo timeZone)
		{
			InParameter.NotNull("timeZone", timeZone);

			DateTime utcStartDateTime = timeZone.SafeConvertTimeToUtc(localStartDateTime);
			DateTime utcEndDateTime = timeZone.SafeConvertTimeToUtc(localEndDateTime);

			return new DateTimePeriod(utcStartDateTime, utcEndDateTime);
		}


		public static DateTimePeriod NewUtcDateTimePeriodFromLocalDate(DateOnly localStartDateTime, DateOnly localEndDateTime, TimeZoneInfo timeZone)
		{
			return NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime.Date, localEndDateTime.Date, timeZone);
		}

		public static DaylightTime GetDaylightChanges(TimeZoneInfo timeZoneInfo, int year)
		{

			// only handles one adjustment per year
			var ruleFound = timeZoneInfo.GetAdjustmentRules().SingleOrDefault(adjust => adjustmentIsApplicable(year, adjust));

			if (ruleFound != null)
			{

				return new DaylightTime(getTransitionDate(timeZoneInfo, ruleFound.DaylightTransitionStart, year),
				 getTransitionDate(timeZoneInfo, ruleFound.DaylightTransitionEnd, year), ruleFound.DaylightDelta);
			}

			return null;
		}

		private static bool adjustmentIsApplicable(int inYear, TimeZoneInfo.AdjustmentRule adjust)
		{
			return (adjust.DateStart.Year <= inYear && adjust.DateEnd.Year >= DateTime.MaxValue.Year);
		}

		private static DateTime getTransitionDate(TimeZoneInfo timeZoneInfo, TimeZoneInfo.TransitionTime transitionTime, int year)
		{
			return transitionTime.IsFixedDateRule ? getFixedDateRuleDate(timeZoneInfo, transitionTime, year) : getFloatingDateRuleDate(timeZoneInfo, transitionTime, year);
		}

		private static DateTime getFixedDateRuleDate(TimeZoneInfo timeZoneInfo, TimeZoneInfo.TransitionTime transitionTime, int year)
		{

			return ConvertToUtc(new DateTime(
				year,
				transitionTime.Month,
				transitionTime.Day,
				transitionTime.TimeOfDay.Hour,
				transitionTime.TimeOfDay.Minute,
				transitionTime.TimeOfDay.Second
				), timeZoneInfo);

			
		}

		private static DateTime getFloatingDateRuleDate(TimeZoneInfo timeZoneInfo, TimeZoneInfo.TransitionTime transitionTime, int year)
		{

			var localCalendar = CultureInfo.CurrentCulture.Calendar;
			var startOfWeekForTransition = transitionTime.Week * 7 - 6;
			var dayOfWeekMonthStart = (int)localCalendar.GetDayOfWeek(new DateTime(year, transitionTime.Month, 1));
			var changeDayOfWeek = (int)transitionTime.DayOfWeek;

			var transitionDay = dayOfWeekMonthStart <= changeDayOfWeek
				? startOfWeekForTransition + (changeDayOfWeek - dayOfWeekMonthStart)
				: startOfWeekForTransition + (7 - dayOfWeekMonthStart + changeDayOfWeek);

			if (transitionDay > localCalendar.GetDaysInMonth(year, transitionTime.Month))
			{
				transitionDay -= 7;
			}

			return ConvertToUtc(new DateTime(
				year,
				transitionTime.Month,
				transitionDay,
				transitionTime.TimeOfDay.Hour,
				transitionTime.TimeOfDay.Minute,
				transitionTime.TimeOfDay.Second
				), timeZoneInfo);
		}

	}
}