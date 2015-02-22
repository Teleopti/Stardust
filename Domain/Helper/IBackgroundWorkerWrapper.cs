namespace Teleopti.Ccc.Domain.Helper
{
	public interface IBackgroundWorkerWrapper
	{
		bool CancellationPending { get; }
		void ReportProgress(int percentProgress, object userState = null);
	}
}