using System;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroupInfoProvider : ISkillGroupInfoProvider
	{
		public ISkillGroupInfo Fetch()
		{
			var current = SkillGroupContext.SkillGroups;
			if (current == null)
				throw new NotSupportedException("SkillGroups not in context.");

			return new SkillGroups(current); //TODO!
		}
	}
}