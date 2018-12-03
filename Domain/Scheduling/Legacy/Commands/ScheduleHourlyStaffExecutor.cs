using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleHourlyStaffExecutor
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly RequiredScheduleHelper _requiredScheduleOptimizerHelper;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly RemoveShiftCategoryToBeInLegalState _removeShiftCategoryToBeInLegalState;

		public ScheduleHourlyStaffExecutor(Func<ISchedulerStateHolder> schedulerStateHolder,
			RequiredScheduleHelper requiredScheduleOptimizerHelper,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			RemoveShiftCategoryToBeInLegalState removeShiftCategoryToBeInLegalState)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleOptimizerHelper = requiredScheduleOptimizerHelper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_removeShiftCategoryToBeInLegalState = removeShiftCategoryToBeInLegalState;
		}

		public void Execute(SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, 
			DateOnlyPeriod selectedPeriod)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			var lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			//set to false for first scheduling and then use it for RemoveShiftCategoryBackToLegalState
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;

			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.SchedulingResultState, true, selectedPeriod.Inflate(1)))
			{
				var selectedScheduleDays = schedulerStateHolder.Schedules.SchedulesForPeriod(selectedPeriod, selectedAgents.ToArray());
				_requiredScheduleOptimizerHelper.ScheduleSelectedStudents(selectedScheduleDays, backgroundWorker, schedulingOptions);

				if (!backgroundWorker.CancellationPending)
				{
					_removeShiftCategoryToBeInLegalState.Execute(useShiftCategoryLimitations, schedulingOptions, selectedAgents, selectedPeriod, backgroundWorker);
				}
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}
	}
}