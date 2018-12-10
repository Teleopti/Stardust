using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public interface IShovelingCallback
	{
		void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, CascadingSkillSet fromSkillSet, double resources);
		void ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, double resources);
		void BeforeShoveling(IShovelResourceData shovelResourceData);
	}
}