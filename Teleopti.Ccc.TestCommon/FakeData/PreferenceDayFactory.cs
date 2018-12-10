using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PreferenceDayFactory
	{
		public static IPreferenceDay CreatePreferenceDay(DateOnly date, IPerson person, IActivity activity)
		{
			return CreatePreferenceDayWithoutMustHave(date, person, activity, new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0),
				false);
		}

		public static IPreferenceDay CreatePreferenceDayWithoutMustHave(DateOnly date, IPerson person, IActivity activity,
			TimeSpan startTime, TimeSpan endTime, bool mustHave)
		{
			var preferenceRestrictionNew = new PreferenceRestriction();
			var preferenceDay = new PreferenceDay(person, date, preferenceRestrictionNew);
			var activityRestriction = new ActivityRestriction(activity)
			{
				StartTimeLimitation = new StartTimeLimitation(startTime, endTime)
			};
			preferenceDay.Restriction.AddActivityRestriction(activityRestriction);
			preferenceDay.Restriction.MustHave = true;
			preferenceDay.TemplateName = "My template";

			return preferenceDay;
		}
	}
}
