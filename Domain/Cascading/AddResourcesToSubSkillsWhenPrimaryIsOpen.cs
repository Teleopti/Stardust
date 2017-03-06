using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkillsWhenPrimaryIsOpen : AddResourcesToSubSkills
	{
		protected override bool AddResourcesToSkillsWithSameIndex(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData, CascadingSkillGroup skillGroup, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex, IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, double totalUnderstaffingPercent, double remainingResourcesToShovel, bool stopShovelDueToSubskills)
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