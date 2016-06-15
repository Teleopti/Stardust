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
			/*
			 * 
			 * 
			var all = period.PersonSkillCollection.Where(x => !((IDeleteTag)x.Skill).IsDeleted && x.Active).ToList();
-			var cascading = period.CascadingSkills().ToArray();
-			var activities = cascading.Select(personSkill => personSkill.Skill.Activity).Distinct();
-			return all
-				.Except(cascading)
-				.Union(activities.Select(activity => activity == null ? 
-					cascading.First(s => s.Skill.Activity == null) : 
-					cascading.First(s => s.Skill.Activity.Equals(activity))));*/


			var all = period.PersonSkillCollection.Where(x => !((IDeleteTag)x.Skill).IsDeleted && x.Active).ToList();
			var cascadingSkills = period.CascadingSkills().ToArray();
			var activities = cascadingSkills.Select(personSkill => personSkill.Skill.Activity).Distinct();
			var allExceptCascading = all.Except(cascadingSkills);

			var primSkills = primarySkills(activities, cascadingSkills);
			return allExceptCascading.Union(primSkills);
		}

		private static IEnumerable<IPersonSkill> primarySkills(IEnumerable<IActivity> activities, IPersonSkill[] cascadingSkills)
		{
			if (!cascadingSkills.Any())
				return Enumerable.Empty<IPersonSkill>();

			//var lowestCascading = cascadingSkills.Min(x => x.Skill.CascadingIndex.Value);
			var ret = new List<IPersonSkill>();
			foreach (var activity in activities)
			{
				var cascadingSkillsWithCorrectActivity = cascadingSkills.Where(x => personSkillHasActivity(x, activity));
				var lowestCascading = cascadingSkillsWithCorrectActivity.Min(x => x.Skill.CascadingIndex.Value);
				ret.AddRange(cascadingSkillsWithCorrectActivity.Where(x => x.Skill.CascadingIndex.Value == lowestCascading));
			}
			return ret;
		}

		//move to IPersonSkill
		private static bool personSkillHasActivity(IPersonSkill personSkill, IActivity activity)
		{
			return activity == null ? 
				personSkill.Skill.Activity == null : 
				personSkill.Skill.Activity.Equals(activity);
		}
	}
}
 