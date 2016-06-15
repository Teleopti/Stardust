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
			var firstSkills = first.CascadingSkillGroupItems.ToArray();
			var secondSkills = second.CascadingSkillGroupItems.ToArray();
			for (var i = 0; i < firstSkills.Length; i++)
			{
				if (i > secondSkills.Length - 1)
				{
					return 0;
				}
				var subSkillDiff = firstSkills[i].CascadingIndex - secondSkills[i].CascadingIndex;
				if (subSkillDiff != 0)
				{
					return subSkillDiff;
				}
			}
			return 0;
		}
	}
}