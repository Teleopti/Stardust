using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SortSkillGroupBySize : IComparer<SkillGroup>
	{
		public int Compare(SkillGroup x, SkillGroup y)
		{
			return y.Agents.Count() - x.Agents.Count();
		}
	}
}