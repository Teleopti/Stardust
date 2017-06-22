using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public class RemoveShiftCategoryToBeInLegalState
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IRequiredScheduleHelper _requiredScheduleOptimizerHelper;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IResourceCalculation _resourceCalculation;

		public RemoveShiftCategoryToBeInLegalState(Func<ISchedulerStateHolder> schedulerStateHolder,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			MatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IUserTimeZone userTimeZone,
			IResourceCalculation resourceCalculation)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleOptimizerHelper = requiredScheduleOptimizerHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_userTimeZone = userTimeZone;
			_resourceCalculation = resourceCalculation;
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

				_requiredScheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays,
					backgroundWorker,
					schedulingOptions,
					selectedPeriod);

				var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, 1, schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
				_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons, rollbackService,
					resourceCalculateDelayer, selectedPeriod, matrixesOfSelectedScheduleDays, backgroundWorker, null);
			}
		}
	}
}