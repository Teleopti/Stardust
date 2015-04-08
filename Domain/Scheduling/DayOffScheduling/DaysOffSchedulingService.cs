using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IDaysOffSchedulingService
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		void Execute(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> allMatrixList, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions);
	}

	public class DaysOffSchedulingService : IDaysOffSchedulingService
	{
		private readonly Func<IAbsencePreferenceScheduler> _absencePreferenceScheduler;
		private readonly Func<IDayOffScheduler> _dayOffScheduler;
		private readonly Func<IMissingDaysOffScheduler> _missingDaysOffScheduler;
		private bool _cancelMe;
		private SchedulingServiceBaseEventArgs _progressEvent;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public DaysOffSchedulingService(Func<IAbsencePreferenceScheduler> absencePreferenceScheduler,
		   Func<IDayOffScheduler> dayOffScheduler,
		   Func<IMissingDaysOffScheduler> missingDaysOffScheduler)
		{
			_absencePreferenceScheduler = absencePreferenceScheduler;
			_dayOffScheduler = dayOffScheduler;
			_missingDaysOffScheduler = missingDaysOffScheduler;
		}

		void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			var eventArgs = new SchedulingServiceSuccessfulEventArgs(e.SchedulePart)
			{
				Cancel = e.Cancel,
				UserCancel = e.UserCancel
			};
			OnDayScheduled(eventArgs);
			e.Cancel = eventArgs.Cancel;
			if (eventArgs.Cancel)
				_cancelMe = true;

			if (_progressEvent != null && _progressEvent.UserCancel)
				return;

			_progressEvent = eventArgs;
		}

		public void Execute(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> allMatrixList, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
		{
			var absencePreferenceScheduler = _absencePreferenceScheduler();
			absencePreferenceScheduler.DayScheduled += dayScheduled;
			absencePreferenceScheduler.AddPreferredAbsence(matrixList, schedulingOptions);
			absencePreferenceScheduler.DayScheduled -= dayScheduled;
			if (_cancelMe)
				return;

			if (_progressEvent != null && _progressEvent.UserCancel)
			{
				_progressEvent = null;
				return;
			}

			var dayOffScheduler = _dayOffScheduler();
			dayOffScheduler.DayScheduled += dayScheduled;
			dayOffScheduler.DayOffScheduling(matrixList, allMatrixList, rollbackService, schedulingOptions);
			dayOffScheduler.DayScheduled -= dayScheduled;
			if (_cancelMe)
				return;

			if (_progressEvent != null && _progressEvent.UserCancel)
			{
				_progressEvent = null;
				return;
			}

			var missingDaysOffScheduler = _missingDaysOffScheduler();
			missingDaysOffScheduler.DayScheduled += dayScheduled;
			missingDaysOffScheduler.Execute(matrixList, schedulingOptions, rollbackService);
			missingDaysOffScheduler.DayScheduled -= dayScheduled;
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);

				if (_progressEvent != null && _progressEvent.UserCancel)
					return;

				_progressEvent = scheduleServiceBaseEventArgs;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		public void RaiseEventForTest(object sender, SchedulingServiceBaseEventArgs e)
		{
			dayScheduled(sender, e);
		}
	}
}