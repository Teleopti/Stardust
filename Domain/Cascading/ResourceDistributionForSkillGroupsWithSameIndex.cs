using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceDistributionForSkillGroupsWithSameIndex
	{
		private readonly Lazy<IDictionary<CascadingSkillGroup, double>> _distributions;

		[RemoveMeWithToggle("remove params shovelResourceData and interval", Toggles.ResourcePlanner_RespectSkillGroupShoveling_44156)]
		public ResourceDistributionForSkillGroupsWithSameIndex(IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
		{
			_distributions = new Lazy<IDictionary<CascadingSkillGroup, double>>(() => init(shovelResourceData, skillGroupsWithSameIndex, interval));
		}

		[RemoveMeWithToggle("make private, remove params shovelResourceData and interval", Toggles.ResourcePlanner_RespectSkillGroupShoveling_44156)]
		protected virtual IDictionary<CascadingSkillGroup, double> init(IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
		{
			var tottiRemainingResources = skillGroupsWithSameIndex.Sum(x => x.RemainingResources);
			return skillGroupsWithSameIndex.ToDictionary(skillGroup => skillGroup, skillGroup => skillGroup.RemainingResources / tottiRemainingResources);
		}

		public double For(CascadingSkillGroup skillGroup)
		{
			return _distributions.Value[skillGroup];
		}
	}


	[RemoveMeWithToggle(Toggles.ResourcePlanner_RespectSkillGroupShoveling_44156)]
	public class ResourceDistributionForSkillGroupsWithSameIndexOLD : ResourceDistributionForSkillGroupsWithSameIndex
	{
		public ResourceDistributionForSkillGroupsWithSameIndexOLD(IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval) : base(shovelResourceData, skillGroupsWithSameIndex, interval)
		{
		}

		protected override IDictionary<CascadingSkillGroup, double> init(IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval)
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
		}
	}
}