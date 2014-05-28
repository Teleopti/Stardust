using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface ISeatLimitationWorkShiftCalculator2
	{
		double? CalculateShiftValue(IPerson person, IVisualLayerCollection layers, IDictionary<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriods, MaxSeatsFeatureOptions option);
	}

	public class SeatLimitationWorkShiftCalculator2 : ISeatLimitationWorkShiftCalculator2
	{
		private readonly ISeatImpactOnPeriodForProjection _seatImpactOnPeriodForProjection;
		private const int TheBigNumber = -10000;

		public SeatLimitationWorkShiftCalculator2(ISeatImpactOnPeriodForProjection seatImpactOnPeriodForProjection)
		{
			_seatImpactOnPeriodForProjection = seatImpactOnPeriodForProjection;
		}

		public double? CalculateShiftValue(IPerson person, IVisualLayerCollection layers, IDictionary<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriods, MaxSeatsFeatureOptions option)
		{
			DateTimePeriod? vcPeriod = layers.Period();
			if (!vcPeriod.HasValue)
				return 0;

			double result = 0;
			foreach (KeyValuePair<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriodDictionaryKeyValue in skillStaffPeriods)
			{
				ISkill skill = skillStaffPeriodDictionaryKeyValue.Key;
				if (skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
					continue;

				foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> keyValuePair in skillStaffPeriodDictionaryKeyValue.Value)
				{
					if (!keyValuePair.Key.Intersect(vcPeriod.Value))
						continue;

					ISkillStaffPeriod skillStaffPeriod = keyValuePair.Value;
					DateOnly dateOnly = _seatImpactOnPeriodForProjection.SkillStaffPeriodDate(skillStaffPeriod, person);
					if (!_seatImpactOnPeriodForProjection.CheckPersonSkill(skill, person, dateOnly))
						continue;

					double thisResult = _seatImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod, layers);
					//double thisResult = 0;

					if (skillStaffPeriod.Payload.CalculatedUsedSeats + thisResult > skillStaffPeriod.Payload.MaxSeats && option == MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)
						return null;

					if (skillStaffPeriod.Payload.CalculatedUsedSeats + thisResult > skillStaffPeriod.Payload.MaxSeats)
						result += TheBigNumber;
				}
			}

			return result;
		}
	}
}
