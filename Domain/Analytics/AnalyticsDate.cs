using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsDatePartial : IAnalyticsDate
	{
		public virtual int DateId { get; set; }
		public virtual DateTime DateDate { get; set; }
	}
	public class AnalyticsDate : AnalyticsDatePartial
	{
		public AnalyticsDate()
		{
			
		}
		public AnalyticsDate(DateTime date, CultureInfo culture) : this()
		{
			DateDate = date.Date;
			Year = date.Year;
			YearMonth = int.Parse($"{date.Year}{date.Month:D2}");
			Month = date.Month;
			MonthName = DateHelper.GetMonthName(date, culture);
			MonthResourceKey = getMonthResourceKey(date.Month);
			DayInMonth = date.Day;
			WeekNumber = DateHelper.WeekNumber(date, culture);
			WeekdayName = culture.DateTimeFormat.GetDayName(date.DayOfWeek);
			WeekdayResourceKey = getWeekDayResourceKey(getDayOfWeek(date));
			WeekdayNumber = getDayOfWeek(date);
			YearWeek = getYearWeek(date, culture);
			Quarter = getQuarter(date);
			InsertDate = DateTime.UtcNow;
		}

		public virtual int Year { get; set; }
		public virtual int YearMonth { get; set; }
		public virtual int Month { get; set; }
		public virtual string MonthName { get; set; }
		public virtual string MonthResourceKey { get; set; }
		public virtual int DayInMonth { get; set; }
		public virtual int WeekdayNumber { get; set; }
		public virtual string WeekdayName { get; set; }
		public virtual string WeekdayResourceKey { get; set; }
		public virtual int WeekNumber { get; set; }
		public virtual string YearWeek { get; set; }
		public virtual string Quarter { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public static IAnalyticsDate Eternity = new AnalyticsDate { DateDate = new DateTime(2059, 12, 31), DateId = -2 };
		public static IAnalyticsDate NotDefined = new AnalyticsDate { DateDate = new DateTime(1900, 01, 01), DateId = -1 };

		private static string getMonthResourceKey(int month)
		{
			switch (month)
			{
				case 1:
					return "ResMonthJanuary";
				case 2:
					return "ResMonthFebruary";
				case 3:
					return "ResMonthMarch";
				case 4:
					return "ResMonthApril";
				case 5:
					return "ResMonthMay";
				case 6:
					return "ResMonthJune";
				case 7:
					return "ResMonthJuly";
				case 8:
					return "ResMonthAugust";
				case 9:
					return "ResMonthSeptember";
				case 10:
					return "ResMonthOctober";
				case 11:
					return "ResMonthNovember";
				case 12:
					return "ResMonthDecember";
				default:
					return "";
			}
		}

		private static string getWeekDayResourceKey(int weekDayNumber)
		{
			switch (weekDayNumber)
			{
				case 1:
					return "ResDayOfWeekMonday";
				case 2:
					return "ResDayOfWeekTuesday";
				case 3:
					return "ResDayOfWeekWednesday";
				case 4:
					return "ResDayOfWeekThursday";
				case 5:
					return "ResDayOfWeekFriday";
				case 6:
					return "ResDayOfWeekSaturday";
				case 7:
					return "ResDayOfWeekSunday";
				default:
					return "";
			}
		}

		private static int getDayOfWeek(DateTime date)
		{
			//In data mart Sunday = 0 and Saturday = 6.
			var ret = (int)date.DayOfWeek;
			if (ret == 0) ret = 7;
			return ret;
		}

		private static string getYearWeek(DateTime date, CultureInfo culture)
		{
			var weekNumber = DateHelper.WeekNumber(date, culture);
			var datePart = weekNumber.ToString(culture);
			if (datePart.Length < 2)
				datePart = string.Concat("0", datePart);

			var year = date.Year;
			if (date.Day <= 6 && weekNumber > 51)
				year -= 1;
			if (date.Day >= 26 && weekNumber == 1)
				year += 1;

			return string.Format(CultureInfo.InvariantCulture, "{0}{1}", year, datePart);
		}

		private static string getQuarter(DateTime date) { return string.Concat(date.Year.ToString(CultureInfo.InvariantCulture), "Q", DateHelper.GetQuarter(date.Month)); }
	}
}
