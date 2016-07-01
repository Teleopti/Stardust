using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResourcesState
	{
		private double _remainingResourcesInSkillGroup;
		private readonly AvailableResourcesToMoveOnSkill _remainingPrimarySkillOverstaff;

		public ShovelResourcesState(double resourcesInSkillGroup, AvailableResourcesToMoveOnSkill primarySkillOverstaff)
		{
			_remainingResourcesInSkillGroup = resourcesInSkillGroup;
			_remainingPrimarySkillOverstaff = primarySkillOverstaff;
		}

		public double ResourcesMoved { get; private set; }

		public double RemaingOverstaff()
		{
			return Math.Min(_remainingPrimarySkillOverstaff.RemainingTotalResources, _remainingResourcesInSkillGroup);
		}

		public IDictionary<ISkill, double> ResourcesAvailableForPrimarySkill()
		{
			return _remainingPrimarySkillOverstaff.ResourcesAvailableForPrimarySkill();
		}

		public double TotalResourcesAtStart
		{
			get { return _remainingPrimarySkillOverstaff.TotalResourcesAtStart; }
		}

		public void AddResourcesTo(ISkillStaffPeriod skillStaffPeriod, double value)
		{
			skillStaffPeriod.AddResources(value);
			_remainingResourcesInSkillGroup -= value;
			_remainingPrimarySkillOverstaff.RemainingTotalResources -= value;
			ResourcesMoved += value;
		}

		public bool NoMoreResourcesToMove()
		{
			return !_remainingPrimarySkillOverstaff.RemainingTotalResources.IsOverstaffed() || _remainingResourcesInSkillGroup.IsZero();
		}
	}
}