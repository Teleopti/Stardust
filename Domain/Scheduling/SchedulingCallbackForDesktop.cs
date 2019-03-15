using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCallbackForDesktop : ISchedulingCallback, IConvertSchedulingCallbackToSchedulingProgress
	{
		private readonly ISchedulingProgress _schedulingProgress;
		private readonly SchedulingOptions _schedulingOptions;
		private int _scheduledCount;
		private readonly int? _totalToBeScheduled;
		private int _totalScheduledCount;

		public SchedulingCallbackForDesktop(ISchedulingProgress schedulingProgress, SchedulingOptions schedulingOptions, int? totalToBeScheduled = null)
		{
			_schedulingProgress = schedulingProgress;
			_schedulingOptions = schedulingOptions;
			_totalToBeScheduled = totalToBeScheduled;
		}

		public void Scheduled(SchedulingCallbackInfo schedulingCallbackInfo)
		{
			_totalScheduledCount++;
			if (schedulingCallbackInfo.WasSuccessful)
			{
				_scheduledCount++;
			}
			if (_scheduledCount >= _schedulingOptions.RefreshRate) //use timer instead
			{
				var scheduledPercent = _totalToBeScheduled.HasValue ? _totalScheduledCount / (double)_totalToBeScheduled : 1;
				if (scheduledPercent > 99) scheduledPercent = 99;
				_schedulingProgress.ReportProgress((int)(scheduledPercent * 100), new SchedulingServiceSuccessfulEventArgs(schedulingCallbackInfo.ScheduleDay));
				_scheduledCount = 0;
			} 
		}

		public bool IsCancelled => _schedulingProgress.CancellationPending;

		public ISchedulingProgress Convert()
		{
			return _schedulingProgress;
		}
	}
}