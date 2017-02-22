using System;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroupInfoProvider
	{
		public SkillGroups Fetch()
		{
			var current = SkillGroupContext.SkillGroups;
			if (current == null)
				throw new NotSupportedException("SkillGroups not in context.");

			return current;
		}
	}
}