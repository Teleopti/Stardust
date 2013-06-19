using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentOvertimeAvailabilityView
	{
		void Update(TimeSpan? startTime, TimeSpan? endTime);
		IScheduleDay ScheduleDay { get; }
		void ShowPreviousSavedOvertimeAvailability(string timePeriod);
	}
}