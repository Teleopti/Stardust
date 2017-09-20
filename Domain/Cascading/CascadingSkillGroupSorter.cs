using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroupSorter : IComparer<CascadingSkillSet>
	{
		public int Compare(CascadingSkillSet first, CascadingSkillSet second)
		{
			var primaryDiff = second.PrimarySkillsCascadingIndex - first.PrimarySkillsCascadingIndex;
			if (primaryDiff != 0)
			{
				return primaryDiff;
			}

			//TODO: is this really needed?
			var primaryCountDiff = second.PrimarySkills.Count() - first.PrimarySkills.Count();
			if (primaryCountDiff != 0)
			{
				return primaryCountDiff;
			}
			//

			var firstSubSkill = first.SubSkillsWithSameIndex.ToArray();
			var secondSubSkill = second.SubSkillsWithSameIndex.ToArray();

			if (firstSubSkill.Length == 0 && secondSubSkill.Length > 0)
				return 1;

			for (var i = 0; i < firstSubSkill.Length; i++)
			{
				if (i > secondSubSkill.Length - 1)
				{
					return -1;
				}

				var subSkillDiff = firstSubSkill[i].CascadingIndex - secondSubSkill[i].CascadingIndex;
				if (subSkillDiff != 0)
				{
					return subSkillDiff;
				}

				var skillCountDiff = secondSubSkill[i].Count() - firstSubSkill[i].Count();
				if (skillCountDiff != 0)
				{
					return skillCountDiff;
				}
			}

			if (firstSubSkill.Length < secondSubSkill.Length)
				return 1;

			return 0;
		}
	}
}