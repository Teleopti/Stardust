using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatsSpecificationDictionaryExtractor
	{
		IDictionary<DateTime, IntervalLevelMaxSeatInfo> ExtractMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo);
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

		public IDictionary<DateTime, IntervalLevelMaxSeatInfo> ExtractMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo)
		{
			if (skillStaffPeriodList.IsNullOrEmpty())
				return null;

			var maxSeatsDictionary = new Dictionary<DateTime, IntervalLevelMaxSeatInfo >();
			foreach (var skillStaffPeriod in skillStaffPeriodList)
			{
				var utcPeriod = skillStaffPeriod.Period;
				var localStartTime = DateTime.SpecifyKind(utcPeriod.StartDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
				var isMaxSeatsReachedOnGivenInterval = _isMaxSeatsReachedOnSkillStaffPeriodSpecification.IsSatisfiedBy(skillStaffPeriod.Payload.CalculatedUsedSeats,
					skillStaffPeriod.Payload.MaxSeats);
				var boostingFactor = _maxSeatBoostingFactorCalculator.GetBoostingFactor(
					skillStaffPeriod.Payload.CalculatedUsedSeats, skillStaffPeriod.Payload.MaxSeats);
				maxSeatsDictionary.Add(localStartTime, new IntervalLevelMaxSeatInfo(isMaxSeatsReachedOnGivenInterval,0.0001 ));
			}
			return maxSeatsDictionary;
		}
	}
}