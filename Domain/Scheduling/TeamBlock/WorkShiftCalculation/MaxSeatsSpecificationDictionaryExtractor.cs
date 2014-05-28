using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatsSpecificationDictionaryExtractor
	{
		IDictionary<DateTime, bool> ExtractMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo);
	}

	public class MaxSeatsSpecificationDictionaryExtractor : IMaxSeatsSpecificationDictionaryExtractor
	{
		private readonly IIsMaxSeatsReachedOnSkillStaffPeriodSpecification _isMaxSeatsReachedOnSkillStaffPeriodSpecification;
		private Dictionary<DateTime, bool> _maxSeatsDictionary;

		public MaxSeatsSpecificationDictionaryExtractor(IIsMaxSeatsReachedOnSkillStaffPeriodSpecification isMaxSeatsReachedOnSkillStaffPeriodSpecification)
		{
			_isMaxSeatsReachedOnSkillStaffPeriodSpecification = isMaxSeatsReachedOnSkillStaffPeriodSpecification;
		}

		public IDictionary<DateTime, bool> ExtractMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo)
		{
			if (skillStaffPeriodList.IsNullOrEmpty())
				return null;

			_maxSeatsDictionary = new Dictionary<DateTime, bool>();
			foreach (var skillStaffPeriod in skillStaffPeriodList)
			{
				var utcPeriod = skillStaffPeriod.Period;
				var localStartTime = DateTime.SpecifyKind(utcPeriod.StartDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
				var isMaxSeatsReachedOnGivenInterval = _isMaxSeatsReachedOnSkillStaffPeriodSpecification.IsSatisfiedBy(skillStaffPeriod.Payload.CalculatedUsedSeats,
					skillStaffPeriod.Payload.MaxSeats);
				_maxSeatsDictionary.Add(localStartTime, isMaxSeatsReachedOnGivenInterval);
			}
			return _maxSeatsDictionary;
		}
	}
}