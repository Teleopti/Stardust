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
        public void Calculate(DateOnly day, IList<IVisualLayerCollection> relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods)
		{
			SkillVisualLayerCollectionDictionary skillVisualLayerCollectionDictionary =
				_skillVisualLayerCollectionDictionaryCreator.CreateSiteVisualLayerCollectionDictionary(relevantProjections, day);
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> pair in relevantSkillStaffPeriods)
			{
				IList<IVisualLayerCollection> visualLayerCollections;
				skillVisualLayerCollectionDictionary.TryGetValue(pair.Key, out visualLayerCollections);

				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = pair.Value;
				foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodDictionary.Values)
				{
					double result = 0;
					if (visualLayerCollections != null)
						result = _seatImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod, visualLayerCollections);
					
					skillStaffPeriod.Payload.CalculatedUsedSeats = result;
				}
				
			}
		}
	}
}