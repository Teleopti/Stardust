using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Islands.Legacy
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
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