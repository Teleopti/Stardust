using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public class ClassicScheduleCommand
	{
		private readonly IMatrixListFactory _matrixListFactory;

		public ClassicScheduleCommand(IMatrixListFactory matrixListFactory)
		{
			_matrixListFactory = matrixListFactory;
		}

		public void Execute(ISchedulingOptions schedulingOptions, BackgroundWorker backgroundWorker, ScheduleOptimizerHelper scheduleOptimizerHelper, IList<IScheduleDay> selectedSchedules, ISchedulerStateHolder schedulerStateHolder)
		{
			var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedSchedules);

			IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixList(selectedSchedules, selectedPeriod);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return;

			var allScheduleDays = new List<IScheduleDay>();

			foreach (var scheduleMatrixPro in matrixesOfSelectedScheduleDays)
			{
				allScheduleDays.AddRange(schedulerStateHolder.Schedules[scheduleMatrixPro.Person].ScheduledDayCollection(scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod).ToList());
			}

			var daysOnlyHelper = new DaysOnlyHelper(schedulingOptions);
			var allMatrixesOfSelectedPersons = _matrixListFactory.CreateMatrixList(allScheduleDays, selectedPeriod);

			if (daysOnlyHelper.DaysOnly)
			{
				if (schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, backgroundWorker, daysOnlyHelper.PreferenceOnlyOptions);

				if (schedulingOptions.RotationDaysOnly)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, backgroundWorker, daysOnlyHelper.RotationOnlyOptions);

				if (schedulingOptions.AvailabilityDaysOnly)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, backgroundWorker, daysOnlyHelper.AvailabilityOnlyOptions);

				if (daysOnlyHelper.UsePreferencesWithNoDaysOnly || daysOnlyHelper.UseRotationsWithNoDaysOnly || daysOnlyHelper.UseAvailabilityWithNoDaysOnly || schedulingOptions.UseStudentAvailability)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, backgroundWorker, daysOnlyHelper.NoOnlyOptions);

			}
			else
				scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays, allMatrixesOfSelectedPersons, true, backgroundWorker, schedulingOptions); 
				
		}
	}
}