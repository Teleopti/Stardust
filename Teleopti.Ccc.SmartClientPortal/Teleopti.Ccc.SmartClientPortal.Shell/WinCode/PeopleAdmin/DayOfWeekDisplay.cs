using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin
{
	public class DayOfWeekDisplay : IComparable
	{
		public DayOfWeek DayOfWeek { get; }
		public string DisplayName { get; }

		public DayOfWeekDisplay(DayOfWeek dayOfWeek, string displayName)
		{
			DayOfWeek = dayOfWeek;
			DisplayName = displayName;
		}

		public static IList<DayOfWeekDisplay> ListOfDayOfWeek => DateHelper.GetDaysOfWeek(CultureInfo.CurrentUICulture).Select(dayOfWeek =>
			new DayOfWeekDisplay(dayOfWeek, CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(dayOfWeek))).ToList();

		public int CompareTo(object other)
		{
			var o = other as DayOfWeekDisplay;
			return o != null ? string.Compare(DisplayName, o.DisplayName, true, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture) : -1;
		}
	}
}