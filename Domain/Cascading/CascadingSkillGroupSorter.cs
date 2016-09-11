using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroupSorter : IComparer<CascadingSkillGroup>
	{
		public int Compare(CascadingSkillGroup first, CascadingSkillGroup second)
		{
			var primaryDiff = second.PrimarySkillsCascadingIndex - first.PrimarySkillsCascadingIndex;
			if (primaryDiff != 0)
			{
				return primaryDiff;
			}

			var primaryCountDiff = second.PrimarySkills.Count() - first.PrimarySkills.Count();
			if (primaryCountDiff != 0)
			{
				return primaryCountDiff;
			}

			var firstSubSkill = first.SubSkillsWithSameIndex.ToArray();
			var secondSubSkill = second.SubSkillsWithSameIndex.ToArray();
			for (var i = 0; i < firstSubSkill.Length; i++)
			{
				if (i > secondSubSkill.Length - 1)
				{
					return 0;
				}
				var subSkillDiff = firstSubSkill[i].CascadingIndex - secondSubSkill[i].CascadingIndex;
				if (subSkillDiff != 0)
				{
					return subSkillDiff;
				}
			}
			return 0;
		}
	}
}