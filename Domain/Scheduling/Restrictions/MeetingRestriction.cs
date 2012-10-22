using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	public class MeetingRestriction : IWorkTimeMinMaxRestriction
	{
		private readonly IEnumerable<PersonMeeting> _personMeetings;

		public MeetingRestriction(IEnumerable<PersonMeeting> personMeetings)
		{
			_personMeetings = personMeetings;
		}

		public bool MayMatch()
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
			foreach (var meeting in _personMeetings)
			{
				foreach (var layer in workShiftProjection.Layers)
				{
					if (meeting.Period.Intersect(layer.Period) && !(layer as WorkShiftProjectionLayer).AllowOverwrite)
						return false;
				}
			}
			return true;
		}
	}
}