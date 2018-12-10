using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface ISkillStaffPeriodIntraIntervalPeriodFinder
	{
		IList<DateTimePeriod> Find(DateTimePeriod skillStaffPeriod, ShiftProjectionCache shiftProjectionCache, ISkill skill);
	}

	public class SkillStaffPeriodIntraIntervalPeriodFinder : ISkillStaffPeriodIntraIntervalPeriodFinder
	{
		public IList<DateTimePeriod> Find(DateTimePeriod skillStaffPeriod, ShiftProjectionCache shiftProjectionCache, ISkill skill)
		{
			var result = new List<DateTimePeriod>();
			var visualLayers = shiftProjectionCache.MainShiftProjection();
			foreach (var visualLayer in visualLayers)
			{
				if (!visualLayer.Period.Intersect(skillStaffPeriod)) continue;
				var activity = visualLayer.Payload as Activity;
				if (activity == null || !activity.Equals(skill.Activity)) continue;
				result.Add(visualLayer.Period);
			}

			return result;	
		}
	}
}
