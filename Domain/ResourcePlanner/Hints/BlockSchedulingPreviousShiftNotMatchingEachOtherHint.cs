using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingPreviousShiftNotMatchingEachOtherHint : ISchedulePostHint
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public BlockSchedulingPreviousShiftNotMatchingEachOtherHint(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		public void FillResult(HintResult hintResult, SchedulePostHintInput input)
		{
			var people = input.People.ToHashSet();
			var period = input.Period;
			var blockPreferenceProvider = input.BlockPreferenceProvider;
			if (blockPreferenceProvider == null)
				return;

			foreach (var schedule in input.Schedules)
			{
				var personPeriods = schedule.Key.PersonPeriods(input.Period);
				if (personPeriods.Any(x => x.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)) continue;
				var person = schedule.Key;
				if (!people.Contains(person)) continue;
				var blockOption = blockPreferenceProvider.ForAgent(person, period.StartDate);
				if (!blockOption.UseTeamBlockOption || blockOption.BlockTypeValue!=BlockFinderType.BetweenDayOff) continue;

				var periodWith7DaysBack = new DateOnlyPeriod(period.StartDate.AddDays(-7), period.StartDate);
				var scheduleDays = schedule.Value.ScheduledDayCollection(periodWith7DaysBack);
				var firstDayInPeriod = scheduleDays.Last();
				if (!firstDayInPeriod.HasDayOff())
				{
					var reversedScheduleDays = scheduleDays.Reverse();
					if (firstDayInPeriod.PersonAssignment() != null && firstDayInPeriod.PersonAssignment().ShiftLayers.Count() != 0)
					{
						checkIfPreivousDaysMatchEachOther(hintResult, firstDayInPeriod, reversedScheduleDays.Skip(1), person, blockOption);
					}
					else
					{
						if(personPeriods.IsEmpty())
							continue;
						var personPeriod = personPeriods.First();
						if (personPeriod.StartDate > period.StartDate)
							continue;

						if (personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate, period.StartDate, person.FirstDayOfWeek))
						{
							var lastDayInPreviousPeriod = reversedScheduleDays.Skip(1).First();
							if (!lastDayInPreviousPeriod.HasDayOff())
							{
								if (lastDayInPreviousPeriod.PersonAssignment() != null && lastDayInPreviousPeriod.PersonAssignment().ShiftLayers.Count() != 0)
								{
									checkIfPreivousDaysMatchEachOther(hintResult, lastDayInPreviousPeriod, reversedScheduleDays.Skip(2), person, blockOption);
								}
							}
						}
					}
				}
			}
		}

		private void checkIfPreivousDaysMatchEachOther(HintResult validationResult, IScheduleDay firstDayAfterPeriod, IEnumerable<IScheduleDay> period, IPerson person, ExtraPreferences blockOption)
		{
			var personAssignment = firstDayAfterPeriod.PersonAssignment();
			var shiftCategory = personAssignment.ShiftCategory;
			var startTime = personAssignment.Period.StartDateTime;
			foreach (var scheduleDay in period)
			{
				if (scheduleDay.HasDayOff() || scheduleDay.PersonAssignment() == null || !scheduleDay.PersonAssignment().ShiftLayers.Any())
				{
					break;
				}
				if (blockOption.UseBlockSameShiftCategory && scheduleDay.PersonAssignment().ShiftCategory != shiftCategory && shiftCategory != null && scheduleDay.PersonAssignment().ShiftCategory != null)
				{
					addValidationError(validationResult, person, nameof(Resources.ExistingShiftNotMatchShiftCategory),
						shiftCategory.Description.ShortName, firstDayAfterPeriod.DateOnlyAsPeriod.DateOnly.Date,
						 scheduleDay.PersonAssignment().ShiftCategory.Description.ShortName, scheduleDay.DateOnlyAsPeriod.DateOnly.Date);
					break;
				}
				if (blockOption.UseBlockSameStartTime && scheduleDay.PersonAssignment().Period.StartDateTime.TimeOfDay != startTime.TimeOfDay)
				{
					addValidationError(validationResult, person, nameof(Resources.ExistingShiftNotMatchStartTime),
						startTime, firstDayAfterPeriod.DateOnlyAsPeriod.DateOnly.Date,
					    scheduleDay.PersonAssignment().Period.StartDateTime , scheduleDay.DateOnlyAsPeriod.DateOnly.Date);
					break;
				}
				if (blockOption.UseBlockSameShift && !_scheduleDayEquator.MainShiftEquals(scheduleDay, firstDayAfterPeriod))
				{
					addValidationError(validationResult, person, nameof(Resources.ExistingShiftNotMatchShift), personAssignment.Date.Date, scheduleDay.DateOnlyAsPeriod.DateOnly.Date);
					break;
				}
			}
		}

		private void addValidationError(HintResult validationResult, IPerson person, string resourceString, params object[] resourceParameters)
		{
			validationResult.Add(new PersonHintError
			{
				PersonName = person.Name.ToString(),
				PersonId = person.Id.Value,
				ErrorResource = resourceString,
				ErrorResourceData = resourceParameters.ToList()
			}, GetType(), ValidationResourceType.BlockScheduling);
		}
	}
}