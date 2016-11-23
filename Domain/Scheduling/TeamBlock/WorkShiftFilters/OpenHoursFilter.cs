using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class OpenHoursFilter
	{
		private readonly IsAnySkillOpen _isAnySkillOpen;

		public OpenHoursFilter(IsAnySkillOpen isAnySkillOpen)
		{
			_isAnySkillOpen = isAnySkillOpen;
		}

		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays, IPerson person, DateOnly datePointer)
		{
			IList<IShiftProjectionCache> openShifts = new List<IShiftProjectionCache>();
			var skillDays = allSkillDays.Where(x => x.CurrentDate == datePointer || x.CurrentDate == datePointer.AddDays(-1) || x.CurrentDate == datePointer.AddDays(1));
			foreach (var shiftProjectionCache in shifts)
			{
				var isClosed = false;
				foreach (var visualLayer in shiftProjectionCache.MainShiftProjection)
				{
					if (!_isAnySkillOpen.Check(skillDays, visualLayer, person.PermissionInformation.DefaultTimeZone()))
					{
						isClosed = true;
						break;
					}
				}

				if (!isClosed)
				{
					openShifts.Add(shiftProjectionCache);
				}
			}

			return openShifts;
		}
	}
}
