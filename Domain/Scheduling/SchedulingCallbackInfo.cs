using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCallbackInfo
	{
		public SchedulingCallbackInfo(IScheduleDay scheduleDay, bool wasSuccessful)
		{
			ScheduleDay = scheduleDay;
			WasSuccessful = wasSuccessful;
		}

		public IScheduleDay ScheduleDay { get; }
		public bool WasSuccessful { get; }
	}
}