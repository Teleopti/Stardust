using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IEffectiveRestriction Combine(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
		{
			if (scheduleDay == null)
				throw new ArgumentNullException("scheduleDay");

			if (effectiveRestriction == null)
				return null;

			var assignments = scheduleDay.PersonAssignmentCollection();

			if (assignments == null || assignments.IsEmpty())
				return effectiveRestriction;

			var person = scheduleDay.Person;
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

			var periods = from a in assignments
			              from s in a.PersonalShiftCollection
			              from l in s.LayerCollection
			              select l.Period
				;

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
