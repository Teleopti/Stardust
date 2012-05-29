using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class DayOffPreference : BasePreference
	{
		private readonly int _thOfDay;
		public IDayOffTemplate DayOffTemplate = TestData.DayOffTemplate;

		public DayOffPreference() : this(0)
		{
		}

		public DayOffPreference(int thOfDay)
		{
			_thOfDay = thOfDay;
		}

		protected override PreferenceRestriction ApplyRestriction() { return new PreferenceRestriction {DayOffTemplate = DayOffTemplate}; }
		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo).AddDays(_thOfDay); }
	}
}