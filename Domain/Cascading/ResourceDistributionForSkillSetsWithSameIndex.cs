using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceDistributionForSkillSetsWithSameIndex
	{
		private readonly Lazy<IDictionary<CascadingSkillSet, double>> _distributions;

		public ResourceDistributionForSkillSetsWithSameIndex(IEnumerable<CascadingSkillSet> skillSetsWithSameIndex)
		{
			_distributions = new Lazy<IDictionary<CascadingSkillSet, double>>(() =>
			{
				var tottiRemainingResources = skillSetsWithSameIndex.Sum(x => x.RemainingResources);
				return skillSetsWithSameIndex.ToDictionary(skillSet => skillSet, skillSet => skillSet.RemainingResources / tottiRemainingResources);
			});
		}

		public double For(CascadingSkillSet skillSet)
		{
			return _distributions.Value[skillSet];
		}
	}
}