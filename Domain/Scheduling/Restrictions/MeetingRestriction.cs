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
			var intersectingActivities = from m in _personMeetings
										 from l in workShiftProjection.Layers
										 let t = m.Person.PermissionInformation.DefaultTimeZone()
										 let layerPeriod = new TimePeriod(l.Period.StartDateTime.Subtract(WorkShift.BaseDate), l.Period.EndDateTime.Subtract(WorkShift.BaseDate))
										 where m.Period.TimePeriod(t).Intersect(layerPeriod)
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