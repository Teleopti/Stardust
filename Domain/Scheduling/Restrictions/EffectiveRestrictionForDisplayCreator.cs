using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionForDisplayCreator : IEffectiveRestrictionForDisplayCreator
	{
		public IEffectiveRestriction GetEffectiveRestrictionForDisplay(IScheduleDay scheduleDay, IEffectiveRestrictionOptions effectiveRestrictionOptions)
		{
			var startTime = new TimeSpan(0, 0, 0);
			var endTime = new TimeSpan(23, 59, 59);
			var endEndTime = new TimeSpan(1, 23, 59, 59);

			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(startTime, endEndTime), new WorkTimeLimitation(startTime, endTime), null, null, null, new List<IActivityRestriction>());

			if (effectiveRestrictionOptions.UsePreference)
			{
				effectiveRestriction = scheduleDay.RestrictionCollection().OfType<IPreferenceRestriction>().Aggregate(effectiveRestriction, (current, preferenceRestriction) => (EffectiveRestriction) current.Combine(new EffectiveRestriction(preferenceRestriction.StartTimeLimitation, preferenceRestriction.EndTimeLimitation, preferenceRestriction.WorkTimeLimitation, preferenceRestriction.ShiftCategory, preferenceRestriction.DayOffTemplate, preferenceRestriction.Absence, preferenceRestriction.ActivityRestrictionCollection)));
			}


			return effectiveRestriction;
		}

		public IEffectiveRestriction GetEffectiveRestrictionForDisplay()
		{
			return null;
		}
	}
}