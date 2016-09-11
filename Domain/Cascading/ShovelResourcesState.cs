using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResourcesState
	{
		private readonly IDictionary<CascadingSkillGroup, double> _maxfactorPerSkillGroup;

		public ShovelResourcesState(IDictionary<ISkill, double> resources, IDictionary<CascadingSkillGroup, double> maxfactorPerSkillGroup)
		{
			_maxfactorPerSkillGroup = maxfactorPerSkillGroup;
			ResourcesAvailableForPrimarySkill = resources;
			RemainingOverstaffing = ResourcesAvailableForPrimarySkill.Values.Sum();
			TotalOverstaffingAtStart = RemainingOverstaffing;
		}

		public IDictionary<ISkill, double> ResourcesAvailableForPrimarySkill { get; }
		public double ResourcesMoved { get; private set; }
		public double RemainingOverstaffing { get; private set; }
		public double TotalOverstaffingAtStart { get; }

		public double MaxToMoveForThisSkillGroup(CascadingSkillGroup skillgroup)
		{
			return TotalOverstaffingAtStart*_maxfactorPerSkillGroup[skillgroup];
		}

		public void AddResourcesTo(ISkillStaffPeriod skillStaffPeriod, double value)
		{
			skillStaffPeriod.AddResources(value);
			RemainingOverstaffing -= value;
			ResourcesMoved += value;
		}
	}
}