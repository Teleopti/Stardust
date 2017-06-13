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
			var tottiRelativeDifference = skillGroupsWithSameIndex.SelectMany(skillGroupWithSameIndex => skillGroupWithSameIndex.PrimarySkills)
				.Sum(otherPrimarySkill => shovelResourceData.GetDataForInterval(otherPrimarySkill, interval).AbsoluteDifference);
			foreach (var skillGroup in skillGroupsWithSameIndex)
			{
				var myrelativeDifference = skillGroup.PrimarySkills.Sum(primarySkill => shovelResourceData.GetDataForInterval(primarySkill, interval).AbsoluteDifference);
				var myFactor = myrelativeDifference / tottiRelativeDifference;
				ret[skillGroup] = double.IsNaN(myFactor) ? 1 : myFactor;
			}
			return ret;

			/* Use this block to make ShouldMoveResourceOnlyWithinSkillGroupWhenParallellSubskillsExists green. 
			var tootiRemainingResources = skillGroupsWithSameIndex.Sum(x => x.RemainingResources);
			var ret = new Dictionary<CascadingSkillGroup, double>();
			foreach (var skillGroup in skillGroupsWithSameIndex)
			{
				ret[skillGroup] = skillGroup.RemainingResources / tootiRemainingResources; //do we still need to check for double.IsNaN(myFactor) here? no?
			}
			return ret;
			*/
		}

		public double For(CascadingSkillGroup skillGroup)
		{
			return _distributions.Value[skillGroup];
		}
	}
}