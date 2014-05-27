using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class MaxSeatsSpecificationDictionary
	{
		private readonly IIsMaxSeatsReachedOnSkillStaffPeriodSpecification _isMaxSeatsReachedOnSkillStaffPeriodSpecification;
		private Dictionary<DateTime, bool> _maxSeatsDictionary;

		public MaxSeatsSpecificationDictionary(IIsMaxSeatsReachedOnSkillStaffPeriodSpecification isMaxSeatsReachedOnSkillStaffPeriodSpecification)
		{
			_isMaxSeatsReachedOnSkillStaffPeriodSpecification = isMaxSeatsReachedOnSkillStaffPeriodSpecification;
		}

		public Dictionary<DateTime, bool> MaxSeatsDictionary
		{
			get { return _maxSeatsDictionary; }
		}

		public IDictionary<DateTime, Boolean> SetMaxSeatsFlag(List<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZoneInfo)
		{
			if (skillStaffPeriodList.IsNullOrEmpty())
				return null;

			_maxSeatsDictionary = new Dictionary<DateTime, Boolean>();
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