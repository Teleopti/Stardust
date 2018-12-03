using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkills
	{
		public void Execute(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillSet> skillSetsWithSameIndex, DateTimePeriod interval, IShovelingCallback shovelingCallback)
		{
			foreach (var skillSet in skillSetsWithSameIndex)
			{
				var maxToMoveForThisSkillSet = shovelResourcesState.MaxToMoveForThisSkillSet(skillSet);
				shovelResourcesState.SetContinueShovel();
				while (shovelResourcesState.ContinueShovel(skillSet))
				{
					foreach (var subSkillsWithSameIndex in skillSet.SubSkillsWithSameIndex)
					{
						var affectedSkills = findSkillsToShovelTo(shovelResourcesState, shovelResourceData, skillSetsWithSameIndex, interval, shovelingCallback, subSkillsWithSameIndex, skillSet, maxToMoveForThisSkillSet);
						if (affectedSkills.Any())
						{
							shovelToSubSkills(shovelResourcesState, shovelResourceData, skillSet, interval, skillSetsWithSameIndex, shovelingCallback, affectedSkills, maxToMoveForThisSkillSet, doActualShoveling);
						}
						else
						{
							if (!shovelResourcesState.IsAnyPrimarySkillOpen)
							{
								shovelResourcesFromClosedPrimarySkill(shovelResourcesState, shovelResourceData, skillSetsWithSameIndex, interval, shovelingCallback, subSkillsWithSameIndex, skillSet, maxToMoveForThisSkillSet);
							}
						}
					}
				}
			}
		}

		private static void shovelResourcesFromClosedPrimarySkill(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillSet> skillSetsWithSameIndex, DateTimePeriod interval,
			IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, CascadingSkillSet skillSet, double maxToMoveForThisSkillSet)
		{
			var totalResourcesToMoveFromClosedPrimarySkill = Math.Min(maxToMoveForThisSkillSet, shovelResourcesState.RemainingOverstaffing);
			foreach (var skillToMoveTo in subSkillsWithSameIndex)
			{
				var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
				var resourceToMove = totalResourcesToMoveFromClosedPrimarySkill / subSkillsWithSameIndex.Count();
				doActualShoveling(shovelResourcesState, skillSet, interval, skillSetsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
			}
		}

		private static IEnumerable<ISkill> findSkillsToShovelTo(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillSet> skillSetsWithSameIndex, DateTimePeriod interval,
			IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, CascadingSkillSet skillSet,
			double maxToMoveForThisSkillSet)
		{
			ICollection<ISkill> affectedSkills = subSkillsWithSameIndex.ToList();
			while (true)
			{
				var countBefore = affectedSkills.Count;
				var newAffectedSkills = new List<ISkill>();
				shovelToSubSkills(shovelResourcesState, shovelResourceData, skillSet, interval, skillSetsWithSameIndex,
					shovelingCallback, affectedSkills, maxToMoveForThisSkillSet, (arg1, arg2, arg3, arg4, arg5, arg6, arg7, skill) => newAffectedSkills.Add(skill));
				affectedSkills = newAffectedSkills;
				if (newAffectedSkills.Count == countBefore)
					break;
			}
			return affectedSkills;
		}

		private static void shovelToSubSkills(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, CascadingSkillSet skillSet, DateTimePeriod interval,
			IEnumerable<CascadingSkillSet> skillSetsWithSameIndex, IShovelingCallback shovelingCallback,
			IEnumerable<ISkill> subSkillsWithSameIndex, double maxToMoveForThisSkillSet, 
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

				var proportionalResourcesToMove = (maxToMoveForThisSkillSet -
												   dataForIntervalTo.CalculatedResource * (totalFStaff / dataForIntervalTo.FStaff - 1) +
												   totalResurces -
												   dataForIntervalTo.CalculatedResource) / (totalFStaff / dataForIntervalTo.FStaff);
				if (proportionalResourcesToMove < 0)
					continue;

				var resourceToMove = Math.Min(Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove), shovelResourcesState.RemainingOverstaffing);
				shovelAction(shovelResourcesState, skillSet, interval, skillSetsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
			}
		}

		private static void doActualShoveling(ShovelResourcesState shovelResourcesState, CascadingSkillSet skillSet,
			DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillSetsWithSameIndex, IShovelingCallback shovelingCallback,
			IShovelResourceDataForInterval dataForIntervalTo, double resourceToMove, ISkill skillToMoveTo)
		{
			shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillSet, resourceToMove);
			shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillSetsWithSameIndex, skillSet, resourceToMove);
		}
	}
}