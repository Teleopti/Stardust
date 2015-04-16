﻿using System;
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

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public DaysOffSchedulingService(Func<IAbsencePreferenceScheduler> absencePreferenceScheduler,
		   Func<IDayOffScheduler> dayOffScheduler,
		   Func<IMissingDaysOffScheduler> missingDaysOffScheduler)
		{
			_absencePreferenceScheduler = absencePreferenceScheduler;
			_dayOffScheduler = dayOffScheduler;
			_missingDaysOffScheduler = missingDaysOffScheduler;
		}

		public void Execute(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> allMatrixList, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
		{
			var cancelMe = false;
			EventHandler<SchedulingServiceBaseEventArgs> dayScheduled = (sender, e) =>
			{
				var eventArgs = new SchedulingServiceSuccessfulEventArgs(e.SchedulePart,()=>cancelMe=true)
				{
					Cancel = e.Cancel
				};
				OnDayScheduled(eventArgs);
				e.Cancel = eventArgs.Cancel;
				if (eventArgs.Cancel) cancelMe = true;
			};

			var absencePreferenceScheduler = _absencePreferenceScheduler();
			absencePreferenceScheduler.DayScheduled += dayScheduled;
			absencePreferenceScheduler.AddPreferredAbsence(matrixList, schedulingOptions);
			absencePreferenceScheduler.DayScheduled -= dayScheduled;
			
			if (cancelMe) return;

			var dayOffScheduler = _dayOffScheduler();
			dayOffScheduler.DayScheduled += dayScheduled;
			dayOffScheduler.DayOffScheduling(matrixList, allMatrixList, rollbackService, schedulingOptions);
			dayOffScheduler.DayScheduled -= dayScheduled;
			
			if (cancelMe) return;

			var missingDaysOffScheduler = _missingDaysOffScheduler();
			missingDaysOffScheduler.DayScheduled += dayScheduled;
			missingDaysOffScheduler.Execute(matrixList, schedulingOptions, rollbackService);
			missingDaysOffScheduler.DayScheduled -= dayScheduled;
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			var handler = DayScheduled;
			if (handler != null)
			{
				handler(this, scheduleServiceBaseEventArgs);
			}
		}
	}
}