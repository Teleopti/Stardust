﻿using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCallbackForDesktop : ISchedulingCallback, IConvertSchedulingCallbackToSchedulingProgress
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

		public ISchedulingProgress Convert()
		{
			return _schedulingProgress;
		}
	}
}