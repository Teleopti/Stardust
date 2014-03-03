using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
    public interface IAdvanceDaysOffSchedulingService
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		void Execute(IList<IScheduleMatrixPro> allMatrixList, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions, 
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization);
    }

    public class AdvanceDaysOffSchedulingService : IAdvanceDaysOffSchedulingService
    {
        private readonly IAbsencePreferenceScheduler _absencePreferenceScheduler;
	    private readonly ITeamDayOffScheduler _teamDayOffScheduler;
        //private readonly ITeamBlockMissingDaysOffScheduler _missingDaysOffScheduler;
        private readonly ITeamBlockMissingDayOffHandler _missingDaysOffScheduler;
        private bool _cancelMe;

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        public AdvanceDaysOffSchedulingService(IAbsencePreferenceScheduler absencePreferenceScheduler,
			ITeamDayOffScheduler teamDayOffScheduler,
            ITeamBlockMissingDayOffHandler missingDaysOffScheduler)
        {
            _absencePreferenceScheduler = absencePreferenceScheduler;
	        _teamDayOffScheduler = teamDayOffScheduler;
	        _missingDaysOffScheduler = missingDaysOffScheduler;
        }



		public void Execute(IList<IScheduleMatrixPro> allMatrixList, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _absencePreferenceScheduler.DayScheduled += dayScheduled;
            _absencePreferenceScheduler.AddPreferredAbsence(allMatrixList, schedulingOptions);
            _absencePreferenceScheduler.DayScheduled -= dayScheduled;
            if (_cancelMe)
                return;

			_teamDayOffScheduler.DayScheduled += dayScheduled;
			_teamDayOffScheduler.DayOffScheduling(allMatrixList, selectedPersons, rollbackService, schedulingOptions, groupPersonBuilderForOptimization);
			_teamDayOffScheduler.DayScheduled -= dayScheduled;
            if (_cancelMe)
                return;

			var selectedMatrixes = new List<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in allMatrixList)
			{
				if(selectedPersons.Contains(scheduleMatrixPro.Person))
					selectedMatrixes.Add(scheduleMatrixPro);
			}

            _missingDaysOffScheduler.DayScheduled += dayScheduled;
			_missingDaysOffScheduler.Execute(selectedMatrixes, schedulingOptions, rollbackService);
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

		void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			var eventArgs = new SchedulingServiceSuccessfulEventArgs(e.SchedulePart);
			eventArgs.Cancel = e.Cancel;
			OnDayScheduled(eventArgs);
			e.Cancel = eventArgs.Cancel;
			if (eventArgs.Cancel)
				_cancelMe = true;
		}
    }
}