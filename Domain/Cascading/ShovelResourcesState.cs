using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResourcesState
	{
		private readonly ResourceDistributionForSkillGroupsWithSameIndex _resourceDistribution;
		private readonly IDictionary<CascadingSkillSet, double> _resourcesMovedOnSkillSet;
		private bool _continueShovelingBasedOnSubSkills;


		public ShovelResourcesState(IDictionary<ISkill, double> resources, 
								ResourceDistributionForSkillGroupsWithSameIndex resourceDistribution,
								bool isAnyPrimarySkillOpen)
		{
			_resourceDistribution = resourceDistribution;
			IsAnyPrimarySkillOpen = isAnyPrimarySkillOpen;
			ResourcesAvailableForPrimarySkill = resources;
			RemainingOverstaffing = ResourcesAvailableForPrimarySkill.Values.Sum();
			TotalOverstaffingAtStart = RemainingOverstaffing;
			_resourcesMovedOnSkillSet = new Dictionary<CascadingSkillSet, double>();
		}

		public bool IsAnyPrimarySkillOpen { get; }
		public IDictionary<ISkill, double> ResourcesAvailableForPrimarySkill { get; }
		public double ResourcesMoved { get; private set; }
		public double RemainingOverstaffing { get; private set; }
		public double TotalOverstaffingAtStart { get; }

		public bool ContinueShovel(CascadingSkillSet skillSet)
		{
			if (!_resourcesMovedOnSkillSet.TryGetValue(skillSet, out double resourcesMovedOnSkillSet))
			{
				resourcesMovedOnSkillSet = 0;
			}
			var ret =  RemainingOverstaffing > (IsAnyPrimarySkillOpen ? 0.1 : 0.001) &&
					resourcesMovedOnSkillSet < MaxToMoveForThisSkillSet(skillSet) &&
					   _continueShovelingBasedOnSubSkills;
			_continueShovelingBasedOnSubSkills = false;
			return ret;
		}

		public double MaxToMoveForThisSkillSet(CascadingSkillSet skillSet)
		{
			return TotalOverstaffingAtStart * _resourceDistribution.For(skillSet);
		}

		public void AddResourcesTo(IShovelResourceDataForInterval shovelResourceDataForInterval, CascadingSkillSet skillSet, double value)
		{
			shovelResourceDataForInterval.AddResources(value);
			RemainingOverstaffing -= value;
			ResourcesMoved += value;
			skillSet.RemainingResources -= value;
			if (_resourcesMovedOnSkillSet.TryGetValue(skillSet, out double currentValue))
			{
				_resourcesMovedOnSkillSet[skillSet] = value + currentValue;
			}
			else
			{
				_resourcesMovedOnSkillSet[skillSet] = value;
			}
			SetContinueShovel();
		}

		public void SetContinueShovel()
		{
			_continueShovelingBasedOnSubSkills = true;
		}
	}
}