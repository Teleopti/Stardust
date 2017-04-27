using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_EvenRelativeDiff_44091)]
	public class ShovelResourcesStateOld : ShovelResourcesState
	{
		public ShovelResourcesStateOld(IDictionary<ISkill, double> resources, ResourceDistributionForSkillGroupsWithSameIndex resourceDistribution, bool isAnyPrimarySkillOpen) : base(resources, resourceDistribution, isAnyPrimarySkillOpen)
		{
		}

		public override bool ContinueShovel(CascadingSkillGroup skillGroup)
		{
			double resourcesMovedOnSkillGroup;
			if (!_resourcesMovedOnSkillGroup.TryGetValue(skillGroup, out resourcesMovedOnSkillGroup))
			{
				resourcesMovedOnSkillGroup = 0;
			}
			return RemainingOverstaffing > (IsAnyPrimarySkillOpen ? 0.1 : 0.001) &&
				   resourcesMovedOnSkillGroup < MaxToMoveForThisSkillGroup(skillGroup);
		}
	}
}