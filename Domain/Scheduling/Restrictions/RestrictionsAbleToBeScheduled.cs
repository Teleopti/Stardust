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
			var weekCheck = checkWeeks(matrixList.First(), schedulePeriod);
			schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
			if (periodCheck && weekCheck)
				return true;

			return false;
		}

		private bool checkWeeks(IScheduleMatrixPro matrix, IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			var possibleMinMaxWorkShiftLengths =
				_workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(matrix, new SchedulingOptions());
			var weekCount = _workShiftMinMaxCalculator.WeekCount(matrix);
			for (int i = 0; i < weekCount; i++)
			{
				var currentMinMaxForWeek = currentMinMax(i, possibleMinMaxWorkShiftLengths, null, matrix);
				if(currentMinMaxForWeek.Minimum > virtualSchedulePeriod.Contract.WorkTimeDirective.MaxTimePerWeek)
					return false;
			}

			return true;
		}

		private static MinMax<TimeSpan> currentMinMax(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, DateOnly? dayToSchedule, IScheduleMatrixPro matrix)
		{
			TimeSpan min = TimeSpan.Zero;
			TimeSpan max = TimeSpan.Zero;
			DateOnly firstDate = possibleMinMaxWorkShiftLengths.Keys.FirstOrDefault();
			for (int i = 0; i <= 6; i++)
			{
				int dayIndex;
				checked
				{
					dayIndex = (weekIndex * 7) + i;
				}
				IScheduleDayPro scheduleDayPro = matrix.GetScheduleDayByKey(firstDate.AddDays(dayIndex));

				TimeSpan contractTime;
				if (dayToSchedule.HasValue)
				{
					if (scheduleDayPro.Day == dayToSchedule.Value)
					{
						contractTime = TimeSpan.Zero;
						min = min.Add(contractTime);
						max = max.Add(contractTime);
						continue;
					}
				}


				var scheduleDay = scheduleDayPro.DaySchedulePart();
				SchedulePartView significant = scheduleDay.SignificantPart();
				if (significant == SchedulePartView.MainShift || significant == SchedulePartView.FullDayAbsence)
				{
					contractTime = scheduleDay.ProjectionService().CreateProjection().ContractTime();
					min = min.Add(contractTime);
					max = max.Add(contractTime);
				}
				else
				{
					if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
					{
						contractTime = TimeSpan.Zero;
						min = min.Add(contractTime);
						max = max.Add(contractTime);
					}
					else
					{
						min = min.Add(possibleMinMaxWorkShiftLengths[scheduleDayPro.Day].Minimum);
						if (matrix.EffectivePeriodDays.Contains(scheduleDayPro))
							max = max.Add(possibleMinMaxWorkShiftLengths[scheduleDayPro.Day].Maximum);
						else //day is outside schedule period so min should be added
							max = max.Add(possibleMinMaxWorkShiftLengths[scheduleDayPro.Day].Minimum);
					}
				}
			}
			return new MinMax<TimeSpan>(min, max);
		}
	}
}