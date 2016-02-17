using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class NoBackgroundWorker : ISchedulingProgress
	{
		public bool CancellationPending { get; private set; }
		public void ReportProgress(int percentProgress, object userState = null)
		{
		}
	}
}