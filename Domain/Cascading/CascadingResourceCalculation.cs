using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper, Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void ForDay(DateOnly date)
		{
			//REMOVE LATER ////
			var persons = _schedulerStateHolder().AllPermittedPersons;

			foreach (var person in persons)
			{
				var orderedCascadingSkills = person.Period(date).PersonSkillCollection
					.Where(x => x.Skill.IsCascading() && x.Active).OrderBy(x => x.Skill.CascadingIndex).ToArray();
				if(!orderedCascadingSkills.Any())
					continue;

				foreach (var cascadingSkill in orderedCascadingSkills)
				{
					((IPersonSkillModify)cascadingSkill).Active = false;
				}
				foreach (var activity in orderedCascadingSkills.Select(personSkill => personSkill.Skill.Activity).Distinct())
				{
					var prioritizedCascadingSkillForActivity = (IPersonSkillModify)orderedCascadingSkills.First(s => s.Skill.Activity.Equals(activity));
					prioritizedCascadingSkillForActivity.Active = true;
				}
			}
			/////

			_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later
		}
	}
}