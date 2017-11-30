using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

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
			_teamInfoFactoryFactory.Create(_schedulerStateHolder().ChoosenAgents, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var matrixList =
				_matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedAgents, selectedPeriod);
			if (!matrixList.Any())
				return false;

			var schedulePartModifyAndRollbackServiceForContractDaysOff =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, new DoNothingScheduleDayChangeCallBack(),
					new ScheduleTagSetter(new NullScheduleTag()));
			_advanceDaysOffSchedulingService.Execute(schedulingCallBack, matrixList, selectedAgents,
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions, _groupPersonBuilderWrapper,
				selectedPeriod);
			var minMaxTime = _workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(matrixList.First(), schedulingOptions);
			var targetTime = _schedulePeriodTargetTimeCalculator.TargetTime(matrixList.First());
			schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();

			return minMaxTime.Minimum <= targetTime && minMaxTime.Maximum >= targetTime;
		}
	}
}