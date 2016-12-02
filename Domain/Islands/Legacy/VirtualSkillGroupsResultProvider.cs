using System;

namespace Teleopti.Ccc.Domain.Islands.Legacy
{
	public class VirtualSkillGroupsResultProvider : ISkillGroupInfoProvider
	{
		public ISkillGroupInfo Fetch()
		{
			var current = VirtualSkillContext.VirtualSkillGroupResult;
			if (current == null)
				throw new NotSupportedException("SkillGroups not in context.");

			return current;
		}
	}
}