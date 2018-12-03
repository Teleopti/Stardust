using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Date helper class 
	/// </summary>
	public static class DateHelper
	{
		static DateHelper()
		{
			var calendar = new UmAlQuraCalendar();
			MaxSmallDateTime = calendar.MaxSupportedDateTime.Date;
			MinSmallDateTime = calendar.MinSupportedDateTime;
		}

		/// <summary>
		/// The cultures that should use week number from ISO8601
		/// </summary>
		/// <remarks>
		/// Nynorsk - Norway 2068
		/// Bokmål - Norway 1044
		/// Swedish - Sweden 1053
		/// German - Germany 1031
		/// German - Austria 3079
		/// German - Switzerland 2055
		/// Danish - Danmark 1030
		/// Finnish - Finland 1035
		/// French - France 1036
		/// English - UK 2057
		/// Spanish - Spain 3082
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly IList<int> Iso8601Cultures = new List<int> { 2068, 1044, 1053, 1031, 3079, 2055, 1030, 1035, 1036, 2057, 3082 };

		/// <summary>
		/// Returns quarter from month.
		/// </summary>
		/// <param name="month">The month.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2012-03-05
		/// </remarks>
		public static int GetQuarter(int month)
		{
			if (month < 1) return 0;
			if (month > 12) return 0;
			return (month - 1) / 3 + 1;
		}

		/// <summary>
		/// Returns week number. Contains a correction for FirstFourDayWeek CalendarWeekRule.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <param name="cult">The cult.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2007-12-05
		/// </remarks>
		public static int WeekNumber(DateTime date, CultureInfo cult)
		{
			var calendarWeekRule = cult.DateTimeFormat.CalendarWeekRule;
			if (Iso8601Cultures.Contains(cult.LCID))
			{
				calendarWeekRule = CalendarWeekRule.FirstFourDayWeek;
			}
			var weekNo = cult.Calendar.GetWeekOfYear(date, calendarWeekRule, cult.DateTimeFormat.FirstDayOfWeek);

			if (weekNo == 53 && cult.DateTimeFormat.Calendar.GetType() == typeof(GregorianCalendar)
							 && calendarWeekRule == CalendarWeekRule.FirstFourDayWeek)
			{
				weekNo = cult.DateTimeFormat.Calendar.GetWeekOfYear(
							 date.AddDays(7),
							 CalendarWeekRule.FirstFourDayWeek,
							 DayOfWeek.Monday) - 1;
				if (weekNo == 0)
				{
					weekNo = 53;
				}
			}
			return weekNo;
		}


		/// <summary>
		/// Check if date is on a weekend, weekends hard coded for now
		/// </summary>
		/// <param name="date"></param>
		/// <param name="cult"></param>
		/// <returns></returns>
		public static bool IsWeekend(DateOnly date, CultureInfo cult)
		{
			//data collected from http://en.wikipedia.org/wiki/Workweek_and_weekend

			var day = date.DayOfWeek;
			switch (cult.LCID)
			{
				case 5121: // Algeria
				case 15361: // Bahrain
				case 2117: // Bangladesh
				case 3073: // Egypt
				case 2049: // Iraq
				case 1037: // Israel
				case 11265: // Jordan
				case 13313: // Kuwait
				case 4097: // Libya
				case 1125: // Maldives
				case 1086: // Malaysia
				case 1121: // Nepal
				case 8193: // Oman
				case 16385: // Qatar
				case 1025: // Saudi Arabia
				case 10241: // Syria
				case 14337: // U.A.E.
				case 9217: // Yemen
					return day == DayOfWeek.Friday || day == DayOfWeek.Saturday;


				case 1164: // Afghanistan Thursday is half a day of work
				case 1065: // Iran  Thursday is half a day of work for most public offices and schools, but for most jobs, Thursday is a working day.
					return day == DayOfWeek.Friday;

				case 2110: // Brunei Darussalam
					return day == DayOfWeek.Friday || day == DayOfWeek.Sunday;


				case 2058: // Mexico it is a custom in most industries and trades to work half day on Saturday
				case 1054: // Thailand the working week is Monday to Saturday for a maximum of 44 to 48 hours per week (Saturday is usually a half or full day)
					return day == DayOfWeek.Sunday;

				default:
					return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
			}
		}

		/// <summary>
		/// Gets the last date in month.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-20
		/// </remarks>
		public static DateTime GetLastDateInMonth(DateTime theDate, CultureInfo culture)
		{
			return GetLastDateInMonth(theDate, culture.Calendar);
		}

		/// <summary>
		/// Gets the last date in month.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="calendar">The calendar.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-05-20
		/// </remarks>
		public static DateTime GetLastDateInMonth(DateTime theDate, Calendar calendar)
		{
			var year = calendar.GetYear(theDate);
			var month = calendar.GetMonth(theDate);
			var era = calendar.GetEra(theDate);

			return calendar.ToDateTime(
				year,
				month,
				calendar.GetDaysInMonth(year, month, era),
				0,
				0,
				0,
				0,
				era);
		}

		/// <summary>
		/// Gets the first date in month.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-20
		/// </remarks>
		public static DateTime GetFirstDateInMonth(DateTime theDate, CultureInfo culture)
		{
			return GetFirstDateInMonth(theDate, culture.Calendar);
		}

		/// <summary>
		/// Gets the first date in month.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="calendar">The calendar.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-05-20
		/// </remarks>
		public static DateTime GetFirstDateInMonth(DateTime theDate, Calendar calendar)
		{
			return calendar.ToDateTime(
				calendar.GetYear(theDate),
				calendar.GetMonth(theDate),
				1,
				0,
				0,
				0,
				0,
				calendar.GetEra(theDate));
		}

		/// <summary>
		/// Gets the first date in week.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-20
		/// </remarks>
		public static DateTime GetFirstDateInWeek(DateTime theDate, CultureInfo culture)
		{
			var currentDayOfWeek = (int) theDate.DayOfWeek;

			var differenceAsDays = currentDayOfWeek - (int)culture.DateTimeFormat.FirstDayOfWeek;
			if (differenceAsDays < 0) differenceAsDays += 7;

			return theDate.AddDays(-differenceAsDays);
		}

		public static DateOnly GetFirstDateInWeek(DateOnly theDate, DayOfWeek workweekStartsAt)
		{
			var currentDayOfWeek = (int) theDate.DayOfWeek;

			var differenceAsDays = currentDayOfWeek - (int) workweekStartsAt;
			if (differenceAsDays < 0) differenceAsDays += 7;

			return theDate.AddDays(-differenceAsDays);
		}

		/// <summary>
		/// Gets the last date in week.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-20
		/// </remarks>
		public static DateTime GetLastDateInWeek(DateTime theDate, CultureInfo culture)
		{
			return GetFirstDateInWeek(theDate, culture).AddDays(6);
		}

		///<summary>
		/// Used when getting the last date in work week for person
		///</summary>
		///<param name="theDate"></param>
		///<param name="workweekStartsAt"></param>
		///<returns></returns>
		public static DateOnly GetLastDateInWeek(DateOnly theDate, DayOfWeek workweekStartsAt)
		{
			return GetFirstDateInWeek(theDate, workweekStartsAt).AddDays(6);
		}

		/// <summary>
		/// Gets the week period using the the TimeZone and Culture.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-08-14    
		/// </remarks>
		public static DateOnlyPeriod GetWeekPeriod(DateOnly theDate, CultureInfo culture)
		{
			return GetWeekPeriod(theDate, culture.DateTimeFormat.FirstDayOfWeek);
		}

		///<summary>
		/// Used when getting a work week for person
		///</summary>
		///<param name="theDate"></param>
		///<param name="workweekStartsAt"></param>
		///<returns></returns>
		public static DateOnlyPeriod GetWeekPeriod(DateOnly theDate, DayOfWeek workweekStartsAt)
		{
			var localStartDate = GetFirstDateInWeek(theDate, workweekStartsAt);
			var localEndDate = localStartDate.AddDays(6);

			return new DateOnlyPeriod(localStartDate, localEndDate);
		}

		/// <summary>
		/// Gets the days of week.
		/// </summary>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-20
		/// </remarks>
		public static IList<DayOfWeek> GetDaysOfWeek(CultureInfo culture)
		{
			IList<DayOfWeek> daysToReturn = new List<DayOfWeek>();

			var current = (int) culture.DateTimeFormat.FirstDayOfWeek;
			for (var i = 0; i < 7; i++)
			{
				daysToReturn.Add((DayOfWeek) current);
				current++;
				if (current > 6) current = 0;
			}

			return daysToReturn;
		}

		/// <summary>
		/// Gets the week day names.
		/// </summary>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2011-06-22
		/// </remarks>
		public static IEnumerable<string> GetWeekdayNames(CultureInfo culture)
		{
			var daysOfWeek = GetDaysOfWeek(culture);
			return daysOfWeek.Select(d => culture.DateTimeFormat.GetDayName(d)).ToList();
		}

		/// <summary>
		/// Gets the Month name.
		/// </summary>
		/// <param name="theDate">The date.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-02-07
		/// </remarks>
		public static string GetMonthName(DateTime theDate, CultureInfo culture)
		{
			var dateTimeFormatInfo = culture.DateTimeFormat;
			return dateTimeFormatInfo.GetMonthName(CultureInfo.CurrentCulture.Calendar.GetMonth(theDate));
		}

		/// <summary>
		/// Get a time string from minutes
		/// </summary>
		/// <param name="totalMinutes"></param>
		/// <returns></returns>
		public static string HourMinutesString(double totalMinutes)
		{
			if (totalMinutes < 1)
				return "00:00";

			var totMinutes = (long) totalMinutes;

			var hours = totMinutes / 60;
			var minutes = (int) (totMinutes - hours * 60);
			string hourString;
			string minutesString;
			var numberInfo = CultureInfo.CurrentCulture.NumberFormat;

			if (hours < 10)
				hourString = "0" + hours.ToString(numberInfo);
			else
				hourString = hours.ToString(numberInfo);

			if (minutes < 10)
				minutesString = "0" + minutes.ToString(numberInfo);
			else
				minutesString = minutes.ToString(numberInfo);

			return hourString + ":" + minutesString;
		}

		/// <summary>
		/// Gets the min small date time.
		/// </summary>
		/// <value>The min small date time.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-01-28
		/// </remarks>
		public static DateTime MinSmallDateTime { get; }

		/// <summary>
		/// Gets the max small date time.
		/// </summary>
		/// <value>The max small date time.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-01-28
		/// </remarks>
		public static DateTime MaxSmallDateTime { get; }

		// Matched to work similar to SQL small date time. https://msdn.microsoft.com/en-us/library/ms182418.aspx
		public static DateTime GetSmallDateTime(DateTime value)
		{
			var timeOfDay = TimeSpan.FromHours(value.Hour).Add(TimeSpan.FromMinutes(value.Minute));
			if (value.Second * 1000 + value.Millisecond < 29999)
				return value.Date.Add(timeOfDay);

			return value.Date.Add(timeOfDay).AddMinutes(1);
		}

		/// <summary>
		/// Gets the DateOnly counterpart.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateOnly ToDateOnly(this DateTime dt)
		{
			return new DateOnly(dt);
		}
	}
}