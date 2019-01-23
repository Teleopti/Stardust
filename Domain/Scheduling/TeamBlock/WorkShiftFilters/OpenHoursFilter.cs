using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class OpenHoursFilter
	{
		private readonly IsAnySkillOpen _isAnySkillOpen;
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public OpenHoursFilter(IsAnySkillOpen isAnySkillOpen, PersonalSkillsProvider personalSkillsProvider)
		{
			_isAnySkillOpen = isAnySkillOpen;
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays, IPerson person, DateOnly datePointer)
		{
			var agentTimeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			var personalSkills = _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(person.Period(datePointer)).Select(s => s.Skill).ToHashSet();
			var period = datePointer.ToDateOnlyPeriod().Inflate(1);
			var skillDays = allSkillDays.Where(x => period.Contains(x.CurrentDate) && personalSkills.Contains(x.Skill)).ToList();

			return shifts.Select(shiftProjectionCache => new
			{
				shiftProjectionCache,
				isClosed = shiftProjectionCache.MainShiftProjection().Any(
					visualLayer => !_isAnySkillOpen.Check(skillDays, visualLayer, agentTimeZoneInfo))
			}).Where(t => !t.isClosed).Select(t => t.shiftProjectionCache).ToList();
		}
	}
}
