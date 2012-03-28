using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ExistingLunchPreferenceToday : BasePreference
	{
		private readonly StartTimeLimitation _start;
		private readonly EndTimeLimitation _end;
		private readonly WorkTimeLimitation _length;

		public ExistingLunchPreferenceToday(int start, int end, int length)
		{
			_start = start == 0 ? new StartTimeLimitation() : new StartTimeLimitation(new TimeSpan(start, 0, 0), new TimeSpan(start, 0, 0));
			_end = end == 0 ? new EndTimeLimitation() : new EndTimeLimitation(new TimeSpan(end, 0, 0), new TimeSpan(end, 0, 0));
			_length = length == 0 ? new WorkTimeLimitation() : new WorkTimeLimitation(new TimeSpan(length, 0, 0), new TimeSpan(length, 0, 0));
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			var activityRestriction = new ActivityRestriction(TestData.ActivityLunch)
			                          	{StartTimeLimitation = _start, EndTimeLimitation = _end, WorkTimeLimitation = _length};
			var preferenceRestriction = new PreferenceRestriction();
			preferenceRestriction.AddActivityRestriction(activityRestriction);
			return preferenceRestriction;
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Now;
		}
	}
}