using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class OccupiedSeatCalculator : IOccupiedSeatCalculator
	{
		private readonly ISkillVisualLayerCollectionDictionaryCreator _skillVisualLayerCollectionDictionaryCreator;
		private readonly ISeatImpactOnPeriodForProjection _seatImpactOnPeriodForProjection;

		public OccupiedSeatCalculator(ISkillVisualLayerCollectionDictionaryCreator skillVisualLayerCollectionDictionaryCreator, ISeatImpactOnPeriodForProjection seatImpactOnPeriodForProjection)
		{
			_skillVisualLayerCollectionDictionaryCreator = skillVisualLayerCollectionDictionaryCreator;
			_seatImpactOnPeriodForProjection = seatImpactOnPeriodForProjection;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods)
		{
//			SkillVisualLayerCollectionDictionary skillVisualLayerCollectionDictionary =
//				_skillVisualLayerCollectionDictionaryCreator.CreateSiteVisualLayerCollectionDictionary(relevantProjections, day);
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> pair in relevantSkillStaffPeriods)
			{
				if (pair.Key.SkillType.ForecastSource != ForecastSource.MaxSeatSkill) continue;

				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = pair.Value;
				foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
					double result = relevantProjections.SkillResources(pair.Key, skillStaffPeriod.Period);

					skillStaffPeriod.Payload.CalculatedLoggedOn = result;
					skillStaffPeriod.Payload.CalculatedUsedSeats = result;
				}
				
			}
		}
	}
}