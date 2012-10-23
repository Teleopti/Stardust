using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class MeetingRestriction : IWorkTimeMinMaxRestriction
	{
		private readonly IEnumerable<IPersonMeeting> _personMeetings;

		public MeetingRestriction(IEnumerable<IPersonMeeting> personMeetings)
		{
			_personMeetings = personMeetings;
		}

		public bool MayMatchWithShifts()
		{
			return true;
		}

		public bool MayMatchBlacklistedShifts()
		{
			return false;
		}

		public bool Match(IShiftCategory shiftCategory)
		{
			return true;
		}

		public bool Match(IWorkShiftProjection workShiftProjection)
		{
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.Utc);
			var intersectingActivities = from m in _personMeetings
			                             from l in workShiftProjection.Layers
			                             where m.Period.TimePeriod(timeZone).Intersect(l.Period.TimePeriod(timeZone))
			                             select l;
			return intersectingActivities.All(x => x.ActivityAllowsOverwrite);
		}





		public override int GetHashCode()
		{
			var result = 0;
			foreach (var meeting in _personMeetings)
			{
				result = (result * 398) ^ meeting.GetHashCode();
			}
			return result;
		}
	}
}