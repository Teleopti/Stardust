using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceDistributionForSkillGroupsWithSameIndex
	{
		private readonly Lazy<IDictionary<CascadingSkillGroup, double>> _distributions;

		public ResourceDistributionForSkillGroupsWithSameIndex(IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
		{
			_distributions = new Lazy<IDictionary<CascadingSkillGroup, double>>(() => init(shovelResourceData, skillGroupsWithSameIndex, interval));
		}

		private static IDictionary<CascadingSkillGroup, double> init(IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
		{
			var ret = new Dictionary<CascadingSkillGroup, double>();
			var tottiRelativeDifference = 0d;
			foreach (var skillGroupWithSameIndex in skillGroupsWithSameIndex)
			{
				foreach (var otherPrimarySkill in skillGroupWithSameIndex.PrimarySkills)
				{
					var relDiffInOtherPrimarySkill = shovelResourceData.GetDataForInterval(otherPrimarySkill, interval).AbsoluteDifference;
					//TODO: suspicious code here - bug? Only include positive values here?
					if (!double.IsNaN(relDiffInOtherPrimarySkill)) 
					{
						tottiRelativeDifference += relDiffInOtherPrimarySkill;
					}
				}
			}
			foreach (var skillGroup in skillGroupsWithSameIndex)
			{
				var myrelativeDifference = skillGroup.PrimarySkills.Sum(primarySkill => shovelResourceData.GetDataForInterval(primarySkill, interval).AbsoluteDifference);
				var myFactor = myrelativeDifference / tottiRelativeDifference;
				ret[skillGroup] = double.IsNaN(myFactor) ? 1 : myFactor;
			}
			return ret;
		}

		public double For(CascadingSkillGroup skillGroup)
		{
			return _distributions.Value[skillGroup];
		}
	}
}