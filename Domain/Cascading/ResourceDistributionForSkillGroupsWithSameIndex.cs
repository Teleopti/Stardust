using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceDistributionForSkillGroupsWithSameIndex
	{
		private readonly Lazy<IDictionary<CascadingSkillSet, double>> _distributions;

		public ResourceDistributionForSkillGroupsWithSameIndex(IEnumerable<CascadingSkillSet> skillGroupsWithSameIndex)
		{
			_distributions = new Lazy<IDictionary<CascadingSkillSet, double>>(() =>
			{
				var tottiRemainingResources = skillGroupsWithSameIndex.Sum(x => x.RemainingResources);
				return skillGroupsWithSameIndex.ToDictionary(skillGroup => skillGroup, skillGroup => skillGroup.RemainingResources / tottiRemainingResources);
			});
		}

		public double For(CascadingSkillSet skillSet)
		{
			return _distributions.Value[skillSet];
		}
	}
}