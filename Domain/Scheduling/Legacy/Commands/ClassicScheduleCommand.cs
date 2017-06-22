using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class ClassicScheduleCommand
	{
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly Func<IResourceCalculation> _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IRequiredScheduleHelper _requiredScheduleOptimizerHelper;

		public ClassicScheduleCommand(MatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			Func<IResourceCalculation> resourceOptimizationHelper,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IUserTimeZone userTimeZone,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper)
		{
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_userTimeZone = userTimeZone;
			_requiredScheduleOptimizerHelper = requiredScheduleOptimizerHelper;
		}

		public void Execute(SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod, bool runWeeklyRestSolver)
		{
			var matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedAgents, selectedPeriod);
			if (!matrixesOfSelectedScheduleDays.Any())
				return;

			var daysOnlyHelper = new DaysOnlyHelper(schedulingOptions);
			if (daysOnlyHelper.DaysOnly)
			{
				if (schedulingOptions.PreferencesDaysOnly || schedulingOptions.UsePreferencesMustHaveOnly)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedAgents, selectedPeriod, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.PreferenceOnlyOptions);

				if (schedulingOptions.RotationDaysOnly)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedAgents, selectedPeriod, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.RotationOnlyOptions);

				if (schedulingOptions.AvailabilityDaysOnly)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedAgents, selectedPeriod, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.AvailabilityOnlyOptions);

				if (daysOnlyHelper.UsePreferencesWithNoDaysOnly || daysOnlyHelper.UseRotationsWithNoDaysOnly ||
					daysOnlyHelper.UseAvailabilityWithNoDaysOnly || schedulingOptions.UseStudentAvailability)
					_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedAgents, selectedPeriod, matrixesOfSelectedScheduleDays,
						backgroundWorker,
						daysOnlyHelper.NoOnlyOptions);

			}
			else
				_requiredScheduleOptimizerHelper.ScheduleSelectedPersonDays(selectedAgents, selectedPeriod, matrixesOfSelectedScheduleDays,
					backgroundWorker,
					schedulingOptions);

			if(runWeeklyRestSolver)
				solveWeeklyRest(schedulingOptions, selectedAgents, _schedulerStateHolder(), selectedPeriod, backgroundWorker);
		}


		private void solveWeeklyRest(SchedulingOptions schedulingOptions, IEnumerable<IPerson> selectedPersons, ISchedulerStateHolder schedulerStateHolder, 
									DateOnlyPeriod selectedPeriod, ISchedulingProgress backgroundWorker)
		{
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper(), 1, schedulingOptions.ConsiderShortBreaks, _schedulerStateHolder().SchedulingResultState, _userTimeZone);
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons.ToList(), rollbackService, resourceCalculateDelayer,
				selectedPeriod, _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.PersonsInOrganization, selectedPeriod), backgroundWorker, null);
		}
	}
}