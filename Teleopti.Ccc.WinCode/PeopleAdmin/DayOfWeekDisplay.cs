using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
	public class DayOfWeekDisplay : IComparable
	{
		public DayOfWeek DayOfWeek { get; private set; }
		public string DisplayName { get; private set; }

		public DayOfWeekDisplay(DayOfWeek dayOfWeek, string displayName)
		{
			DayOfWeek = dayOfWeek;
			DisplayName = displayName;
		}

		public static IList<DayOfWeekDisplay> ListOfDayOfWeek
		{
		get
		{
			var ret = new List<DayOfWeekDisplay>();
			IList<DayOfWeek> dayOfWeekCollection = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);

			foreach (var dayOfWeek in dayOfWeekCollection)
			{
				ret.Add(new DayOfWeekDisplay(dayOfWeek, CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dayOfWeek)));
			}

			return ret;
		}
			
		}

		public int CompareTo(object other)
		{
			var o = other as DayOfWeekDisplay;
			return o != null ? string.Compare(DisplayName, o.DisplayName, true, TeleoptiPrincipal.Current.Regional.Culture) : -1;
		}
		
	}
}