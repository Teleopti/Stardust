using System.Collections.Generic;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class NoShovelingCallback : IShovelingCallback
	{
		public void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, CascadingSkillSet fromSkillSet, double resources)
		{
		}

		public void ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, double resources)
		{
		}

		public void BeforeShoveling(IShovelResourceData shovelResourceData)
		{
		}
	}
}