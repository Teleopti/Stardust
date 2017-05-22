using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class CancelSchedulingCallback : ISchedulingCallback
	{
		public void Scheduling(SchedulingCallbackInfo schedulingCallbackInfo)
		{
			NumberOfScheduleAttempts++;
		}

		public int NumberOfScheduleAttempts { get; private set; }

		public bool IsCancelled => true;
	}
}