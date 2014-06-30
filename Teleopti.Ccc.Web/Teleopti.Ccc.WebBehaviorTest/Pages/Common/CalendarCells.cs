using System;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	internal static class CalendarCells
	{
		public static string DateSelector(string formattedDate) { return "li[data-mytime-date='" + formattedDate + "']"; }
		
		public static string DateSelector(DateTime date) { return DateSelector(date.ToString("yyyy-MM-dd")); }
	}
}