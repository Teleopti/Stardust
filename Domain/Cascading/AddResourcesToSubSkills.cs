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
					var totalUnderstaffingPercent = shovelResourcesDataForIntervalForUnderstaffedSkills.Sum(x => -x.RelativeDifference);

					var remainingResourcesToShovel = Math.Min(shovelResourcesState.RemainingOverstaffing, maxToMoveForThisSkillGroup);

					foreach (var skillToMoveTo in subSkillsWithSameIndex)
					{
						double? resourceToMove;
						var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
						if (shovelResourcesState.IsAnyPrimarySkillOpen)
						{
							resourceToMove = addResourcesToSkillsWithSameIndex(
								dataForIntervalTo, 
								remainingResourcesToShovel,
								maxToMoveForThisSkillGroup,
								totalFStaff,
								totalResurces);
						}
						else
						{
							stopShovelDueToSubskills = false;
							resourceToMove = addResourcesToSkillsWithSameIndexWhenPrimarySkillIsClosed(dataForIntervalTo,
								subSkillsWithSameIndex, totalUnderstaffingPercent, remainingResourcesToShovel,
								maxToMoveForThisSkillGroup, totalFStaff, totalResurces);
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

		private static double oskarFormula(double resourcesToShovelAllSkills, 
			double demandAllSkills, double presentResourcesAllSkills,
			double demandThisSkill, double presentResourcesThisSkill)
		{
			return (resourcesToShovelAllSkills - presentResourcesThisSkill * (demandAllSkills / demandThisSkill - 1) + presentResourcesAllSkills - presentResourcesThisSkill) /
					  (demandAllSkills / demandThisSkill);
		}

		private static double? addResourcesToSkillsWithSameIndex(IShovelResourceDataForInterval shovelResourceDataForInterval,
			double remainingResourcesToShovel, double maxToMoveForThisSkillGroup, double totalFStaff, double totalResurces)
		{
			var skillToMoveToAbsoluteDifference = shovelResourceDataForInterval.AbsoluteDifference;
			if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
				return null;

			var proportionalResourcesToMove = oskarFormula(maxToMoveForThisSkillGroup,
				totalFStaff, 
				totalResurces, 
				shovelResourceDataForInterval.FStaff, 
				shovelResourceDataForInterval.CalculatedResource);

			return Math.Min(Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove), remainingResourcesToShovel);
		}

		private static double? addResourcesToSkillsWithSameIndexWhenPrimarySkillIsClosed(IShovelResourceDataForInterval shovelResourceDataForInterval,
			SubSkillsWithSameIndex subSkillsWithSameIndex, double totalUnderstaffingPercent, double remainingResourcesToShovel,
			double maxToMoveForThisSkillGroup, double totalFStaff, double totalResurces)
		{
			if (totalUnderstaffingPercent > 0)
			{
				return addResourcesToSkillsWithSameIndex(shovelResourceDataForInterval, remainingResourcesToShovel, maxToMoveForThisSkillGroup, totalFStaff, totalResurces);
			}

			var proportionalResourcesToMove = remainingResourcesToShovel / subSkillsWithSameIndex.Count();
			return Math.Min(remainingResourcesToShovel, proportionalResourcesToMove);
		}
	}
}