using System;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Date helper class 
    /// </summary>
    public static class DateHelper
	{
        static DateHelper()
        {
            var calendar = new UmAlQuraCalendar();
             _maxSmallDateTime = calendar.MaxSupportedDateTime.Date;
             _minSmallDateTime = calendar.MinSupportedDateTime;
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

        private readonly static DateTime _minSmallDateTime;
        private static readonly DateTime _maxSmallDateTime;

		/// <summary>
		/// Returns quarter from month.
		/// </summary>
		/// <param name="month">The month.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: mathias
		/// Created date: 2012-03-05
		/// </remarks>
		public static int GetQuarter(int month)
		{
			if (month < 1) return 0;
			if (month > 12) return 0;
			return (month - 1)/3 + 1;
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
            int weekNo = cult.Calendar.GetWeekOfYear(date, calendarWeekRule, cult.DateTimeFormat.FirstDayOfWeek);

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
        public static bool IsWeekend(DateTime date, CultureInfo cult)
        {
            bool ret;
            if (cult.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Sunday || cult.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday)
            {
                switch (cult.Calendar.GetDayOfWeek(date))
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        ret = true;
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            else if (cult.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Saturday)
            {
                switch (cult.Calendar.GetDayOfWeek(date))
                {
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        ret = true;
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            else
            {
                throw new ArgumentException("No weekend defined for this first day of week", "cult");
            }
            return ret;
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
            int year = calendar.GetYear(theDate);
            int month = calendar.GetMonth(theDate);
            int era = calendar.GetEra(theDate);

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
            int firstDayOfWeek = (int)culture.DateTimeFormat.FirstDayOfWeek;
            int currentDayOfWeek = (int)theDate.DayOfWeek;

            int differenceAsDays = currentDayOfWeek - firstDayOfWeek;
            if (differenceAsDays < 0) differenceAsDays += 7;

            return theDate.AddDays(-differenceAsDays);
        }

        ///<summary>
        /// Used when getting the first date in work week for person
        ///</summary>
        ///<param name="theDate"></param>
        ///<param name="workweekStartsAt"></param>
        ///<returns></returns>
        public static DateTime GetFirstDateInWeek(DateTime theDate, DayOfWeek workweekStartsAt)
        {
            var currentDayOfWeek = (int)theDate.DayOfWeek;

            int differenceAsDays = currentDayOfWeek - (int)workweekStartsAt;
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
        public static DateTime GetLastDateInWeek(DateTime theDate, DayOfWeek workweekStartsAt)
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
            DateTime localStartDate = GetFirstDateInWeek(theDate.Date, culture);
            DateTime localEndDate = localStartDate.AddDays(6);

            return new DateOnlyPeriod(new DateOnly(localStartDate), new DateOnly(localEndDate));
        }

        ///<summary>
        /// Used when getting a work week for person
        ///</summary>
        ///<param name="theDate"></param>
        ///<param name="workweekStartsAt"></param>
        ///<returns></returns>
        public static DateOnlyPeriod GetWeekPeriod(DateOnly theDate, DayOfWeek workweekStartsAt)
        {
            DateTime localStartDate = GetFirstDateInWeek(theDate.Date, workweekStartsAt);
            DateTime localEndDate = localStartDate.AddDays(6);

            return new DateOnlyPeriod(new DateOnly(localStartDate), new DateOnly(localEndDate));
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

            int current = (int)culture.DateTimeFormat.FirstDayOfWeek;
            for (int i = 0; i < 7; i++)
            {
                daysToReturn.Add((DayOfWeek)current);
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
		/// Created by: mathiass
		/// Created date: 2011-06-22
		/// </remarks>
		public static IEnumerable<string> GetWeekdayNames(CultureInfo culture)
		{
			var dayNames = new List<string>();
			var daysOfWeek = GetDaysOfWeek(culture);
			foreach (var dayOfWeek in daysOfWeek)
			{
				dayNames.Add(culture.DateTimeFormat.GetDayName(dayOfWeek));
			}
			return dayNames;
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
        public  static string GetMonthName(DateTime theDate, CultureInfo culture)
        {
            DateTimeFormatInfo dateTimeFormatInfo = culture.DateTimeFormat;
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

            long totMinutes = (long)totalMinutes;

            long hours = totMinutes / 60;
            int minutes = (int)(totMinutes - hours * 60);
            string hourString;
            string minutesString;
            NumberFormatInfo numberInfo = CultureInfo.CurrentCulture.NumberFormat;

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
        public static DateTime MinSmallDateTime
        {
            get { return _minSmallDateTime; }
        }

        /// <summary>
        /// Gets the max small date time.
        /// </summary>
        /// <value>The max small date time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-01-28
        /// </remarks>
        public static DateTime MaxSmallDateTime
        {
            get { return _maxSmallDateTime; }
        }

        /// <summary>
        /// Splits the date time period.
        /// TODO: Maybe should be moved to DatetimePeriod
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Henryg
        /// Created date: 2009-11-13
        /// </remarks>
        public static IEnumerable<DateTimePeriod> SplitDateTimePeriod(DateTimePeriod dateTimePeriod, TimeSpan timeSpan)
        {
            IList<DateTimePeriod> splittedDateTimePeriods = new List<DateTimePeriod>();
            DateTime startDateTime = dateTimePeriod.StartDateTime;
            DateTime endDateTime = dateTimePeriod.StartDateTime.Add(timeSpan);

            while (endDateTime <= dateTimePeriod.EndDateTime)
            {
                DateTimePeriod partDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime.AddTicks(-1));
                splittedDateTimePeriods.Add(partDateTimePeriod);
                startDateTime = endDateTime;
                endDateTime = startDateTime.Add(timeSpan);
            }
            DateTimePeriod lastDateTimePeriod = new DateTimePeriod(startDateTime, dateTimePeriod.EndDateTime);
            splittedDateTimePeriods.Add(lastDateTimePeriod);
            return splittedDateTimePeriods;
        }
    }
}