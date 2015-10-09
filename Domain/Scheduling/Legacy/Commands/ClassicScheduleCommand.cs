using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IClassicScheduleCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, IBackgroundWorkerWrapper backgroundWorker,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper, IList<IScheduleDay> selectedSchedules, bool runWeeklyRestSolver);
	}

	public class ClassicScheduleCommand : IClassicScheduleCommand
	{
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly Func<IResourceOptimizationHelper> _resourceOptimizationHelper;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public ClassicScheduleCommand(IMatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			Func<IResourceOptimizationHelper> resourceOptimizationHelper, IOptimizerHelperHelper optimizerHelper,
			Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_optimizerHelper = optimizerHelper;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IBackgroundWorkerWrapper backgroundWorker,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper, IList<IScheduleDay> selectedSchedules, bool runWeeklyRestSolver)
		{
			var selectedPeriod = _optimizerHelper.GetSelectedPeriod(selectedSchedules);

			IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixList(selectedSchedules,
				selectedPeriod);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return;

			var allScheduleDays = new List<IScheduleDay>();
			var stateHolder = _schedulerStateHolder();

			foreach (var scheduleMatrixPro in matrixesOfSelectedScheduleDays)
			{
				allScheduleDays.AddRange(
					stateHolder.Schedules[scheduleMatrixPro.Person].ScheduledDayCollection(
						scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod).ToList());
			}

			var daysOnlyHelper = new DaysOnlyHelper(schedulingOptions);
			var allMatrixesOfSelectedPersons = _matrixListFactory.CreateMatrixList(allScheduleDays, selectedPeriod);

			if (daysOnlyHelper.DaysOnly)
			{
				if (schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly)
					requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						allMatrixesOfSelectedPersons, true, backgroundWorker,
						daysOnlyHelper.PreferenceOnlyOptions);

				if (schedulingOptions.RotationDaysOnly)
					requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						allMatrixesOfSelectedPersons, true, backgroundWorker,
						daysOnlyHelper.RotationOnlyOptions);

				if (schedulingOptions.AvailabilityDaysOnly)
					requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						allMatrixesOfSelectedPersons, true, backgroundWorker,
						daysOnlyHelper.AvailabilityOnlyOptions);

				if (daysOnlyHelper.UsePreferencesWithNoDaysOnly || daysOnlyHelper.UseRotationsWithNoDaysOnly ||
					daysOnlyHelper.UseAvailabilityWithNoDaysOnly || schedulingOptions.UseStudentAvailability)
					requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						allMatrixesOfSelectedPersons, true, backgroundWorker,
						daysOnlyHelper.NoOnlyOptions);

			}
			else
				requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
					allMatrixesOfSelectedPersons, true, backgroundWorker,
					schedulingOptions);

			if(runWeeklyRestSolver)
				solveWeeklyRest(schedulingOptions, selectedSchedules, stateHolder, selectedPeriod, backgroundWorker);
		}


		private void solveWeeklyRest(ISchedulingOptions schedulingOptions, IList<IScheduleDay> selectedSchedules, ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod, IBackgroundWorkerWrapper backgroundWorker)
		{
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper(), 1, true,
				schedulingOptions.ConsiderShortBreaks);
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons, rollbackService, resourceCalculateDelayer,
				selectedPeriod, _matrixListFactory.CreateMatrixListAll(selectedPeriod), backgroundWorker);
		}
	}
}