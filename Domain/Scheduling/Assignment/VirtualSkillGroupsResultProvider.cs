using System;
using Teleopti.Ccc.Domain.DayOffPlanning;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class VirtualSkillGroupsResultProvider
	{
		public VirtualSkillGroupsCreatorResult Fetch()
		{
			if (VirtualSkillContext.VirtualSkillGroupResult == null)
				throw new NotSupportedException("VirtualSkillGroupResult not in context.");
			return VirtualSkillContext.VirtualSkillGroupResult;
		}
	}
}