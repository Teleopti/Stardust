using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResourcesState
	{
		private readonly CascadingSkillGroup _skillGroup;

		public ShovelResourcesState(IDictionary<ISkill, double> resources, CascadingSkillGroup skillGroup)
		{
			_skillGroup = skillGroup;
			ResourcesAvailableForPrimarySkill = resources;
			RemainingOverstaffing = ResourcesAvailableForPrimarySkill.Values.Sum();
			TotalOverstaffingAtStart = RemainingOverstaffing;
			SkillgroupResourcesAtStart = _skillGroup.RemainingResources;
		}

		public IDictionary<ISkill, double> ResourcesAvailableForPrimarySkill { get; }
		public double ResourcesMoved { get; private set; }
		public double RemainingOverstaffing { get; private set; }
		public double TotalOverstaffingAtStart { get; private set; }
		public double SkillgroupResourcesAtStart { get; }

		public void AddResourcesTo(ISkillStaffPeriod skillStaffPeriod, double value)
		{
			skillStaffPeriod.AddResources(value);
			RemainingOverstaffing -= value;
			ResourcesMoved += value;
			_skillGroup.RemainingResources -= value;
		}
	}
}