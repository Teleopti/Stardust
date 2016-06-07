using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CascadingPersonalSkills
	{
		public IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period)
		{
			var all = period.PersonSkillCollection.Where(x => !((IDeleteTag)x.Skill).IsDeleted && x.Active).ToList();
			var cascading = period.CascadingSkills().ToArray();
			var activities = cascading.Select(personSkill => personSkill.Skill.Activity).Distinct();
			return all
				.Except(cascading)
				.Union(activities.Select(activity => activity == null ? 
					cascading.First(s => s.Skill.Activity == null) : 
					cascading.First(s => s.Skill.Activity.Equals(activity))));
		}
	}
}