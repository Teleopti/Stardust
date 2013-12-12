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

			var periods = from l in assignment.PersonalLayers()
			              select l.Period;

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
