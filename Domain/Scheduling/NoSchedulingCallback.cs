namespace Teleopti.Ccc.Domain.Scheduling
{
	public class NoSchedulingCallback : ISchedulingCallback
	{
		public void Scheduled(SchedulingCallbackInfo schedulingCallbackInfo)
		{
		}

		public bool IsCancelled => false;
	}
}