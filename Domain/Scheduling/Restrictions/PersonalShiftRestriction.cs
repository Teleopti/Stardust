using System.Collections.Generic;
using System.Linq;
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
			var intersectingActivities = from m in _personAssignments
			                             from l in workShiftProjection.Layers
										 let t = m.Person.PermissionInformation.DefaultTimeZone()
			                             from shift in m.PersonalShiftCollection
			                             from k in shift.LayerCollection
										 let layerPeriod = new TimePeriod(l.Period.StartDateTime.Subtract(WorkShift.BaseDate), l.Period.EndDateTime.Subtract(WorkShift.BaseDate))
										 where k.Period.TimePeriod(t).Intersect(layerPeriod)
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