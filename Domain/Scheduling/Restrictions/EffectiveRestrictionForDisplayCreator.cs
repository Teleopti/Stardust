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

			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(startTime, endEndTime), new WorkTimeLimitation(startTime, endTime), null, null, null, new List<IActivityRestriction>());

			//var extractor  = new RestrictionExtractorWithoutStateHolder();
			//extractor.Extract(scheduleDay);
			//var effectiveRestriction = extractor.CombinedRestriction(
			//    new SchedulingOptions
			//        {
			//            UseAvailability = effectiveRestrictionOptions.UseAvailability,
			//            UsePreferences = effectiveRestrictionOptions.UsePreference,
			//        }
			//    );

			if (scheduleDay != null && effectiveRestrictionOptions != null)
			{
				if (effectiveRestrictionOptions.UsePreference)
				{
					effectiveRestriction = scheduleDay.RestrictionCollection().OfType<IPreferenceRestriction>()
						.Aggregate(effectiveRestriction,
						           (current, preferenceRestriction) =>
						           	{
						           		var restriction = new EffectiveRestriction(
						           			preferenceRestriction.StartTimeLimitation,
						           			preferenceRestriction.EndTimeLimitation,
						           			preferenceRestriction.WorkTimeLimitation,
						           			preferenceRestriction.ShiftCategory,
						           			preferenceRestriction.DayOffTemplate,
						           			preferenceRestriction.Absence,
						           			preferenceRestriction.ActivityRestrictionCollection
						           			);
						           		return (EffectiveRestriction) current.Combine(restriction);
						           	}
						);
				}

				if (effectiveRestrictionOptions.UseAvailability)
				{
					foreach (var availabilityRestriction in scheduleDay.RestrictionCollection().OfType<IAvailabilityRestriction>())
					{
						if (availabilityRestriction.NotAvailable)
							effectiveRestriction =
								(EffectiveRestriction)effectiveRestriction.Combine(new EffectiveRestriction(availabilityRestriction.StartTimeLimitation,
								                                                      availabilityRestriction.EndTimeLimitation,
								                                                      availabilityRestriction.WorkTimeLimitation, null,
								                                                      new DayOffTemplate(new Description("Not available", "N/A")),
								                                                      null, new List<IActivityRestriction>()));
						else
						{
							effectiveRestriction =
								(EffectiveRestriction)effectiveRestriction.Combine(new EffectiveRestriction(availabilityRestriction.StartTimeLimitation,
																				  availabilityRestriction.EndTimeLimitation,
																				  availabilityRestriction.WorkTimeLimitation, null,
																				  null,
																				  null, new List<IActivityRestriction>()));
						}
					}
				}
			}

			return effectiveRestriction;
		}
	}
}