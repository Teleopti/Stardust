namespace Teleopti.Ccc.Domain.Scheduling
{
	public class CurrentSchedulingCallback : ICurrentSchedulingCallback
	{
		public ISchedulingCallback Current()
		{
			return new NoSchedulingCallback();
		}
	}
}