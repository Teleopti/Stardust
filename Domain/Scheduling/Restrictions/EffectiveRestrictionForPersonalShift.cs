using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionForPersonalShift : IEffectiveRestrictionAdder
	{
		public IEffectiveRestriction AddEffectiveRestriction(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
		{
			if (scheduleDay == null)
				throw new ArgumentNullException("scheduleDay");

			if (effectiveRestriction == null)
				return null;

			if (scheduleDay.PersonAssignmentCollection().IsEmpty())
				return effectiveRestriction;

			//inte på parten här??????????
			IPerson person = scheduleDay.Person;
			ICccTimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

			foreach (IPersonAssignment assignment in scheduleDay.PersonAssignmentCollection())
			{
				foreach (IPersonalShift shift in assignment.PersonalShiftCollection)
				{
					var personalShiftPeriod = shift.LayerCollection.Period();
					if (!personalShiftPeriod.HasValue) continue;
					var personalShiftRestriction = new EffectiveRestriction(
						new StartTimeLimitation(null, personalShiftPeriod.Value.TimePeriod(timeZoneInfo).StartTime),
						new EndTimeLimitation(personalShiftPeriod.Value.TimePeriod(timeZoneInfo).EndTime, null),
						new WorkTimeLimitation(personalShiftPeriod.Value.TimePeriod(timeZoneInfo).SpanningTime(), null),
						null, null, null, new List<IActivityRestriction>());
					effectiveRestriction = effectiveRestriction.Combine(personalShiftRestriction);
				}
			}
			return effectiveRestriction;
		}
	}
}
