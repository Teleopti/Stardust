using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills
	{
		public void Execute(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillSet> skillGroupsWithSameIndex, DateTimePeriod interval, IShovelingCallback shovelingCallback)
		{
			foreach (var skillGroup in skillGroupsWithSameIndex)
			{
				var maxToMoveForThisSkillGroup = shovelResourcesState.MaxToMoveForThisSkillGroup(skillGroup);
				shovelResourcesState.SetContinueShovel();
				while (shovelResourcesState.ContinueShovel(skillGroup))
				{
					foreach (var subSkillsWithSameIndex in skillGroup.SubSkillsWithSameIndex)
					{
						var affectedSkills = findSkillsToShovelTo(shovelResourcesState, shovelResourceData, skillGroupsWithSameIndex, interval, shovelingCallback, subSkillsWithSameIndex, skillGroup, maxToMoveForThisSkillGroup);
						if (affectedSkills.Any())
						{
							shovelToSubSkills(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, affectedSkills, maxToMoveForThisSkillGroup, doActualShoveling);
						}
						else
						{
							if (!shovelResourcesState.IsAnyPrimarySkillOpen)
							{
								shovelResourcesFromClosedPrimarySkill(shovelResourcesState, shovelResourceData, skillGroupsWithSameIndex, interval, shovelingCallback, subSkillsWithSameIndex, skillGroup, maxToMoveForThisSkillGroup);
							}
						}
					}
				}
			}
		}

		private static void shovelResourcesFromClosedPrimarySkill(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillSet> skillGroupsWithSameIndex, DateTimePeriod interval,
			IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, CascadingSkillSet skillSet, double maxToMoveForThisSkillGroup)
		{
			var totalResourcesToMoveFromClosedPrimarySkill = Math.Min(maxToMoveForThisSkillGroup, shovelResourcesState.RemainingOverstaffing);
			foreach (var skillToMoveTo in subSkillsWithSameIndex)
			{
				var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
				var resourceToMove = totalResourcesToMoveFromClosedPrimarySkill / subSkillsWithSameIndex.Count();
				doActualShoveling(shovelResourcesState, skillSet, interval, skillGroupsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
			}
		}

		private static IEnumerable<ISkill> findSkillsToShovelTo(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillSet> skillGroupsWithSameIndex, DateTimePeriod interval,
			IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, CascadingSkillSet skillSet,
			double maxToMoveForThisSkillGroup)
		{
			ICollection<ISkill> affectedSkills = subSkillsWithSameIndex.ToList();
			while (true)
			{
				var countBefore = affectedSkills.Count;
				var newAffectedSkills = new List<ISkill>();
				shovelToSubSkills(shovelResourcesState, shovelResourceData, skillSet, interval, skillGroupsWithSameIndex,
					shovelingCallback, affectedSkills, maxToMoveForThisSkillGroup, (arg1, arg2, arg3, arg4, arg5, arg6, arg7, skill) => newAffectedSkills.Add(skill));
				affectedSkills = newAffectedSkills;
				if (newAffectedSkills.Count == countBefore)
					break;
			}
			return affectedSkills;
		}

		private static void shovelToSubSkills(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, CascadingSkillSet skillSet, DateTimePeriod interval,
			IEnumerable<CascadingSkillSet> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback,
			IEnumerable<ISkill> subSkillsWithSameIndex, double maxToMoveForThisSkillGroup, 
			Action<ShovelResourcesState, CascadingSkillSet, DateTimePeriod, IEnumerable<CascadingSkillSet>, IShovelingCallback, IShovelResourceDataForInterval, double, ISkill> shovelAction)
		{
			var shovelResourcesDataForIntervalForUnderstaffedSkills = subSkillsWithSameIndex
				.Select(paralellSkill => shovelResourceData.GetDataForInterval(paralellSkill, interval))
				.Where(x => x.AbsoluteDifference.IsUnderstaffed());
			var totalFStaff = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => x.FStaff);
			var totalResurces = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => x.CalculatedResource);

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

				var resourceToMove = Math.Min(Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove), shovelResourcesState.RemainingOverstaffing);
				shovelAction(shovelResourcesState, skillSet, interval, skillGroupsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
			}
		}

		private static void doActualShoveling(ShovelResourcesState shovelResourcesState, CascadingSkillSet skillSet,
			DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback,
			IShovelResourceDataForInterval dataForIntervalTo, double resourceToMove, ISkill skillToMoveTo)
		{
			shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillSet, resourceToMove);
			shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillSet, resourceToMove);
		}
	}
}