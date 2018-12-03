using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public class RemoveShiftCategoryToBeInLegalState
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly WeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly TeamBlockRetryRemoveShiftCategoryBackToLegalService _teamBlockRemoveShiftCategoryBackToLegalService;

		public RemoveShiftCategoryToBeInLegalState(Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			MatrixListFactory matrixListFactory,
			WeeklyRestSolverCommand weeklyRestSolverCommand,
			IUserTimeZone userTimeZone,
			IResourceCalculation resourceCalculation,
			TeamBlockRetryRemoveShiftCategoryBackToLegalService teamBlockRemoveShiftCategoryBackToLegalService)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_userTimeZone = userTimeZone;
			_resourceCalculation = resourceCalculation;
			_teamBlockRemoveShiftCategoryBackToLegalService = teamBlockRemoveShiftCategoryBackToLegalService;
		}

		public void Execute(bool useShiftCategoryLimitations, SchedulingOptions schedulingOptions,
			IEnumerable<IPerson> selectedPersons,
			DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker)
		{
			schedulingOptions.UseShiftCategoryLimitations = useShiftCategoryLimitations;
			if (schedulingOptions.UseShiftCategoryLimitations)
			{
				var schedulerStateHolder = _schedulerStateHolder();
				var matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedPersons, selectedPeriod);
				if (!matrixesOfSelectedScheduleDays.Any())
					return;

				_teamBlockRemoveShiftCategoryBackToLegalService.Execute(schedulingOptions, _schedulerStateHolder().SchedulingResultState, matrixesOfSelectedScheduleDays, backgroundWorker);

				var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
				_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons, rollbackService,
					resourceCalculateDelayer, selectedPeriod, matrixesOfSelectedScheduleDays, backgroundWorker, null);
			}
		}
	}
}