using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
    public class AdvanceDaysOffSchedulingService
    {
        private readonly AbsencePreferenceScheduling _absencePreferenceScheduling;
	    private readonly TeamDayOffScheduling _teamDayOffScheduling;
	    private readonly TeamBlockMissingDayOffHandling _missingDayOffHandling;

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        public AdvanceDaysOffSchedulingService(AbsencePreferenceScheduling absencePreferenceScheduling,
	        TeamDayOffScheduling teamDayOffScheduling,
            TeamBlockMissingDayOffHandling missingDayOffHandling)
        {
	        _absencePreferenceScheduling = absencePreferenceScheduling;
	        _teamDayOffScheduling = teamDayOffScheduling;
	        _missingDayOffHandling = missingDayOffHandling;
        }

		public void Execute(IEnumerable<IScheduleMatrixPro> matrixes, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions,
			IGroupPersonBuilderWrapper groupPersonBuilderForOptimization, DateOnlyPeriod selectedPeriod)
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
			_absencePreferenceScheduling.DayScheduled += dayScheduled;
			_absencePreferenceScheduling.AddPreferredAbsence(matrixes, schedulingOptions);
			_absencePreferenceScheduling.DayScheduled -= dayScheduled;
            
			if (cancelMe)return;

			_teamDayOffScheduling.DayScheduled += dayScheduled;
			_teamDayOffScheduling.DayOffScheduling(matrixes, selectedPersons, rollbackService, schedulingOptions, groupPersonBuilderForOptimization);
			_teamDayOffScheduling.DayScheduled -= dayScheduled;
            
			if (cancelMe)return;

			var selectedMatrixes = new List<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in matrixes)
			{
				if(selectedPersons.Contains(scheduleMatrixPro.Person) && scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Intersection(selectedPeriod).HasValue)
					selectedMatrixes.Add(scheduleMatrixPro);
			}

			_missingDayOffHandling.DayScheduled += dayScheduled;
			_missingDayOffHandling.Execute(selectedMatrixes, schedulingOptions, rollbackService);
			_missingDayOffHandling.DayScheduled -= dayScheduled;
        }

        protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        {
            EventHandler<SchedulingServiceBaseEventArgs> handler = DayScheduled;
            if (handler != null)
            {
                handler(this, scheduleServiceBaseEventArgs);
            }
        }
    }
}