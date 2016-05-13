using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CascadingPersonSkillProvider : PersonSkillProvider
	{
		protected override IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod personPeriod)
		{
			var all = personPeriod.PersonSkillCollection.Where(x => !((IDeleteTag)x.Skill).IsDeleted).ToList();
			var cascading = personPeriod.CascadingSkills().ToArray();
			var activities = cascading.Select(personSkill => personSkill.Skill.Activity).Distinct();
			return all.Except(cascading).Union(activities.Select(activity => cascading.First(s => s.Skill.Activity.Equals(activity))));
		}
	}
}