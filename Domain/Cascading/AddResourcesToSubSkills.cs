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
					var shovelResourcesDataForIntervalForUnderstaffedSkills = subSkillsWithSameIndex
						.Select(paralellSkill => shovelResourceData.GetDataForInterval(paralellSkill, interval))
						.Where(x => x.AbsoluteDifference.IsUnderstaffed());
					var totalFStaff = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => x.FStaff);
					var totalResurces = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => x.CalculatedResource);
					var totalUnderstaffingPercentIsPositive = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => -x.RelativeDifference) > 0;

					var remainingResourcesToShovel = Math.Min(shovelResourcesState.RemainingOverstaffing, maxToMoveForThisSkillGroup);

					foreach (var skillToMoveTo in subSkillsWithSameIndex)
					{
						double? resourceToMove;
						var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
						if (shovelResourcesState.IsAnyPrimarySkillOpen)
						{
							resourceToMove = addResourcesToSkillsWithSameIndex(dataForIntervalTo, remainingResourcesToShovel, maxToMoveForThisSkillGroup, totalFStaff, totalResurces);
						}
						else
						{
							stopShovelDueToSubskills = false;
							resourceToMove = totalUnderstaffingPercentIsPositive ?
								addResourcesToSkillsWithSameIndex(dataForIntervalTo, remainingResourcesToShovel, maxToMoveForThisSkillGroup, totalFStaff, totalResurces) :
								Math.Min(remainingResourcesToShovel, remainingResourcesToShovel / subSkillsWithSameIndex.Count());
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

		private static double? addResourcesToSkillsWithSameIndex(IShovelResourceDataForInterval shovelResourceDataForInterval, double remainingResourcesToShovel, 
			double maxToMoveForThisSkillGroup, double totalFStaff, double totalResurces)
		{
			var skillToMoveToAbsoluteDifference = shovelResourceDataForInterval.AbsoluteDifference;
			if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
				return null;

			var proportionalResourcesToMove = (maxToMoveForThisSkillGroup -
											shovelResourceDataForInterval.CalculatedResource * (totalFStaff / shovelResourceDataForInterval.FStaff - 1) +
											totalResurces -
											shovelResourceDataForInterval.CalculatedResource) / (totalFStaff / shovelResourceDataForInterval.FStaff);

			return Math.Min(Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove), remainingResourcesToShovel);
		}
	}
}