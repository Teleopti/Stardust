using System.ComponentModel;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class BackgroundWorkerWrapper : ISchedulingProgress
	{
		private readonly BackgroundWorker _backgroundWorker;

		public BackgroundWorkerWrapper(BackgroundWorker backgroundWorker)
		{
			_backgroundWorker = backgroundWorker;
		}

		public bool CancellationPending { get { return _backgroundWorker.CancellationPending; } }
		public void ReportProgress(int percentProgress, object userState = null)
		{
			_backgroundWorker.ReportProgress(percentProgress,userState);
		}
	}
}