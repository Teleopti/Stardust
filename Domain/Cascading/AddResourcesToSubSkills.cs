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
						double? resourceToMove;
						var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
						if (shovelResourcesState.IsAnyPrimarySkillOpen)
						{
							resourceToMove = addResourcesToSkillsWithSameIndexWhenPrimarySkillIsOpened(dataForIntervalTo, totalUnderstaffingPercent, remainingResourcesToShovel);
						}
						else
						{
							stopShovelDueToSubskills = false;
							resourceToMove = addResourcesToSkillsWithSameIndexWhenPrimarySkillIsClosed(dataForIntervalTo, subSkillsWithSameIndex, totalUnderstaffingPercent, remainingResourcesToShovel);
						}
						if (resourceToMove.HasValue)
						{
							stopShovelDueToSubskills = false;
							shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillGroup, resourceToMove.Value);
							shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove.Value);
						}
					}
				}

				if (stopShovelDueToSubskills)
					return;
			}
		}

		private static double? addResourcesToSkillsWithSameIndexWhenPrimarySkillIsOpened(IShovelResourceDataForInterval shovelResourceDataForInterval,
			double totalUnderstaffingPercent, double remainingResourcesToShovel)
		{
			var skillToMoveToAbsoluteDifference = shovelResourceDataForInterval.AbsoluteDifference;
			if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
				return null;

			var understaffingPercent = -shovelResourceDataForInterval.RelativeDifference;
			var proportionalResourcesToMove = understaffingPercent / totalUnderstaffingPercent * remainingResourcesToShovel;

			return Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
		}

		private static double? addResourcesToSkillsWithSameIndexWhenPrimarySkillIsClosed(IShovelResourceDataForInterval shovelResourceDataForInterval,
			SubSkillsWithSameIndex subSkillsWithSameIndex, double totalUnderstaffingPercent, double remainingResourcesToShovel)
		{
			if (totalUnderstaffingPercent > 0)
			{
				var skillToMoveToAbsoluteDifference = shovelResourceDataForInterval.AbsoluteDifference;
				if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
					return null;
				var understaffingPercent = -shovelResourceDataForInterval.RelativeDifference;
				var proportionalResourcesToMove = understaffingPercent / totalUnderstaffingPercent * remainingResourcesToShovel;
				return Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
			}
			else
			{
				var proportionalResourcesToMove = remainingResourcesToShovel / subSkillsWithSameIndex.Count();
				return Math.Min(remainingResourcesToShovel, proportionalResourcesToMove);
			}
		}
	}
}