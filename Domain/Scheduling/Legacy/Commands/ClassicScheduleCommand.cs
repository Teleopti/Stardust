using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class ClassicScheduleCommand
	{
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly Func<IResourceCalculation> _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IRequiredScheduleHelper _requiredScheduleOptimizerHelper;

		public ClassicScheduleCommand(IMatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			Func<IResourceCalculation> resourceOptimizationHelper,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			PeriodExtractorFromScheduleParts periodExtractor,
			IUserTimeZone userTimeZone,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper)
		{
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_periodExtractor = periodExtractor;
			_userTimeZone = userTimeZone;
			_requiredScheduleOptimizerHelper = requiredScheduleOptimizerHelper;
		}

		public void Execute(SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker, IEnumerable<IScheduleDay> selectedSchedules, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, bool runWeeklyRestSolver)
		{
			var selectedPeriod = _periodExtractor.ExtractPeriod(selectedSchedules);

			IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedSchedules);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return;

			var daysOnlyHelper = new DaysOnlyHelper(schedulingOptions);
			if (daysOnlyHelper.DaysOnly)
			{
				if (schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.PreferenceOnlyOptions);

				if (schedulingOptions.RotationDaysOnly)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.RotationOnlyOptions);

				if (schedulingOptions.AvailabilityDaysOnly)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.AvailabilityOnlyOptions);

				if (daysOnlyHelper.UsePreferencesWithNoDaysOnly || daysOnlyHelper.UseRotationsWithNoDaysOnly ||
					daysOnlyHelper.UseAvailabilityWithNoDaysOnly || schedulingOptions.UseStudentAvailability)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.NoOnlyOptions);

			}
			else
				_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedSchedules, matrixesOfSelectedScheduleDays,
					backgroundWorker,
					schedulingOptions);

			if(runWeeklyRestSolver && selectedPeriod.HasValue)
				solveWeeklyRest(schedulingOptions, selectedSchedules, _schedulerStateHolder(), selectedPeriod.Value, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}


		private void solveWeeklyRest(SchedulingOptions schedulingOptions, IEnumerable<IScheduleDay> selectedSchedules, ISchedulerStateHolder schedulerStateHolder, 
									DateOnlyPeriod selectedPeriod, ISchedulingProgress backgroundWorker, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper(), 1, schedulingOptions.ConsiderShortBreaks, _schedulerStateHolder().SchedulingResultState, _userTimeZone);
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons, rollbackService, resourceCalculateDelayer,
				selectedPeriod, _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.PersonsInOrganization, selectedPeriod), backgroundWorker, dayOffOptimizationPreferenceProvider);
		}
	}
}