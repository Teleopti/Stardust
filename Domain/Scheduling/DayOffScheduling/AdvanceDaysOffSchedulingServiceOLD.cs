using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class AdvanceDaysOffSchedulingServiceOLD
	{
		private readonly IAbsencePreferenceScheduler _absencePreferenceScheduler;
		private readonly ITeamDayOffScheduler _teamDayOffScheduler;
		private readonly ITeamBlockMissingDayOffHandler _missingDaysOffScheduler;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public AdvanceDaysOffSchedulingServiceOLD(IAbsencePreferenceScheduler absencePreferenceScheduler,
			ITeamDayOffScheduler teamDayOffScheduler,
			ITeamBlockMissingDayOffHandler missingDaysOffScheduler)
		{
			_absencePreferenceScheduler = absencePreferenceScheduler;
			_teamDayOffScheduler = teamDayOffScheduler;
			_missingDaysOffScheduler = missingDaysOffScheduler;
		}

		public void Execute(IEnumerable<IScheduleMatrixPro> matrixes, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions,
			IGroupPersonBuilderWrapper groupPersonBuilderForOptimization, DateOnlyPeriod selectedPeriod)
		{
			var cancelMe = false;
			EventHandler<SchedulingServiceBaseEventArgs> dayScheduled = (sender, e) =>
			{
				var eventArgs = new SchedulingServiceSuccessfulEventArgs(e.SchedulePart, () => cancelMe = true)
				{
					Cancel = e.Cancel
				};
				OnDayScheduled(eventArgs);
				e.Cancel = eventArgs.Cancel;
				if (eventArgs.Cancel) cancelMe = true;
			};
			_absencePreferenceScheduler.DayScheduled += dayScheduled;
			_absencePreferenceScheduler.AddPreferredAbsence(matrixes, schedulingOptions);
			_absencePreferenceScheduler.DayScheduled -= dayScheduled;

			if (cancelMe) return;

			_teamDayOffScheduler.DayScheduled += dayScheduled;
			_teamDayOffScheduler.DayOffScheduling(matrixes, selectedPersons, rollbackService, schedulingOptions, groupPersonBuilderForOptimization);
			_teamDayOffScheduler.DayScheduled -= dayScheduled;

			if (cancelMe) return;

			var selectedMatrixes = new List<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in matrixes)
			{
				if (selectedPersons.Contains(scheduleMatrixPro.Person) && scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Intersection(selectedPeriod).HasValue)
					selectedMatrixes.Add(scheduleMatrixPro);
			}

			_missingDaysOffScheduler.DayScheduled += dayScheduled;
			_missingDaysOffScheduler.Execute(selectedMatrixes, schedulingOptions, rollbackService);
			_missingDaysOffScheduler.DayScheduled -= dayScheduled;
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