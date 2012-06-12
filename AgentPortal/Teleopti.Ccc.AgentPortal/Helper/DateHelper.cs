using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    /// <summary>
    /// Date helper class 
    /// </summary>
    public static class DateHelper
	{
		//Nynorsk - Norway 2068
		//Bokmål - Norway 1044
		//Swedish - Sweden 1053
		//German - Germany 1031
		//German - Austria 3079
		//German - Switzerland 2055
		//Danish - Danmark 1030
		//Finnish - Finland 1035
		//France - France 1036
		private static readonly IList<int> Iso8601Cultures = new List<int> { 2068, 1044, 1053, 1031, 3079, 2055, 1030, 1035, 1036 };
		
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
            int weekNo = cult.Calendar.GetWeekOfYear(date, cult.DateTimeFormat.CalendarWeekRule, cult.DateTimeFormat.FirstDayOfWeek);

            if (weekNo == 53 && cult.DateTimeFormat.Calendar.GetType() == typeof(GregorianCalendar)
                && cult.DateTimeFormat.CalendarWeekRule == CalendarWeekRule.FirstFourDayWeek)
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
            int year = culture.Calendar.GetYear(theDate);
            int month = culture.Calendar.GetMonth(theDate);
            int era = culture.Calendar.GetEra(theDate);

            return culture.Calendar.ToDateTime(
                year,
                month,
                culture.Calendar.GetDaysInMonth(year, month, era),
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
            return culture.Calendar.ToDateTime(
                culture.Calendar.GetYear(theDate),
                culture.Calendar.GetMonth(theDate),
                1,
                0,
                0,
                0,
                0,
                culture.Calendar.GetEra(theDate));
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
            DateTimeFormatInfo dateTimeFormatInfo = culture.DateTimeFormat;
            return dateTimeFormatInfo.GetMonthName(theDate.Month);
        }

        public static bool UseIso8601Format(CultureInfo cultureInfo)
        {
        	return Iso8601Cultures.Contains(cultureInfo.LCID);
        }
    }
}