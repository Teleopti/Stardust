using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCancelSchedulingProgress : ISchedulingProgress
	{
		public bool CancellationPending { get; }

		public void ReportProgress(int percentProgress, object userState = null)
		{
			var progress = userState as SchedulingServiceBaseEventArgs;
			progress?.CancelCallback();
		}
	}
}