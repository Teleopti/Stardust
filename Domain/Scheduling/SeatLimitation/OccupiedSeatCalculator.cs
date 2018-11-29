using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class OccupiedSeatCalculator : IOccupiedSeatCalculator
	{
		public void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods)
		{
			foreach (KeyValuePair<ISkill, IResourceCalculationPeriodDictionary> pair in relevantSkillStaffPeriods.Items())
			{
				if (pair.Key.SkillType.ForecastSource != ForecastSource.MaxSeatSkill) continue;

				IResourceCalculationPeriodDictionary skillStaffPeriodDictionary = pair.Value;
				foreach (var keyValuePair in skillStaffPeriodDictionary.Items())
				{
					double result = relevantProjections.ActivityResourcesWhereSeatRequired(pair.Key, keyValuePair.Key);

					keyValuePair.Value.SetCalculatedLoggedOn(result);
					keyValuePair.Value.SetCalculatedUsedSeats(result);
				}
			}
		}
	}
}