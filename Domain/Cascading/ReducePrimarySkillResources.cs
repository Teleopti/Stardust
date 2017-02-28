using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ReducePrimarySkillResources
	{
		public void Execute(ShovelResourcesState state, IShovelResourceData shovelResourceData, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroup, IShovelingCallback shovelingCallback)
		{
			if (jumpOutEarly(state))
				return;

			foreach (var keyValueForPrimarySkillAndResources in state.ResourcesAvailableForPrimarySkill)
			{
				var primarySkill = keyValueForPrimarySkillAndResources.Key;
				var shovelDataForInterval = shovelResourceData.GetDataForInterval(primarySkill, interval);
				var overstaffForSkill = keyValueForPrimarySkillAndResources.Value;
				var percentageOverstaff = overstaffForSkill / state.TotalOverstaffingAtStart;
				var resourceToSubtract = state.ResourcesMoved * percentageOverstaff;
				shovelDataForInterval.AddResources(-resourceToSubtract);
				shovelingCallback.ResourcesWasRemovedFrom(primarySkill, interval, skillGroup, resourceToSubtract);
			}
		}

		private static bool jumpOutEarly(ShovelResourcesState state)
		{
			const double tolerance = 0.0000000001;
			return Math.Abs(state.ResourcesMoved) < tolerance || Math.Abs(state.TotalOverstaffingAtStart) < tolerance;
		}
	}
}