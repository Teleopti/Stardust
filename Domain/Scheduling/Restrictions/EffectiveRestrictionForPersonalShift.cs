using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionForPersonalShift : IEffectiveRestrictionForPersonalShift
	{
		public IEffectiveRestriction AddEffectiveRestriction(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
		{
			if (scheduleDay == null)
				throw new ArgumentNullException("scheduleDay");

			if (effectiveRestriction == null)
				return null;

			var assignments = scheduleDay.PersonAssignmentCollection();

			if (assignments.IsEmpty())
				return effectiveRestriction;

			//inte på parten här??????????
			IPerson person = scheduleDay.Person;
			ICccTimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

			foreach (IPersonAssignment assignment in assignments)
			{
				effectiveRestriction = (from shift in assignment.PersonalShiftCollection
				                        select shift.LayerCollection.Period()
				                        into personalShiftPeriod where personalShiftPeriod.HasValue 
										select new EffectiveRestriction
											(new StartTimeLimitation(null, personalShiftPeriod.Value.TimePeriod(timeZoneInfo).StartTime), new EndTimeLimitation(personalShiftPeriod.Value.TimePeriod(timeZoneInfo).EndTime, null), new WorkTimeLimitation(personalShiftPeriod.Value.TimePeriod(timeZoneInfo).SpanningTime(), null), null, null, null, new List<IActivityRestriction>())).Aggregate(effectiveRestriction, (current, personalShiftRestriction) => current.Combine(personalShiftRestriction));
			}
			return effectiveRestriction;
		}
	}
}
