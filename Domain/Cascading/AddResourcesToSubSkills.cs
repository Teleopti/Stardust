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

		private static void doForSkillgroup(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, CascadingSkillGroup skillGroup, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback)
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

					foreach (var skillToMoveTo in subSkillsWithSameIndex)
					{
						var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
						if (shovelResourcesState.IsAnyPrimarySkillOpen)
						{
							stopShovelDueToSubskills = addResourcesToSkillsWithSameIndexWhenPrimarySkillIsOpened(shovelResourcesState, dataForIntervalTo,
								skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, skillToMoveTo,
								totalUnderstaffingPercent, remainingResourcesToShovel, stopShovelDueToSubskills);
						}
						else
						{
							stopShovelDueToSubskills = false;
							addResourcesToSkillsWithSameIndexWhenPrimarySkillIsClosed(shovelResourcesState, dataForIntervalTo, skillGroup,
								interval, skillGroupsWithSameIndex, shovelingCallback, subSkillsWithSameIndex, skillToMoveTo, totalUnderstaffingPercent,
								remainingResourcesToShovel);

						}
					}
				}

				if (stopShovelDueToSubskills)
					return;
			}
		}

		private static bool addResourcesToSkillsWithSameIndexWhenPrimarySkillIsOpened(ShovelResourcesState shovelResourcesState, IShovelResourceDataForInterval shovelResourceDataForInterval,
			CascadingSkillGroup skillGroup, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex,
			IShovelingCallback shovelingCallback, ISkill skillToMoveTo, double totalUnderstaffingPercent,
			double remainingResourcesToShovel, bool stopShovelDueToSubskills)
		{
			var skillToMoveToAbsoluteDifference = shovelResourceDataForInterval.AbsoluteDifference;
			if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
				return stopShovelDueToSubskills;

			var understaffingPercent = -shovelResourceDataForInterval.RelativeDifference;
			var proportionalResourcesToMove = understaffingPercent / totalUnderstaffingPercent * remainingResourcesToShovel;

			var resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
			shovelResourcesState.AddResourcesTo(shovelResourceDataForInterval, skillGroup, resourceToMove);
			shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove);

			return false;
		}

		private static void addResourcesToSkillsWithSameIndexWhenPrimarySkillIsClosed(ShovelResourcesState shovelResourcesState, IShovelResourceDataForInterval shovelResourceDataForInterval,
			CascadingSkillGroup skillGroup, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex,
			IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, ISkill skillToMoveTo, double totalUnderstaffingPercent,
			double remainingResourcesToShovel)
		{
			double resourceToMove;

			if (totalUnderstaffingPercent > 0)
			{
				var skillToMoveToAbsoluteDifference = shovelResourceDataForInterval.AbsoluteDifference;
				if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
					return;
				var understaffingPercent = -shovelResourceDataForInterval.RelativeDifference;
				var proportionalResourcesToMove = understaffingPercent / totalUnderstaffingPercent * remainingResourcesToShovel;
				resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
			}
			else
			{
				var proportionalResourcesToMove = remainingResourcesToShovel / subSkillsWithSameIndex.Count();
				resourceToMove = Math.Min(remainingResourcesToShovel, proportionalResourcesToMove);
			}

			shovelResourcesState.AddResourcesTo(shovelResourceDataForInterval, skillGroup, resourceToMove);
			shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove);
		}
	}
}