using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class OccupiedSeatCalculator : IOccupiedSeatCalculator
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods)
		{
			foreach (KeyValuePair<ISkill, IResourceCalculationPeriodDictionary> pair in relevantSkillStaffPeriods.Items())
			{
				if (pair.Key.SkillType.ForecastSource != ForecastSource.MaxSeatSkill) continue;

				IResourceCalculationPeriodDictionary skillStaffPeriodDictionary = pair.Value;
				foreach (IResourceCalculationPeriod skillStaffPeriod in skillStaffPeriodDictionary.OnlyValues())
				{
					double result = relevantProjections.ActivityResourcesWhereSeatRequired(pair.Key, skillStaffPeriod.CalculationPeriod);

					skillStaffPeriod.SetCalculatedLoggedOn(result);
					skillStaffPeriod.SetCalculatedUsedSeats(result);
				}
			}
		}
	}
}