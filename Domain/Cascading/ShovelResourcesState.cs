using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResourcesState
	{
		private const double minPrimaryOverstaffToContinue = 0.1;
		private readonly ResourceDistributionForSkillGroupsWithSameIndex _resourceDistribution;
		private readonly IDictionary<CascadingSkillGroup, double> _resourcesMovedOnSkillGroup;

		public ShovelResourcesState(IDictionary<ISkill, double> resources, ResourceDistributionForSkillGroupsWithSameIndex resourceDistribution)
		{
			_resourceDistribution = resourceDistribution;
			ResourcesAvailableForPrimarySkill = resources;
			RemainingOverstaffing = ResourcesAvailableForPrimarySkill.Values.Sum();
			TotalOverstaffingAtStart = RemainingOverstaffing;
			_resourcesMovedOnSkillGroup = new Dictionary<CascadingSkillGroup, double>();
		}

		public IDictionary<ISkill, double> ResourcesAvailableForPrimarySkill { get; }
		public double ResourcesMoved { get; private set; }
		public double RemainingOverstaffing { get; private set; }
		public double TotalOverstaffingAtStart { get; }

		public bool ContinueShovel(CascadingSkillGroup skillGroup)
		{
			double resourcesMovedOnSkillGroup;
			if (!_resourcesMovedOnSkillGroup.TryGetValue(skillGroup, out resourcesMovedOnSkillGroup))
			{
				resourcesMovedOnSkillGroup = 0;
			}
			return RemainingOverstaffing > minPrimaryOverstaffToContinue && 
				resourcesMovedOnSkillGroup < MaxToMoveForThisSkillGroup(skillGroup);
		}

		public double MaxToMoveForThisSkillGroup(CascadingSkillGroup skillgroup)
		{
			return TotalOverstaffingAtStart * _resourceDistribution.For(skillgroup);
		}

		public void AddResourcesTo(IShovelResourceDataForInterval shovelResourceDataForInterval, CascadingSkillGroup skillGroup, double value)
		{
			shovelResourceDataForInterval.AddResources(value);
			RemainingOverstaffing -= value;
			ResourcesMoved += value;
			skillGroup.RemainingResources -= value;
			double currentValue;
			if (_resourcesMovedOnSkillGroup.TryGetValue(skillGroup, out currentValue))
			{
				_resourcesMovedOnSkillGroup[skillGroup] = value + currentValue;
			}
			else
			{
				_resourcesMovedOnSkillGroup[skillGroup] = value;
			}
		}
	}
}