namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ISchedulingCallback
	{
		void Scheduling(SchedulingCallbackInfo schedulingCallbackInfo);
		bool IsCancelled { get; }
	}
}