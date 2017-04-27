using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_EvenRelativeDiff_44091)]
	public class PrimarySkillOverstaffOld : PrimarySkillOverstaff
	{
		protected override ShovelResourcesState CreateShovelResourcesState(IShovelResourceData shovelResourceData,
			IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval, Dictionary<ISkill, double> dic,
			bool primarySkillsExistsButTheyAreAllClosed)
		{
			return new ShovelResourcesStateOld(dic,
				new ResourceDistributionForSkillGroupsWithSameIndex(shovelResourceData, skillGroupsWithSameIndex, interval),
				!primarySkillsExistsButTheyAreAllClosed);
		}
	}
}