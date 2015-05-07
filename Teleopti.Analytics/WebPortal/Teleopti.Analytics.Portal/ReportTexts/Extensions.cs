using System;

namespace Teleopti.Analytics.ReportTexts
{
	public static class Extensions
	{
		public static string GetMonthResourceKey(this int month)
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

		public static string GetWeekDayResourceKey(this DayOfWeek weekDayNumber)
		{
			var number = (int) weekDayNumber;
			if (weekDayNumber == DayOfWeek.Sunday)
				number = 7;
			return number.GetWeekDayResourceKey();
		}

		public static string GetWeekDayResourceKey(this int weekDayNumber)
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
	}
}
