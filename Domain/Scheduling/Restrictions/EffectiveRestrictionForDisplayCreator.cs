using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

			var combiner = new RestrictionCombiner();

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(startTime, endEndTime), new WorkTimeLimitation(startTime, endTime), null, null, null, new List<IActivityRestriction>());

			if (scheduleDay != null && effectiveRestrictionOptions != null)
			{
				if (effectiveRestrictionOptions.UsePreference)
				{
					effectiveRestriction = combiner.CombinePreferenceRestrictions(
						scheduleDay.RestrictionCollection().OfType<IPreferenceRestriction>(), 
						effectiveRestriction, false);
				}

				if (effectiveRestrictionOptions.UseAvailability)
				{
					effectiveRestriction = combiner.CombineAvailabilityRestrictions(
						scheduleDay.RestrictionCollection().OfType<IAvailabilityRestriction>(),
						effectiveRestriction);
				}
			}

			return effectiveRestriction;
		}
	}
}