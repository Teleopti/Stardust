namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ISchedulingCallback
	{
		void Scheduled(SchedulingCallbackInfo schedulingCallbackInfo);
		bool IsCancelled { get; }
	}
}