using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCloseSchedulingProgress : ISchedulingProgress
	{
		public FakeCloseSchedulingProgress()
		{
			CancellationPending = true; //happens when closing app
		}

		public bool CancellationPending { get; }
		public void ReportProgress(int percentProgress, object userState = null)
		{
		}
	}
}