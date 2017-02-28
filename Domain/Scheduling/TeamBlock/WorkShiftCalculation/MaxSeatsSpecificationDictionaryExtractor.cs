using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface IMaxSeatsSpecificationDictionaryExtractor
	{
		IDictionary<DateTime, IntervalLevelMaxSeatInfo> ExtractMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo, bool considerEqualMaxAndCalSeatAsBroken);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class MaxSeatsSpecificationDictionaryExtractor : IMaxSeatsSpecificationDictionaryExtractor
	{
		private readonly IIsMaxSeatsReachedOnSkillStaffPeriodSpecification _isMaxSeatsReachedOnSkillStaffPeriodSpecification;
		private readonly IMaxSeatBoostingFactorCalculator _maxSeatBoostingFactorCalculator;
		private readonly IUsedSeats _usedSeats;

		public MaxSeatsSpecificationDictionaryExtractor(IIsMaxSeatsReachedOnSkillStaffPeriodSpecification isMaxSeatsReachedOnSkillStaffPeriodSpecification, 
																						IMaxSeatBoostingFactorCalculator maxSeatBoostingFactorCalculator,
																						IUsedSeats usedSeats)
		{
			_isMaxSeatsReachedOnSkillStaffPeriodSpecification = isMaxSeatsReachedOnSkillStaffPeriodSpecification;
			_maxSeatBoostingFactorCalculator = maxSeatBoostingFactorCalculator;
			_usedSeats = usedSeats;
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
				var calculatedUsedSeats = _usedSeats.Fetch(skillStaffPeriod);
				var isMaxSeatsReachedOnGivenInterval = false;
				if(considerEqualMaxAndCalSeatAsBroken )
					isMaxSeatsReachedOnGivenInterval =_isMaxSeatsReachedOnSkillStaffPeriodSpecification.IsSatisfiedByWithEqualCondition(calculatedUsedSeats,maxSeat);
				else
					isMaxSeatsReachedOnGivenInterval = _isMaxSeatsReachedOnSkillStaffPeriodSpecification.IsSatisfiedByWithoutEqualCondition(calculatedUsedSeats, maxSeat);
				var boostingFactor = _maxSeatBoostingFactorCalculator.GetBoostingFactor(calculatedUsedSeats, maxSeat);

				if(!maxSeatsDictionary.ContainsKey(localStartTime))
					maxSeatsDictionary.Add(localStartTime, new IntervalLevelMaxSeatInfo(isMaxSeatsReachedOnGivenInterval, boostingFactor));
			}
			return maxSeatsDictionary;
		}
		
	}
}