using System.Collections.Generic;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class NoShovelingCallback : IShovelingCallback
	{
		public void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroups, CascadingSkillGroup fromSkillGroup, double resources)
		{
		}

		public void ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroups, double resources)
		{
		}

		public void BeforeShoveling(IShovelResourceData shovelResourceData)
		{
		}
	}
}