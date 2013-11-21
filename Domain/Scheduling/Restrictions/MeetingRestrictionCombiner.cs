using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class MeetingRestrictionCombiner : IMeetingRestrictionCombiner
	{
		private readonly IEffectiveRestrictionCombiner _combiner;

		public MeetingRestrictionCombiner(IEffectiveRestrictionCombiner combiner)
		{
			_combiner = combiner;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IEffectiveRestriction Combine(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
		{
			if (effectiveRestriction == null)
				return null;

			var meetings = scheduleDay.PersonMeetingCollection();

			if (meetings == null || meetings.IsEmpty())
				return effectiveRestriction;

			var person = scheduleDay.Person;
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

			var asEffectiveRestrictions = from m in meetings
			                              let meetingPeriod = m.Period
			                              let period = meetingPeriod.TimePeriod(timeZoneInfo)
			                              select new EffectiveRestriction(
				                                     new StartTimeLimitation(null, period.StartTime),
				                                     new EndTimeLimitation(period.EndTime, null),
				                                     new WorkTimeLimitation(period.SpanningTime(), null),
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