using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class ActivityRequiresSkillProjectionFilter
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public ActivityRequiresSkillProjectionFilter(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IList<ShiftProjectionCache> Filter(IPerson person, IList<ShiftProjectionCache> shiftList, DateOnly dateToCheck)
		{
			if (person == null) return null;
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var personPeriod = person.Period(dateToCheck);
			if (personPeriod == null) return shiftList;
			
			var validActivities = _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(personPeriod).Select(p => p.Skill.Activity).ToHashSet();
			var workShiftsWithValidActivities = shiftList.Where(s =>
			{
				var projectedLayersRequiringSkill = s.MainShiftProjection().Select(l => l.Payload).OfType<IActivity>().Where(a => a.RequiresSkill);
				return projectedLayersRequiringSkill.All(validActivities.Contains);
			}).ToList();
			
			return workShiftsWithValidActivities.Count == 0 ? new List<ShiftProjectionCache>() : workShiftsWithValidActivities;
		}
	}
}