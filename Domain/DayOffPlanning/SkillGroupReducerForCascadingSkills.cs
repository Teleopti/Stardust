using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class SkillGroupReducerForCascadingSkills
	{
		public void ReduceToPrimarySkill(IPersonPeriod personPeriod)
		{
			var skillList = personPeriod.PersonSkillCollection.Select(personSkill => personSkill.Skill).OrderBy(skill => skill.Name);
			if (!skillList.Any())
				return;
			var activityList = skillList.Select(s => s.Activity).Distinct().ToList();
			foreach (var activity in activityList)
			{
				var primarySkill = skillList.First(skill => skill.Activity.Equals(activity));
				var activity1 = activity;
				foreach (var personSkill in personPeriod.PersonSkillCollection.Where(ps => ps.Skill.Activity.Equals(activity1)))
				{
					var personSkillModify = (IPersonSkillModify)personSkill;
					personSkillModify.Active = false;

					if (personSkill.Skill.Equals(primarySkill))
						personSkillModify.Active = true;
				}
			}		
		}
	}
}