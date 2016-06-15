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
			var cascadingSkills = period.CascadingSkills().ToArray();
			var allExceptCascading = all.Except(cascadingSkills);

			var primSkills = primarySkills(cascadingSkills);
			return allExceptCascading.Union(primSkills);
		}

		private static IEnumerable<IPersonSkill> primarySkills(IPersonSkill[] cascadingSkills)
		{
			if (!cascadingSkills.Any())
				return Enumerable.Empty<IPersonSkill>();

			var activities = cascadingSkills.Select(personSkill => personSkill.Skill.Activity).Distinct();
			var ret = new List<IPersonSkill>();
			foreach (var activity in activities)
			{
				var cascadingSkillsWithCorrectActivity = cascadingSkills.Where(x => x.HasActivity(activity)).ToArray();
				var lowestCascading = cascadingSkillsWithCorrectActivity.Min(x => x.Skill.CascadingIndex.Value);
				ret.AddRange(cascadingSkillsWithCorrectActivity.Where(x => x.Skill.CascadingIndex.Value == lowestCascading));
			}
			return ret;
		}
	}
}
 