using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy;
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

		public RestrictionsNotAbleToBeScheduledResult Execute(IVirtualSchedulePeriod schedulePeriod)
		{
			var schedulingOptions = new SchedulingOptions { DayOffTemplate = new DayOffTemplate() };
			var schedulingCallBack = new SchedulingCallbackForDesktop(new NoSchedulingProgress(), new SchedulingOptions());
			var selectedAgents = new List<IPerson> { schedulePeriod.Person };
			var selectedPeriod = schedulePeriod.DateOnlyPeriod;
			var extendedPeriod = new DateOnlyPeriod(selectedPeriod.StartDate, selectedPeriod.EndDate.AddDays(6));
			_teamInfoFactoryFactory.Create(_schedulerStateHolder().ChoosenAgents, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var matrixList =
				_matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedAgents, extendedPeriod).ToList();
			if (!matrixList.Any())
				return null;

			var restrictionFound = false;
			foreach (var dateOnly in selectedPeriod.DayCollection())
			{
				var scheduleDay = _schedulerStateHolder().Schedules[schedulePeriod.Person].ScheduledDay(dateOnly);
				if (scheduleDay.RestrictionCollection().Any())
					restrictionFound = true;
			}

			if (!restrictionFound)
			{
				return new RestrictionsNotAbleToBeScheduledResult
				{
					Agent = schedulePeriod.Person,
					Reason = RestrictionNotAbleToBeScheduledReason.NoRestrictions,
					Period = selectedPeriod,
					Matrix = matrixList.First()
				};
			}
			var schedulePartModifyAndRollbackServiceForContractDaysOff =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, new DoNothingScheduleDayChangeCallBack(),
					new ScheduleTagSetter(new NullScheduleTag()));
			_advanceDaysOffSchedulingService.Execute(schedulingCallBack, matrixList, selectedAgents,
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions, _groupPersonBuilderWrapper,
				extendedPeriod);

			_workShiftMinMaxCalculator.ResetCache();
			var minMaxTime = _workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(matrixList.First(), schedulingOptions);
			var targetTimePeriod = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrixList.First());

			if (toManyDaysOff(matrixList.First()))
			{
				schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
				return new RestrictionsNotAbleToBeScheduledResult
				{
					Agent = schedulePeriod.Person,
					Reason = RestrictionNotAbleToBeScheduledReason.TooManyDaysOff,
					Period = selectedPeriod,
					Matrix = matrixList.First()
				};
			}

			var failingPeriod = checkConflictingRestrictions(matrixList.First());
			if (failingPeriod.HasValue)
			{
				schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
				return new RestrictionsNotAbleToBeScheduledResult
				{
					Agent = schedulePeriod.Person,
					Reason = RestrictionNotAbleToBeScheduledReason.ConflictingRestrictions,
					Period = failingPeriod.Value,
					Matrix = matrixList.First()
				};
			}

			if (minMaxTime.Minimum > targetTimePeriod.EndTime)
			{
				schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
				return new RestrictionsNotAbleToBeScheduledResult
				{
					Agent = schedulePeriod.Person,
					Reason = RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod,
					Period = selectedPeriod,
					Matrix = matrixList.First()
				};
			}
			if (minMaxTime.Maximum < targetTimePeriod.StartTime)
			{
				schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
				return new RestrictionsNotAbleToBeScheduledResult
				{
					Agent = schedulePeriod.Person,
					Reason = RestrictionNotAbleToBeScheduledReason.TooLittleWorkTimeInPeriod,
					Period = selectedPeriod,
					Matrix = matrixList.First()
				};
			}

			IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths =
				_workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(matrixList.First(), new SchedulingOptions(), null);
			failingPeriod = checkWeeks(matrixList.First(), schedulePeriod, possibleMinMaxWorkShiftLengths);
			if (failingPeriod.HasValue)
			{
				schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
				return new RestrictionsNotAbleToBeScheduledResult
				{
					Agent = schedulePeriod.Person,
					Reason = RestrictionNotAbleToBeScheduledReason.TooMuchWorkTimeInPeriod,
					Period = failingPeriod.Value,
					Matrix = matrixList.First()
				};
			}

			failingPeriod = checkNigthlyRest(matrixList.First());
			if (failingPeriod.HasValue)
			{
				schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
				return new RestrictionsNotAbleToBeScheduledResult
				{
					Agent = schedulePeriod.Person,
					Reason = RestrictionNotAbleToBeScheduledReason.NightlyRestMightBeBroken,
					Period = failingPeriod.Value,
					Matrix = matrixList.First()
				};
			}

			schedulePartModifyAndRollbackServiceForContractDaysOff.RollbackMinimumChecks();
			return new RestrictionsNotAbleToBeScheduledResult
			{
				Agent = schedulePeriod.Person,
				Reason = RestrictionNotAbleToBeScheduledReason.NoIssue,
				Period = selectedPeriod,
				Matrix = matrixList.First()
			};
		}

		private bool toManyDaysOff(IScheduleMatrixPro matrix)
		{
			var currentDaysOff = 0;
			foreach (var matrixEffectivePeriodDay in matrix.EffectivePeriodDays)
			{
				if (matrixEffectivePeriodDay.DaySchedulePart().HasDayOff())
					currentDaysOff++;
			}

			var targetDaysOff = matrix.SchedulePeriod.DaysOff();
			var positiveTolerance = matrix.Person.Period(matrix.EffectivePeriodDays[0].Day).PersonContract.Contract
				.PositiveDayOffTolerance;

			return currentDaysOff > targetDaysOff + positiveTolerance;
		}

		//TODO simplify this code
		private DateOnlyPeriod? checkNigthlyRest(IScheduleMatrixPro matrix)
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

				if(significant1 == SchedulePartView.MainShift && significant2 == SchedulePartView.MainShift)
					continue;

				DateTime earliestEnd;
				if (significant1 == SchedulePartView.MainShift)
				{
					earliestEnd = scheduleDay1.ProjectionService().CreateProjection().Period().Value.EndDateTime;
					earliestEnd = TimeZoneHelper.ConvertFromUtc(earliestEnd, matrix.Person.PermissionInformation.DefaultTimeZone());
				}
				else
				{
					var effectiveRestriction = _restrictionExtractor.Extract(scheduleDay1).CombinedRestriction(new SchedulingOptions());
					if (effectiveRestriction == null)
						return null;

					if (effectiveRestriction.EndTimeLimitation.StartTime.HasValue)
					{
						earliestEnd = dateOnly.Date.Add(effectiveRestriction.EndTimeLimitation.StartTime.Value);
					}
					else
					{
						earliestEnd = dateOnly.Date.Add(workShiftEarliestEnd);
					}
				}

				DateTime latestStart;
				if (significant2 == SchedulePartView.MainShift)
				{
					latestStart = scheduleDay2.ProjectionService().CreateProjection().Period().Value.StartDateTime;
					latestStart = TimeZoneHelper.ConvertFromUtc(latestStart, matrix.Person.PermissionInformation.DefaultTimeZone());
				}
				else
				{
					var effectiveRestriction = _restrictionExtractor.Extract(scheduleDay2).CombinedRestriction(new SchedulingOptions());
					if (effectiveRestriction == null)
						return null;

					latestStart = dateOnly.AddDays(1).Date.Add(effectiveRestriction.StartTimeLimitation.EndTime.HasValue
						? effectiveRestriction.StartTimeLimitation.EndTime.Value
						: workShiftLatestStart);
				}

				if (latestStart.Subtract(earliestEnd).TotalHours < nightlyRest.TotalHours)
					return new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			}

			return null;
		}

		private DateOnlyPeriod? checkConflictingRestrictions(IScheduleMatrixPro matrix)
		{
			foreach (var dateOnly in matrix.SchedulePeriod.DateOnlyPeriod.DayCollection())
			{
				var scheduleDay = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
				if (scheduleDay.IsScheduled())
					continue;

				var effectiveRestriction = _restrictionExtractor.Extract(scheduleDay).CombinedRestriction(new SchedulingOptions());
				if(effectiveRestriction == null)
					return new DateOnlyPeriod(dateOnly, dateOnly);
			}

			return null;
		}

		private DateOnlyPeriod? checkWeeks(IScheduleMatrixPro matrix, IVirtualSchedulePeriod virtualSchedulePeriod, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths)
		{
			var weekCount = _workShiftMinMaxCalculator.WeekCount(matrix);
			if (virtualSchedulePeriod == null || !virtualSchedulePeriod.IsValid || virtualSchedulePeriod.Contract == null)
				return null;
			var maxTimePerWeek = virtualSchedulePeriod.Contract.WorkTimeDirective.MaxTimePerWeek;
			for (int weekIndex = 0; weekIndex < weekCount; weekIndex++)
			{
				if (weekIndex == 0) // || weekIndex == weekCount - 1)
				{
					var skipThisWeek = _workShiftMinMaxCalculatorSkipWeekCheck.SkipWeekCheck(matrix, firstDateInWeekIndex(weekIndex, matrix));
					if (skipThisWeek)
						continue;
				}

				var currentMinMaxForWeek = currentMinMax(weekIndex, possibleMinMaxWorkShiftLengths, matrix);
				if(currentMinMaxForWeek.Minimum > maxTimePerWeek)
				{
					var firstDate = firstDateInWeekIndex(weekIndex, matrix);
					return new DateOnlyPeriod(firstDate, firstDate.AddDays(6));
				}
			}

			return null;
		}

		private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
		{
			return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
		}

		private static MinMax<TimeSpan> currentMinMax(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, IScheduleMatrixPro matrix)
		{
			var min = TimeSpan.Zero;
			var max = TimeSpan.Zero;
			var firstDate = possibleMinMaxWorkShiftLengths.Keys.FirstOrDefault();
			for (var i = 0; i <= 6; i++)
			{
				int dayIndex;
				checked
				{
					dayIndex = (weekIndex * 7) + i;
				}
				var scheduleDayPro = matrix.GetScheduleDayByKey(firstDate.AddDays(dayIndex));

				TimeSpan contractTime;
				var scheduleDay = scheduleDayPro.DaySchedulePart();
				var significant = scheduleDay.SignificantPart();
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