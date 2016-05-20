using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkillGroupSorter : IComparer<CascadingSkillGroup>
	{
		public int Compare(CascadingSkillGroup first, CascadingSkillGroup second)
		{
			var primaryDiff = second.PrimarySkill.CascadingIndex.Value - first.PrimarySkill.CascadingIndex.Value;
			if (primaryDiff != 0)
			{
				return primaryDiff;
			}

			var firstSkills = first.CascadingSkills.ToArray();
			var secondSkills = second.CascadingSkills.ToArray();

			for (var i = 0; i < firstSkills.Length; i++)
			{
				//TODO: lägg på test för detta case!

				//if (i > secondSkills.Length - 1)
				//{
				//	return ??
				//}

				var subSkillDiff = firstSkills[i].CascadingIndex.Value - secondSkills[i].CascadingIndex.Value;
				if (subSkillDiff != 0)
				{
					return subSkillDiff;
				}
			}
			return 0;
		}
	}
}