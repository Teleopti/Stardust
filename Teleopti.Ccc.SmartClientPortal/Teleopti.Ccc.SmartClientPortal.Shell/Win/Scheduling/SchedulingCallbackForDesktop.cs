using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class SchedulingCallbackForDesktop : ISchedulingCallback
	{
		private readonly ISchedulingProgress _schedulingProgress;
		private readonly SchedulingOptions _schedulingOptions;
		private int _scheduledCount;

		public SchedulingCallbackForDesktop(ISchedulingProgress schedulingProgress, SchedulingOptions schedulingOptions)
		{
			_schedulingProgress = schedulingProgress;
			_schedulingOptions = schedulingOptions;
		}

		public void Scheduled(SchedulingCallbackInfo schedulingCallbackInfo)
		{
			if (schedulingCallbackInfo.WasSuccessful)
			{
				_scheduledCount++;
			}
			if (_scheduledCount >= _schedulingOptions.RefreshRate)
			{
				_schedulingProgress.ReportProgress(1, new SchedulingServiceSuccessfulEventArgs(schedulingCallbackInfo.ScheduleDay));
				_scheduledCount = 0;
			}
		}

		public bool IsCancelled => _schedulingProgress.CancellationPending;
	}
}