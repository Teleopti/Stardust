using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityDayCreator
	{
		IStudentAvailabilityDay Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, bool endNextDay);
		bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, bool endNextDay);
	}

	public class AgentStudentAvailabilityDayCreator : IAgentStudentAvailabilityDayCreator
	{
		public IStudentAvailabilityDay Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, bool endNextDay)
		{
			if(scheduleDay == null) throw new ArgumentNullException("scheduleDay");

			if (!CanCreate(startTime, endTime, endNextDay)) return null;

			var restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(startTime, null);
			restriction.EndTimeLimitation = new EndTimeLimitation(endTime, null);
			var studentAvailabilityDay = new StudentAvailabilityDay(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, new List<IStudentAvailabilityRestriction> { restriction });
			
			return studentAvailabilityDay;
		}

		public bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, bool endNextDay)
		{
			if (startTime == null && endTime == null) return false;

			if (startTime != null && endTime != null && endNextDay == false)
			{
				if (startTime.Value >= endTime.Value) return false;
			}

			return true;
		}
	}
}
