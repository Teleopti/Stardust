using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionForMeeting : IEffectiveRestrictionForMeeting
	{
		public IEffectiveRestriction AddEffectiveRestriction(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
		{
			if (scheduleDay == null)
				throw new ArgumentNullException("scheduleDay");

			if (effectiveRestriction == null)
				return null;

			var meetings = scheduleDay.PersonMeetingCollection();

			if (meetings.IsEmpty())
				return effectiveRestriction;

			//inte på parten här??????????
			IPerson person = scheduleDay.Person;
			ICccTimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

			foreach (IPersonMeeting meeting in meetings)
			{
				DateTimePeriod personalShiftPeriod = meeting.Period;
				var personalShiftRestriction = new EffectiveRestriction(
					new StartTimeLimitation(null, personalShiftPeriod.TimePeriod(timeZoneInfo).StartTime),
					new EndTimeLimitation(personalShiftPeriod.TimePeriod(timeZoneInfo).EndTime, null),
					new WorkTimeLimitation(personalShiftPeriod.TimePeriod(timeZoneInfo).SpanningTime(), null), null,
					null, null, new List<IActivityRestriction>());
				effectiveRestriction = effectiveRestriction.Combine(personalShiftRestriction);
			}
			return effectiveRestriction;
		}
	}
}