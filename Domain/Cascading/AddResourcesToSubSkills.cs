using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills : IAddResourcesToSubSkills
	{
		public void Execute(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval, IShovelingCallback shovelingCallback)
		{
			foreach (var skillGroup in skillGroupsWithSameIndex)
			{
				doForSkillgroup(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback);
			}
		}

		private void doForSkillgroup(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, CascadingSkillGroup skillGroup, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback)
		{
			var maxToMoveForThisSkillGroup = shovelResourcesState.MaxToMoveForThisSkillGroup(skillGroup);
			while (shovelResourcesState.ContinueShovel(skillGroup))
			{
				var stopShovelDueToSubskills = true;

				foreach (var subSkillsWithSameIndex in skillGroup.SubSkillsWithSameIndex)
				{
					var totalUnderstaffingPercent = subSkillsWithSameIndex
						.Select(paralellSkill => shovelResourceData.GetDataForInterval(paralellSkill, interval))
						.Where(x => x.AbsoluteDifference.IsUnderstaffed())
						.Sum(x => -x.RelativeDifference);

					var remainingResourcesToShovel = Math.Min(shovelResourcesState.RemainingOverstaffing, maxToMoveForThisSkillGroup);

					stopShovelDueToSubskills = AddResourcesToSkillsWithSameIndex(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, subSkillsWithSameIndex, totalUnderstaffingPercent, remainingResourcesToShovel, stopShovelDueToSubskills);
				}

				if (stopShovelDueToSubskills)
					return;
			}
		}

		protected virtual bool AddResourcesToSkillsWithSameIndex(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, CascadingSkillGroup skillGroup, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, double totalUnderstaffingPercent, double remainingResourcesToShovel, bool stopShovelDueToSubskills)
		{
			foreach (var skillToMoveTo in subSkillsWithSameIndex)
			{
				var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
				var skillToMoveToAbsoluteDifference = dataForIntervalTo.AbsoluteDifference;
				if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
					continue;

				stopShovelDueToSubskills = false;
				var understaffingPercent = -dataForIntervalTo.RelativeDifference;
				var proportionalResourcesToMove = understaffingPercent / totalUnderstaffingPercent * remainingResourcesToShovel;

				var resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
				shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillGroup, resourceToMove);
				shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove);
			}
			return stopShovelDueToSubskills;
		}
	}
}