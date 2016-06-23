using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResourcesState
	{
		private double _remainingResourcesInSkillGroup;
		private double _remainingPrimarySkillOverstaff;

		public ShovelResourcesState(double resourcesInSkillGroup, double primarySkillOverstaff)
		{
			_remainingResourcesInSkillGroup = resourcesInSkillGroup;
			_remainingPrimarySkillOverstaff = primarySkillOverstaff;
		}

		public double ResourcesMoved { get; private set; }

		public double RemaingOverstaff()
		{
			return Math.Min(_remainingPrimarySkillOverstaff, _remainingResourcesInSkillGroup);
		}

		public void AddResourcesTo(ISkillStaffPeriod skillStaffPeriod, double value)
		{
			skillStaffPeriod.AddResources(value);
			_remainingResourcesInSkillGroup -= value;
			_remainingPrimarySkillOverstaff -= value;
			ResourcesMoved += value;
		}

		public bool NoMoreResourcesToMove()
		{
			return !_remainingPrimarySkillOverstaff.IsOverstaffed() || _remainingResourcesInSkillGroup.IsZero();
		}
	}
}