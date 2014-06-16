using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatsSpecificationDictionaryExtractor
	{
		IDictionary<DateTime, IntervalLevelMaxSeatInfo> ExtractMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo, bool considerEqualMaxAndCalSeatAsBroken);
	}

	public class MaxSeatsSpecificationDictionaryExtractor : IMaxSeatsSpecificationDictionaryExtractor
	{
		private readonly IIsMaxSeatsReachedOnSkillStaffPeriodSpecification _isMaxSeatsReachedOnSkillStaffPeriodSpecification;
		private readonly MaxSeatBoostingFactorCalculator _maxSeatBoostingFactorCalculator;

		public MaxSeatsSpecificationDictionaryExtractor(IIsMaxSeatsReachedOnSkillStaffPeriodSpecification isMaxSeatsReachedOnSkillStaffPeriodSpecification, MaxSeatBoostingFactorCalculator maxSeatBoostingFactorCalculator)
		{
			_isMaxSeatsReachedOnSkillStaffPeriodSpecification = isMaxSeatsReachedOnSkillStaffPeriodSpecification;
			_maxSeatBoostingFactorCalculator = maxSeatBoostingFactorCalculator;
		}

		public IDictionary<DateTime, IntervalLevelMaxSeatInfo> ExtractMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo, bool considerEqualMaxAndCalSeatAsBroken)
		{
			if (skillStaffPeriodList.IsNullOrEmpty())
				return null;

			var maxSeatsDictionary = new Dictionary<DateTime, IntervalLevelMaxSeatInfo >();
			foreach (var skillStaffPeriod in skillStaffPeriodList)
			{
				var utcPeriod = skillStaffPeriod.Period;
				var localStartTime = DateTime.SpecifyKind(utcPeriod.StartDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
				var maxSeat = skillStaffPeriod.Payload.MaxSeats;
				var calculatedUsedSeats = skillStaffPeriod.Payload.CalculatedUsedSeats;
				var isMaxSeatsReachedOnGivenInterval = false;
				if(considerEqualMaxAndCalSeatAsBroken )
					isMaxSeatsReachedOnGivenInterval =_isMaxSeatsReachedOnSkillStaffPeriodSpecification.IsSatisfiedByWithEqualCondition(calculatedUsedSeats,maxSeat);
				else
					isMaxSeatsReachedOnGivenInterval = _isMaxSeatsReachedOnSkillStaffPeriodSpecification.IsSatisfiedByWithoutEqualCondition(calculatedUsedSeats, maxSeat);
				var boostingFactor = _maxSeatBoostingFactorCalculator.GetBoostingFactor(calculatedUsedSeats, maxSeat);
				maxSeatsDictionary.Add(localStartTime, new IntervalLevelMaxSeatInfo(isMaxSeatsReachedOnGivenInterval, boostingFactor));
			}
			return maxSeatsDictionary;
		}
		
	}
}