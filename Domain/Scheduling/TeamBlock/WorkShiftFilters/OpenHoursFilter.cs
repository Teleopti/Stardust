using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays, IPerson person, DateOnly datePointer)
		{
			var skillDays = allSkillDays.Where(x => x.CurrentDate == datePointer || x.CurrentDate == datePointer.AddDays(-1) || x.CurrentDate == datePointer.AddDays(1));

			var agentTimeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			return shifts.Select(shiftProjectionCache => new
			{
				shiftProjectionCache,
				isClosed = shiftProjectionCache.MainShiftProjection.Any(
					visualLayer => !_isAnySkillOpen.Check(skillDays, visualLayer, agentTimeZoneInfo))
			}).Where(t => !t.isClosed).Select(t => t.shiftProjectionCache).ToList();
		}
	}
}
