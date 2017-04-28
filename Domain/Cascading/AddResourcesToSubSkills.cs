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
						var affectedSkills = findSkillsToShovelTo(shovelResourcesState, shovelResourceData, skillGroupsWithSameIndex, interval, shovelingCallback, subSkillsWithSameIndex, skillGroup, maxToMoveForThisSkillGroup);
						if (affectedSkills.Any())
						{
							shovelToSubSkills(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, affectedSkills, maxToMoveForThisSkillGroup, doActualShoveling);
						}
						else
						{
							if (!shovelResourcesState.IsAnyPrimarySkillOpen)
							{
								var remainingResourcesToShovel = shovelResourcesState.RemainingOverstaffing;
								foreach (var skillToMoveTo in subSkillsWithSameIndex)
								{
									var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
									var resourceToMove = remainingResourcesToShovel / subSkillsWithSameIndex.Count();
									doActualShoveling(shovelResourcesState, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
								}
							}
						}
					}
				}
			}
		}

		private static ICollection<ISkill> findSkillsToShovelTo(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, DateTimePeriod interval,
			IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, CascadingSkillGroup skillGroup,
			double maxToMoveForThisSkillGroup)
		{
			ICollection<ISkill> affectedSkills = subSkillsWithSameIndex.ToList();
			while (true)
			{
				var countBefore = affectedSkills.Count;
				var newAffectedSkills = new List<ISkill>();
				shovelToSubSkills(shovelResourcesState, shovelResourceData, skillGroup, interval, skillGroupsWithSameIndex,
					shovelingCallback, affectedSkills, maxToMoveForThisSkillGroup, (arg1, arg2, arg3, arg4, arg5, arg6, arg7, skill) => newAffectedSkills.Add(skill));
				affectedSkills = newAffectedSkills;
				if (newAffectedSkills.Count == countBefore)
					break;
			}
			return affectedSkills;
		}

		private static void shovelToSubSkills(ShovelResourcesState shovelResourcesState,
			IShovelResourceData shovelResourceData, CascadingSkillGroup skillGroup, DateTimePeriod interval,
			IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback,
			IEnumerable<ISkill> subSkillsWithSameIndex, double maxToMoveForThisSkillGroup, 
			Action<ShovelResourcesState, CascadingSkillGroup, DateTimePeriod , IEnumerable<CascadingSkillGroup>, IShovelingCallback, IShovelResourceDataForInterval, double, ISkill> shovelAction)
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
				shovelAction(shovelResourcesState, skillGroup, interval, skillGroupsWithSameIndex, shovelingCallback, dataForIntervalTo, resourceToMove, skillToMoveTo);
			}
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