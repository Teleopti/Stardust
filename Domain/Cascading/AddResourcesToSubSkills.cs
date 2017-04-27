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
					if (!shovelResourcesState.IsAnyPrimarySkillOpen)
					{
						stopShovelDueToSubskills = false;

						var understaffedSubSkillsExists =
							subSkillsWithSameIndex
								.Select(paralellSkill => shovelResourceData.GetDataForInterval(paralellSkill, interval))
								.Any(x => x.AbsoluteDifference.IsUnderstaffed());
						if (!understaffedSubSkillsExists)
						{
							var remainingResourcesToShovel = Math.Min(shovelResourcesState.RemainingOverstaffing, maxToMoveForThisSkillGroup);
							foreach (var skillToMoveTo in subSkillsWithSameIndex)
							{
								var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
								var resourceToMove = Math.Min(remainingResourcesToShovel, remainingResourcesToShovel / subSkillsWithSameIndex.Count());

								shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillGroup, resourceToMove);
								shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove);
		
							}
							continue;
						}
					}
					IEnumerable<ISkill> affectedSkills = subSkillsWithSameIndex;
					while (true)
					{
						var countBefore = affectedSkills.Count();
						affectedSkills = mightShovelToSubSkills(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, affectedSkills, maxToMoveForThisSkillGroup, false);
						if (affectedSkills.Count() == countBefore)
							break;
					}
					if (affectedSkills.Any())
					{
						stopShovelDueToSubskills = false;
						mightShovelToSubSkills(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, affectedSkills, maxToMoveForThisSkillGroup, true);
					}
				}

				if (stopShovelDueToSubskills)
					return;
			}
		}

		private static IEnumerable<ISkill> mightShovelToSubSkills(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, CascadingSkillGroup skillGroup, DateTimePeriod interval,
			IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback,
			IEnumerable<ISkill> subSkillsWithSameIndex, double maxToMoveForThisSkillGroup, bool doActualMove)
		{
			var affectedSkills = new List<ISkill>();
			var shovelResourcesDataForIntervalForUnderstaffedSkills = subSkillsWithSameIndex
				.Select(paralellSkill => shovelResourceData.GetDataForInterval(paralellSkill, interval))
				.Where(x => x.AbsoluteDifference.IsUnderstaffed());
			var totalFStaff = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => x.FStaff);
			var totalResurces = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => x.CalculatedResource);

			var remainingResourcesToShovel = Math.Min(shovelResourcesState.RemainingOverstaffing, maxToMoveForThisSkillGroup);

			foreach (var skillToMoveTo in subSkillsWithSameIndex)
			{
				var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
				var resourceToMove = addResourcesToSkillsWithSameIndex(dataForIntervalTo, remainingResourcesToShovel,
					maxToMoveForThisSkillGroup, totalFStaff, totalResurces);

				if (resourceToMove.HasValue)
				{
					if (doActualMove)
					{
						shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillGroup, resourceToMove.Value);
						shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove.Value);
					}
					affectedSkills.Add(skillToMoveTo);
				}
			}
			return affectedSkills;
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

			if (proportionalResourcesToMove < 0)
				return null;

			return Math.Min(Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove), remainingResourcesToShovel);
		}
	}
}