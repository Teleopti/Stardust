using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourcesToSubSkillsWhenPrimaryIsClosed : AddResourcesToSubSkills
	{
		protected override bool AddResourcesToSkillsWithSameIndex(ShovelResourcesState shovelResourcesState, IShovelResourceData shovelResourceData,
			CascadingSkillGroup skillGroup, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroupsWithSameIndex,
			IShovelingCallback shovelingCallback, SubSkillsWithSameIndex subSkillsWithSameIndex, double totalUnderstaffingPercent,
			double remainingResourcesToShovel, bool stopShovelDueToSubskills)
		{
			foreach (var skillToMoveTo in subSkillsWithSameIndex)
			{
				var dataForIntervalTo = shovelResourceData.GetDataForInterval(skillToMoveTo, interval);
				double resourceToMove;

				if (totalUnderstaffingPercent > 0)
				{
					var skillToMoveToAbsoluteDifference = dataForIntervalTo.AbsoluteDifference;
					if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
						continue;
					var understaffingPercent = -dataForIntervalTo.RelativeDifference;
					var proportionalResourcesToMove = understaffingPercent / totalUnderstaffingPercent * remainingResourcesToShovel;
					resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);
				}
				else
				{
					var proportionalResourcesToMove = remainingResourcesToShovel / subSkillsWithSameIndex.Count();
					resourceToMove = Math.Min(remainingResourcesToShovel, proportionalResourcesToMove);
				}

				shovelResourcesState.AddResourcesTo(dataForIntervalTo, skillGroup, resourceToMove);
				shovelingCallback.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroupsWithSameIndex, skillGroup, resourceToMove);
			}

			return false;
		}
	}
}