using System;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Islands;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class VirtualSkillGroupsResultProvider
	{
		public VirtualSkillGroupsCreatorResult Fetch()
		{
			var current = VirtualSkillContext.VirtualSkillGroupResult;
			if (current == null)
				throw new NotSupportedException("VirtualSkillGroupResult not in context.");

			return current;
		}
	}
}