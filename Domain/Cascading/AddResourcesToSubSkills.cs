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
				var maxToMoveForThisSkillGroup = shovelResourcesState.MaxToMoveForThisSkillGroup(skillGroup);
				shovelResourcesState.SetContinueShovel();
				while (shovelResourcesState.ContinueShovel(skillGroup))
				{
					foreach (var subSkillsWithSameIndex in skillGroup.SubSkillsWithSameIndex)
					{
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
							mightShovelToSubSkills(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, affectedSkills, maxToMoveForThisSkillGroup, true);
						}
						else
						{
							if (!shovelResourcesState.IsAnyPrimarySkillOpen)
							{
								var remainingResourcesToShovel = Math.Min(shovelResourcesState.RemainingOverstaffing, maxToMoveForThisSkillGroup);
								foreach (var skillToMoveTo in subSkillsWithSameIndex)
								{
									var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
									var resourceToMove = Math.Min(remainingResourcesToShovel, remainingResourcesToShovel / subSkillsWithSameIndex.Count());
									doActualShoveling(shovelResourcesState, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
								}
							}
						}
					}
				}
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
				var skillToMoveToAbsoluteDifference = dataForIntervalTo.AbsoluteDifference;
				if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
					continue;

				var proportionalResourcesToMove = (maxToMoveForThisSkillGroup -
												   dataForIntervalTo.CalculatedResource * (totalFStaff / dataForIntervalTo.FStaff - 1) +
												   totalResurces -
												   dataForIntervalTo.CalculatedResource) / (totalFStaff / dataForIntervalTo.FStaff);
				if (proportionalResourcesToMove < 0)
					continue;

				var resourceToMove = Math.Min(Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove), remainingResourcesToShovel);
				if (doActualMove)
				{
					doActualShoveling(shovelResourcesState, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
				}
				affectedSkills.Add(skillToMoveTo);
			}
			return affectedSkills;
		}

		private static void doActualShoveling(ShovelResourcesState shovelResourcesState, CascadingSkillGroup skillGroup,
			DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback,
			IShovelResourceDataForInterval dataForIntervalTo, double resourceToMove, ISkill skillToMoveTo)
		{
			shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillGroup, resourceToMove);
			shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove);
		}
	}
}