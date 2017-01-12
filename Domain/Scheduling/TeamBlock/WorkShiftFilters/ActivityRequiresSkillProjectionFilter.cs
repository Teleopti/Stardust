using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class ActivityRequiresSkillProjectionFilter : IActivityRequiresSkillProjectionFilter
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public ActivityRequiresSkillProjectionFilter(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IList<IShiftProjectionCache> Filter(IPerson person, IList<IShiftProjectionCache> shiftList, DateOnly dateToCheck, IWorkShiftFinderResult finderResult)
		{
			if (person == null) return null;
			if (shiftList == null) return null;
			if (finderResult == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var personPeriod = person.Period(dateToCheck);
			if (personPeriod == null) return shiftList;
			
			var validActivities = _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(personPeriod).Select(p => p.Skill.Activity).ToArray();
			var workShiftsWithValidActivities = shiftList.AsParallel().Where(s =>
			{
				var projectedLayersRequiringSkill = s.MainShiftProjection.Select(l => l.Payload).OfType<IActivity>().Where(a => a.RequiresSkill);
				return projectedLayersRequiringSkill.All(validActivities.Contains);
			}).ToList();
			
			return workShiftsWithValidActivities.Count == 0 ? filterResults(shiftList, finderResult) : workShiftsWithValidActivities;
		}

		private static IList<IShiftProjectionCache> filterResults(IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(UserTexts.Resources.AfterCheckingAgainstActivities,
					shiftList.Count, 0));
			return new List<IShiftProjectionCache>();
		}
	}
}