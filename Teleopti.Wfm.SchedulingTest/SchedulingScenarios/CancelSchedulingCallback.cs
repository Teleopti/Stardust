using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class CancelSchedulingCallback : ISchedulingCallback, IConvertSchedulingCallbackToSchedulingProgress
	{
		public void Scheduled(SchedulingCallbackInfo schedulingCallbackInfo)
		{
			NumberOfScheduleAttempts++;
		}

		public int NumberOfScheduleAttempts { get; private set; }

		public bool IsCancelled => true;

		public ISchedulingProgress Convert()
		{
			return new FakeCancelSchedulingProgress();
		}
	}
}