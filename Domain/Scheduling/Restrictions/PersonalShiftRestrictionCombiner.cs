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

			IList<IEffectiveRestriction> asEffectiveRestrictions = new List<IEffectiveRestriction>();
			foreach (var p in periods)
			{
				var workTime = p.TimePeriod(timeZoneInfo).SpanningTime();
				if (workTime >= TimeSpan.FromHours(24)) workTime = new TimeSpan(23, 59, 59);
				asEffectiveRestrictions.Add(new EffectiveRestriction(
														 new StartTimeLimitation(null, p.TimePeriod(timeZoneInfo).StartTime),
														 new EndTimeLimitation(p.TimePeriod(timeZoneInfo).EndTime, null),
														 new WorkTimeLimitation(workTime, null),
														 null,
														 null,
														 null,
														 new List<IActivityRestriction>()));
			}

			return _combiner.CombineEffectiveRestrictions(asEffectiveRestrictions, effectiveRestriction);
		}
	}
}
