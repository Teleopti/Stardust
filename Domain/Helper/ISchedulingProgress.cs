namespace Teleopti.Ccc.Domain.Helper
{
	public interface ISchedulingProgress
	{
		bool CancellationPending { get; }
		void ReportProgress(int percentProgress, object userState = null);
	}
}