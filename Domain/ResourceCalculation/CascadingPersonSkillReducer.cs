using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CascadingPersonSkillReducer : IPersonSkillReducer
	{
		public IEnumerable<IPersonSkill> Reduce(IPersonPeriod personPeriod)
		{
			var all = personPeriod.PersonSkillCollection.Where(x => !((IDeleteTag)x.Skill).IsDeleted).ToList();
			var cascading = personPeriod.CascadingSkills().ToArray();
			var activities = cascading.Select(personSkill => personSkill.Skill.Activity).Distinct();
			var toBeUsed = all.Except(cascading).ToList();
			toBeUsed.AddRange(activities.Select(activity => cascading.First(s => s.Skill.Activity.Equals(activity))));
			return toBeUsed;
		}
	}
}