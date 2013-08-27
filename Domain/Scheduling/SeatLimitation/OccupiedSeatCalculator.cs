using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class OccupiedSeatCalculator : IOccupiedSeatCalculator
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods)
		{
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> pair in relevantSkillStaffPeriods)
			{
				if (pair.Key.SkillType.ForecastSource != ForecastSource.MaxSeatSkill) continue;

				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = pair.Value;
				foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
					double result = relevantProjections.ActivityResourcesWhereSeatRequired(pair.Key, skillStaffPeriod.Period);

					skillStaffPeriod.Payload.CalculatedLoggedOn = result;
					skillStaffPeriod.Payload.CalculatedUsedSeats = result;
				}
			}
		}
	}
}