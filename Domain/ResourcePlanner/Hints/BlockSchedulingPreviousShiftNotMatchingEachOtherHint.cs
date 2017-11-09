using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingPreviousShiftNotMatchingEachOtherHint : IScheduleHint
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public BlockSchedulingPreviousShiftNotMatchingEachOtherHint(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		public void FillResult(HintResult hintResult, HintInput input)
		{
			var people = input.People;
			var period = input.Period;
			var blockPreferenceProvider = input.BlockPreferenceProvider;
			var scheduleDictionary = input.Schedules ?? input.CurrentSchedule;
			foreach (var schedule in scheduleDictionary)
			{
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
						var personPeriod = person.PersonPeriods(period).First();
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

		private void checkIfPreivousDaysMatchEachOther(HintResult validationResult, IScheduleDay firstDayAfterPeriod, IEnumerable<IScheduleDay> period, IPerson person, IExtraPreferences blockOption)
		{
			var personAssignment = firstDayAfterPeriod.PersonAssignment();
			var shiftCategory = personAssignment.ShiftCategory;
			var startTime = personAssignment.Period.StartDateTime.TimeOfDay;
			foreach (var scheduleDay in period)
			{
				if (scheduleDay.HasDayOff() || scheduleDay.PersonAssignment() == null || !scheduleDay.PersonAssignment().ShiftLayers.Any())
				{
					break;
				}
				if (blockOption.UseBlockSameShiftCategory && scheduleDay.PersonAssignment().ShiftCategory != shiftCategory)
				{
					addValidationError(validationResult, person, string.Format(Resources.PreviousShiftNotMatchShiftCategory, scheduleDay.DateOnlyAsPeriod.DateOnly.ToShortDateString(), shiftCategory.Description.Name));
					break;
				}
				if (blockOption.UseBlockSameStartTime && scheduleDay.PersonAssignment().Period.StartDateTime.TimeOfDay != startTime)
				{
					addValidationError(validationResult, person, string.Format(Resources.PreviousShiftNotMatchStartTime, scheduleDay.DateOnlyAsPeriod.DateOnly.ToShortDateString(), startTime));
					break;
				}
				if (blockOption.UseBlockSameShift && !_scheduleDayEquator.MainShiftEquals(scheduleDay, firstDayAfterPeriod))
				{
					addValidationError(validationResult, person, string.Format(Resources.PreviousShiftNotMatchShift, scheduleDay.DateOnlyAsPeriod.DateOnly.ToShortDateString(), personAssignment.Date.ToShortDateString()));
					break;
				}
			}
		}

		private void addValidationError(HintResult validationResult, IPerson person, string message)
		{
			validationResult.Add(new PersonHintError
			{
				PersonName = person.Name.ToString(),
				PersonId = person.Id.Value,
				ValidationError = message
			}, GetType());
		}
	}
}