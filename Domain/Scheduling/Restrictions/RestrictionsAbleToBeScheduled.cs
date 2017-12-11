using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
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
		private readonly WorkShiftMinMaxCalculatorSkipWeekCheck _workShiftMinMaxCalculatorSkipWeekCheck;
		private readonly IRestrictionExtractor _restrictionExtractor;

		public RestrictionsAbleToBeScheduled(Func<ISchedulerStateHolder> schedulerStateHolder,
			AdvanceDaysOffSchedulingService advanceDaysOffSchedulingService, MatrixListFactory matrixListFactory,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper, TeamInfoFactoryFactory teamInfoFactoryFactory,
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator,
			WorkShiftMinMaxCalculatorSkipWeekCheck workShiftMinMaxCalculatorSkipWeekCheck,
			IRestrictionExtractor restrictionExtractor)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_advanceDaysOffSchedulingService = advanceDaysOffSchedulingService;
			_matrixListFactory = matrixListFactory;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
			_workShiftMinMaxCalculatorSkipWeekCheck = workShiftMinMaxCalculatorSkipWeekCheck;
			_restrictionExtractor = restrictionExtractor;
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

			//TODO jump out as early as possible
			var periodCheck = minMaxTime.Minimum <= targetTimePeriod.EndTime && minMaxTime.Maximum >= targetTimePeriod.StartTime;
			IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths =
				_workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(matrixList.First(), new SchedulingOptions());
			var weekCheck = checkWeeks(matrixList.First(), schedulePeriod, possibleMinMaxWorkShiftLengths);
			var nightlyCheck = checkNigthlyRest(matrixList.First());
			schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
			if (periodCheck && weekCheck && nightlyCheck)
				return true;

			return false;
		}

		//TODO simplify this code
		private bool checkNigthlyRest(IScheduleMatrixPro matrix)
		{
			var nightlyRest = matrix.SchedulePeriod.Contract.WorkTimeDirective.NightlyRest;
			var workShiftLatestStart = TimeSpan.Zero;
			var workShiftEarliestEnd = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
			var ruleSetBag = matrix.Person.Period(matrix.SchedulePeriod.DateOnlyPeriod.StartDate)?.RuleSetBag;
			if (ruleSetBag != null)
			{
				foreach (var workShiftRuleSet in ruleSetBag.RuleSetCollection)
				{
					if(workShiftLatestStart < workShiftRuleSet.TemplateGenerator.StartPeriod.Period.EndTime)
						workShiftLatestStart = workShiftRuleSet.TemplateGenerator.StartPeriod.Period.EndTime;

					if (workShiftEarliestEnd > workShiftRuleSet.TemplateGenerator.EndPeriod.Period.StartTime)
						workShiftEarliestEnd = workShiftRuleSet.TemplateGenerator.EndPeriod.Period.StartTime;
				}
			}
			else
			{
				workShiftLatestStart = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
				workShiftEarliestEnd = TimeSpan.Zero;
			}

			// TODO this checks one day too much in the end
			foreach (var dateOnly in matrix.SchedulePeriod.DateOnlyPeriod.Inflate(1).DayCollection())
			{
				var scheduleDay1 = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
				var significant1 = scheduleDay1.SignificantPart();
				if (significant1 == SchedulePartView.FullDayAbsence || significant1 == SchedulePartView.ContractDayOff || significant1 == SchedulePartView.DayOff)
					continue;

				var scheduleDay2 = matrix.GetScheduleDayByKey(dateOnly.AddDays(1)).DaySchedulePart();
				var significant2 = scheduleDay2.SignificantPart();
				if (significant2 == SchedulePartView.FullDayAbsence || significant2 == SchedulePartView.ContractDayOff || significant2 == SchedulePartView.DayOff)
					continue;

				var earliestEnd = DateTime.MaxValue;
				if (significant1 == SchedulePartView.MainShift)
				{
					earliestEnd = scheduleDay1.ProjectionService().CreateProjection().Period().Value.EndDateTime;
				}
				else
				{
					var effectiveRestriction = _restrictionExtractor.Extract(scheduleDay1).CombinedRestriction(new SchedulingOptions());
					if (effectiveRestriction.EndTimeLimitation.StartTime.HasValue)
					{
						earliestEnd = dateOnly.Date.Add(effectiveRestriction.EndTimeLimitation.StartTime.Value);
					}
					else
					{
						earliestEnd = dateOnly.Date.Add(workShiftEarliestEnd);
					}
				}

				var latestStart = DateTime.MinValue;
				if (significant2 == SchedulePartView.MainShift)
				{
					latestStart = scheduleDay2.ProjectionService().CreateProjection().Period().Value.StartDateTime;
				}
				else
				{
					var effectiveRestriction = _restrictionExtractor.Extract(scheduleDay2).CombinedRestriction(new SchedulingOptions());
					if(effectiveRestriction.StartTimeLimitation.EndTime.HasValue)
					{
						latestStart = dateOnly.AddDays(1).Date.Add(effectiveRestriction.StartTimeLimitation.EndTime.Value);
					}
					else
					{
						latestStart = dateOnly.AddDays(1).Date.Add(workShiftLatestStart);
					}
				}

				if (latestStart.Subtract(earliestEnd).TotalHours < nightlyRest.TotalHours)
					return false;
			}

			return true;
		}

		private bool checkWeeks(IScheduleMatrixPro matrix, IVirtualSchedulePeriod virtualSchedulePeriod, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths)
		{
			var weekCount = _workShiftMinMaxCalculator.WeekCount(matrix);
			var maxTimePerWeek = virtualSchedulePeriod.Contract.WorkTimeDirective.MaxTimePerWeek;
			for (int weekIndex = 0; weekIndex < weekCount; weekIndex++)
			{
				if (weekIndex == 0) // || weekIndex == weekCount - 1)
				{
					var skipThisWeek = _workShiftMinMaxCalculatorSkipWeekCheck.SkipWeekCheck(matrix, firstDateInWeekIndex(weekIndex, matrix));
					if (skipThisWeek)
						return true;
				}

				var currentMinMaxForWeek = currentMinMax(weekIndex, possibleMinMaxWorkShiftLengths, matrix);
				if(currentMinMaxForWeek.Minimum > maxTimePerWeek)
					return false;
			}

			return true;
		}

		private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
		{
			return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
		}

		private static MinMax<TimeSpan> currentMinMax(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, IScheduleMatrixPro matrix)
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