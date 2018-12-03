using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
    public class AdvanceDaysOffSchedulingService
    {
        private readonly AbsencePreferenceScheduling _absencePreferenceScheduling;
	    private readonly TeamDayOffScheduling _teamDayOffScheduling;
	    private readonly TeamBlockMissingDayOffHandling _missingDayOffHandling;

        public AdvanceDaysOffSchedulingService(AbsencePreferenceScheduling absencePreferenceScheduling,
	        TeamDayOffScheduling teamDayOffScheduling,
            TeamBlockMissingDayOffHandling missingDayOffHandling)
        {
	        _absencePreferenceScheduling = absencePreferenceScheduling;
	        _teamDayOffScheduling = teamDayOffScheduling;
	        _missingDayOffHandling = missingDayOffHandling;
        }

		public void Execute(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixes, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions, IGroupPersonBuilderWrapper groupPersonBuilderForOptimization, DateOnlyPeriod selectedPeriod)
		{
			_absencePreferenceScheduling.AddPreferredAbsence(schedulingCallback, matrixes, schedulingOptions, rollbackService);
			_teamDayOffScheduling.DayOffScheduling(schedulingCallback, matrixes, selectedPersons, rollbackService, schedulingOptions, groupPersonBuilderForOptimization);

			var selectedMatrixes = new List<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in matrixes)
			{
				if (selectedPersons.Contains(scheduleMatrixPro.Person) && selectedPeriod.Contains(scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod))
					selectedMatrixes.Add(scheduleMatrixPro);
			}
			_missingDayOffHandling.Execute(schedulingCallback, selectedMatrixes, schedulingOptions, rollbackService);
        }
    }
}