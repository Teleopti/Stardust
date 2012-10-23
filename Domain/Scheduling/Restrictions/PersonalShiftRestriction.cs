using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class PersonalShiftRestriction : IWorkTimeMinMaxRestriction
	{
		private readonly IEnumerable<IPersonAssignment> _personAssignments;

		public PersonalShiftRestriction(IEnumerable<IPersonAssignment> personAssignments)
		{
			_personAssignments = personAssignments;
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
			var intersectingActivities = from m in _personAssignments
			                             from l in workShiftProjection.Layers
			                             from n in m.PersonalShiftCollection
			                             from k in n.LayerCollection
			                             where k.Period.TimePeriod(timeZone).Intersect(l.Period.TimePeriod(timeZone))
			                             select l;
			return intersectingActivities.All(x => x.ActivityAllowsOverwrite);
		}


		public override int GetHashCode()
		{
			var result = 0;
			foreach (var assignment in _personAssignments)
			{
				result = (result * 398) ^ assignment.GetHashCode();
			}
			return result;
		}
	}
}