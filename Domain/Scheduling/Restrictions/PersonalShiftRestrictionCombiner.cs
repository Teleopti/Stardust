using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class PersonalShiftRestrictionCombiner : IPersonalShiftRestrictionCombiner
	{
		private readonly IEffectiveRestrictionCombiner _combiner;

		public PersonalShiftRestrictionCombiner(IEffectiveRestrictionCombiner combiner)
		{
			_combiner = combiner;
		}

		public IEffectiveRestriction Combine(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
		{
			if (scheduleDay == null)
				throw new ArgumentNullException("scheduleDay");

			if (effectiveRestriction == null)
				return null;

			var assignment = scheduleDay.PersonAssignment();

			if (assignment == null)
				return effectiveRestriction;

			var person = scheduleDay.Person;
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

			var periods = from l in assignment.PersonalActivities()
			              select l.Period;

			var asEffectiveRestrictions = from p in periods
										  select new EffectiveRestriction(
													 new StartTimeLimitation(null, p.TimePeriod(timeZoneInfo).StartTime),
													 new EndTimeLimitation(p.TimePeriod(timeZoneInfo).EndTime, null),
													 new WorkTimeLimitation(p.TimePeriod(timeZoneInfo).SpanningTime(), null),
													 null,
													 null,
													 null,
													 new List<IActivityRestriction>())
												 as IEffectiveRestriction
				;

			return _combiner.CombineEffectiveRestrictions(asEffectiveRestrictions, effectiveRestriction);
		}
	}
}
