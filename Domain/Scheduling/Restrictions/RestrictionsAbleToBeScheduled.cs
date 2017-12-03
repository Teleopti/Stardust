using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class RestrictionsAbleToBeScheduled
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly AdvanceDaysOffSchedulingService _advanceDaysOffSchedulingService;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;

		public RestrictionsAbleToBeScheduled(Func<ISchedulerStateHolder> schedulerStateHolder,
			AdvanceDaysOffSchedulingService advanceDaysOffSchedulingService, MatrixListFactory matrixListFactory,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper, TeamInfoFactoryFactory teamInfoFactoryFactory,
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_advanceDaysOffSchedulingService = advanceDaysOffSchedulingService;
			_matrixListFactory = matrixListFactory;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		public bool Execute(IVirtualSchedulePeriod schedulePeriod)
		{
			var schedulingOptions = new SchedulingOptions { DayOffTemplate = new DayOffTemplate() };
			var schedulingCallBack = new SchedulingCallbackForDesktop(new NoSchedulingProgress(), new SchedulingOptions());
			var selectedAgents = new List<IPerson> { schedulePeriod.Person };
			var selectedPeriod = schedulePeriod.DateOnlyPeriod;
			var extendedPeriod = new DateOnlyPeriod(selectedPeriod.StartDate, selectedPeriod.EndDate.AddDays(6));
			_teamInfoFactoryFactory.Create(_schedulerStateHolder().ChoosenAgents, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var matrixList =
				_matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedAgents, extendedPeriod);
			if (!matrixList.Any())
				return false;

			var schedulePartModifyAndRollbackServiceForContractDaysOff =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, new DoNothingScheduleDayChangeCallBack(),
					new ScheduleTagSetter(new NullScheduleTag()));
			_advanceDaysOffSchedulingService.Execute(schedulingCallBack, matrixList, selectedAgents,
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions, _groupPersonBuilderWrapper,
				extendedPeriod);

			_workShiftMinMaxCalculator.ResetCache();
			var minMaxTime = _workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(matrixList.First(), schedulingOptions);
			var targetTimePeriod = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrixList.First());

			var periodCheck = minMaxTime.Minimum <= targetTimePeriod.EndTime && minMaxTime.Maximum >= targetTimePeriod.StartTime;
			var weekCheck = checkWeeks(matrixList.First());
			schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
			if (periodCheck && weekCheck)
				return true;

			return false;
		}

		private bool checkWeeks(IScheduleMatrixPro matrix)
		{

			var weekCount = _workShiftMinMaxCalculator.WeekCount(matrix);
			for (int i = 0; i < weekCount; i++)
			{
				//_workShiftMinMaxCalculator.ResetCache();
				if (!_workShiftMinMaxCalculator.IsWeekInLegalState(i, matrix, new SchedulingOptions()))
					return false;
			}

			return true;
		}
	}
}