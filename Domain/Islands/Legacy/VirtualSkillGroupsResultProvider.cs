using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Islands.Legacy
{
	public class VirtualSkillGroupsResultProvider
	{
		public ISkillGroupInfo Fetch()
		{
			var current = VirtualSkillContext.VirtualSkillGroupResult;
			if (current == null)
				throw new NotSupportedException("VirtualSkillGroupResult not in context.");

			return current;
		}
	}
}