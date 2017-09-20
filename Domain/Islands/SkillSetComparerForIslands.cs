using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillSetComparerForIslands : IEqualityComparer<SkillSet>
	{
		public bool Equals(SkillSet x, SkillSet y)
		{
			return x.Skills.Intersect(y.Skills).Any();
		}

		public int GetHashCode(SkillSet obj)
		{
			return 1;
		}
	}
}