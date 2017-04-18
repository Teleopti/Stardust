using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityDayCreator
	{
		IStudentAvailabilityDay Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
		bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, out bool startTimeError, out bool endTimeError);
	}

	public class AgentStudentAvailabilityDayCreator : IAgentStudentAvailabilityDayCreator
	{
		public IStudentAvailabilityDay Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime)
		{
			if(scheduleDay == null) throw new ArgumentNullException("scheduleDay");

			bool startTimeError;
			bool endTimeError;
			if (!CanCreate(startTime, endTime, out startTimeError, out endTimeError)) return null;

			var restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(startTime, null);
			restriction.EndTimeLimitation = new EndTimeLimitation(null, endTime);
			var studentAvailabilityDay = new StudentAvailabilityDay(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, new List<IStudentAvailabilityRestriction> { restriction });
			
			return studentAvailabilityDay;
		}

		public bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, out bool startTimeError, out bool endTimeError)
		{
			if (startTime == null && endTime == null)
			{
				startTimeError = true;
				endTimeError = true;
				return false;	
			}
				
			if (startTime != null && endTime != null)
			{
				if (startTime.Value >= endTime.Value)
				{
					startTimeError = true;
					endTimeError = false;
					return false;
				}
			}

			if (startTime == null)
			{
				startTimeError = true;
				endTimeError = false;
				return false;
			}

			if (endTime == null)
			{
				startTimeError = false;
				endTimeError = true;
				return false;
			}

			startTimeError = false;
			endTimeError = false;
			return true;
		}
	}
}
