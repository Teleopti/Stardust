using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public interface IShovelingCallback
	{
		void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroups, CascadingSkillGroup fromSkillGroup, double resources);
		void ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroups, double resources);
		void BeforeShoveling(IShovelResourceData shovelResourceData);
	}
}