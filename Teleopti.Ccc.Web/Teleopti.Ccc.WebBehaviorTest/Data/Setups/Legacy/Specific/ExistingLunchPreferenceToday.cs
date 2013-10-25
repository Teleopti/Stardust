using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class ExistingLunchPreferenceToday : BasePreference
	{
		private readonly StartTimeLimitation _start;
		private readonly EndTimeLimitation _end;
		private readonly WorkTimeLimitation _length;

		public ExistingLunchPreferenceToday(WorkTimeLimitation workTimeLimitation)
		{
			_start = new StartTimeLimitation();
			_end = new EndTimeLimitation();
			_length = workTimeLimitation;
		}

		public ExistingLunchPreferenceToday(EndTimeLimitation endTimeLimitation)
		{
			_start = new StartTimeLimitation();
			_end = endTimeLimitation;
			_length = new WorkTimeLimitation();
		}

		public ExistingLunchPreferenceToday(StartTimeLimitation startTimeLimitation)
		{
			_start = startTimeLimitation;
			_end = new EndTimeLimitation();
			_length = new WorkTimeLimitation();
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
            return DateOnlyForBehaviorTests.TestToday;
		}
	}
}