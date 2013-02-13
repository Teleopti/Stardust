using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
    public interface IAdvanceDaysOffSchedulingService
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void Execute(IList<IScheduleMatrixPro> allMatrixList, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions);
    }

    public class AdvanceDaysOffSchedulingService : IAdvanceDaysOffSchedulingService
    {
        private readonly IAbsencePreferenceScheduler _absencePreferenceScheduler;
	    private readonly ITeamDayOffScheduler _teamDayOffScheduler;
	    private readonly IMissingDaysOffScheduler _missingDaysOffScheduler;
        private bool _cancelMe;

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        public AdvanceDaysOffSchedulingService(IAbsencePreferenceScheduler absencePreferenceScheduler,
			ITeamDayOffScheduler teamDayOffScheduler, 
            IMissingDaysOffScheduler missingDaysOffScheduler)
        {
            _absencePreferenceScheduler = absencePreferenceScheduler;
	        _teamDayOffScheduler = teamDayOffScheduler;
	        _missingDaysOffScheduler = missingDaysOffScheduler;
        }


        void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        {
            var eventArgs = new SchedulingServiceBaseEventArgs(e.SchedulePart);
            eventArgs.Cancel = e.Cancel;
            OnDayScheduled(eventArgs);
            e.Cancel = eventArgs.Cancel;
            if (eventArgs.Cancel)
                _cancelMe = true;
        }

        public void Execute(IList<IScheduleMatrixPro> allMatrixList, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
        {
            _absencePreferenceScheduler.DayScheduled += dayScheduled;
            _absencePreferenceScheduler.AddPreferredAbsence(allMatrixList, schedulingOptions);
            _absencePreferenceScheduler.DayScheduled -= dayScheduled;
            if (_cancelMe)
                return;

			_teamDayOffScheduler.DayScheduled += dayScheduled;
			_teamDayOffScheduler.DayOffScheduling(allMatrixList, rollbackService, schedulingOptions);
			_teamDayOffScheduler.DayScheduled -= dayScheduled;
            if (_cancelMe)
                return;

            _missingDaysOffScheduler.DayScheduled += dayScheduled;
            _missingDaysOffScheduler.Execute(allMatrixList, schedulingOptions, rollbackService);
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