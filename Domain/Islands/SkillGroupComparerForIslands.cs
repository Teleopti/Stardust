using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroupComparerForIslands : IEqualityComparer<SkillGroup>
	{
		public bool Equals(SkillGroup x, SkillGroup y)
		{
			return x.Skills.Intersect(y.Skills).Any();
		}

		public int GetHashCode(SkillGroup obj)
		{
			return 1;
		}
	}
}