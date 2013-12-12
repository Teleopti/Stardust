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
		private readonly IAbsencePreferenceScheduler _absencePreferenceScheduler;
		private readonly IDayOffScheduler _dayOffScheduler;
		private readonly IMissingDaysOffScheduler _missingDaysOffScheduler;
		private bool _cancelMe;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public DaysOffSchedulingService(IAbsencePreferenceScheduler absencePreferenceScheduler,
		   IDayOffScheduler dayOffScheduler,
		   IMissingDaysOffScheduler missingDaysOffScheduler)
		{
			_absencePreferenceScheduler = absencePreferenceScheduler;
			_dayOffScheduler = dayOffScheduler;
			_missingDaysOffScheduler = missingDaysOffScheduler;
		}

		void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			var eventArgs = new SchedulingServiceSuccessfulEventArgs(e.SchedulePart);
 			eventArgs.Cancel = e.Cancel;
			OnDayScheduled(eventArgs);
			e.Cancel = eventArgs.Cancel;
			if (eventArgs.Cancel)
				_cancelMe = true;
		}

		public void Execute(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> allMatrixList, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
		{
			_absencePreferenceScheduler.DayScheduled += dayScheduled;
			_absencePreferenceScheduler.AddPreferredAbsence(matrixList, schedulingOptions);
			_absencePreferenceScheduler.DayScheduled -= dayScheduled;
			if (_cancelMe)
				return;

			_dayOffScheduler.DayScheduled += dayScheduled;
			_dayOffScheduler.DayOffScheduling(matrixList, allMatrixList, rollbackService, schedulingOptions);
			_dayOffScheduler.DayScheduled -= dayScheduled;
			if (_cancelMe)
				return;

			_missingDaysOffScheduler.DayScheduled += dayScheduled;
			_missingDaysOffScheduler.Execute(matrixList, schedulingOptions, rollbackService);
			_missingDaysOffScheduler.DayScheduled -= dayScheduled;
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		public void RaiseEventForTest(object sender, SchedulingServiceBaseEventArgs e)
		{
			dayScheduled(sender, e);
		}
	}
}