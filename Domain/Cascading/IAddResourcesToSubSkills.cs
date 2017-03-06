using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public interface IAddResourcesToSubSkills
	{
		void Execute(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval, IShovelingCallback shovelingCallback);
	}
}