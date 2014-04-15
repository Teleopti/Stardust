using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public class ClassicScheduleCommand
	{
		private readonly IMatrixListFactory _matrixListFactory;
	    private readonly IWeeklyRestSolverCommand  _weeklyRestSolverCommand;
	    private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
	    private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

	    public ClassicScheduleCommand(IMatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand, IScheduleDayChangeCallback scheduleDayChangeCallback, IResourceOptimizationHelper resourceOptimizationHelper)
	    {
	        _matrixListFactory = matrixListFactory;
	        _weeklyRestSolverCommand = weeklyRestSolverCommand;
	        _scheduleDayChangeCallback = scheduleDayChangeCallback;
	        _resourceOptimizationHelper = resourceOptimizationHelper;
	    }

	    public void Execute(ISchedulingOptions schedulingOptions, BackgroundWorker backgroundWorker,
		                    ScheduleOptimizerHelper scheduleOptimizerHelper, IList<IScheduleDay> selectedSchedules,
		                    ISchedulerStateHolder schedulerStateHolder)
		{
			var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedSchedules);

			IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixList(selectedSchedules,
			                                                                                               selectedPeriod);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return;

			var allScheduleDays = new List<IScheduleDay>();

			foreach (var scheduleMatrixPro in matrixesOfSelectedScheduleDays)
			{
				allScheduleDays.AddRange(
					schedulerStateHolder.Schedules[scheduleMatrixPro.Person].ScheduledDayCollection(
						scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod).ToList());
			}

			var daysOnlyHelper = new DaysOnlyHelper(schedulingOptions);
			var allMatrixesOfSelectedPersons = _matrixListFactory.CreateMatrixList(allScheduleDays, selectedPeriod);

			if (daysOnlyHelper.DaysOnly)
			{
				if (schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
					                                                   allMatrixesOfSelectedPersons, true, backgroundWorker,
					                                                   daysOnlyHelper.PreferenceOnlyOptions);

				if (schedulingOptions.RotationDaysOnly)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
					                                                   allMatrixesOfSelectedPersons, true, backgroundWorker,
					                                                   daysOnlyHelper.RotationOnlyOptions);

				if (schedulingOptions.AvailabilityDaysOnly)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
					                                                   allMatrixesOfSelectedPersons, true, backgroundWorker,
					                                                   daysOnlyHelper.AvailabilityOnlyOptions);

				if (daysOnlyHelper.UsePreferencesWithNoDaysOnly || daysOnlyHelper.UseRotationsWithNoDaysOnly ||
				    daysOnlyHelper.UseAvailabilityWithNoDaysOnly || schedulingOptions.UseStudentAvailability)
					scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
					                                                   allMatrixesOfSelectedPersons, true, backgroundWorker,
					                                                   daysOnlyHelper.NoOnlyOptions);

			}
			else
				scheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
				                                                   allMatrixesOfSelectedPersons, true, backgroundWorker,
				                                                   schedulingOptions);

            runWeeklyRestSolver(schedulingOptions, selectedSchedules, schedulerStateHolder, selectedPeriod,backgroundWorker );
		}


	    private void runWeeklyRestSolver(ISchedulingOptions schedulingOptions, IList<IScheduleDay> selectedSchedules, ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod, BackgroundWorker backgroundWorker)
	    {
	        var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
	            schedulingOptions.ConsiderShortBreaks);
	        ISchedulePartModifyAndRollbackService rollbackService =
	            new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
	                _scheduleDayChangeCallback,
	                new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
	        var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
	        _weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons, rollbackService, resourceCalculateDelayer,
	            selectedPeriod, _matrixListFactory.CreateMatrixListAll(selectedPeriod),backgroundWorker );
	    }

	    
	}
}