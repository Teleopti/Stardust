using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class BlockSchedulingPreviousShiftNotMatchingEachOtherValidator : IScheduleValidator
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public BlockSchedulingPreviousShiftNotMatchingEachOtherValidator(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		public void FillResult(ValidationResult validationResult, ValidationInput input)
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
					if (firstDayInPeriod.PersonAssignment() != null)
					{
						checkIfPreivousDaysMatchEachOther(validationResult, firstDayInPeriod, reversedScheduleDays.Skip(1), person, blockOption);
					}
					else
					{
						var personPeriod = person.PersonPeriods(period).First();
						if (personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate, period.StartDate, person.FirstDayOfWeek))
						{
							var firstPreviousDay = reversedScheduleDays.Skip(1).First();
							if (firstPreviousDay.HasDayOff())
							{
								continue;
							}
							if (firstPreviousDay.PersonAssignment() != null)
							{
								checkIfPreivousDaysMatchEachOther(validationResult, firstPreviousDay, reversedScheduleDays.Skip(2), person, blockOption);
							}
						}
					}
				}
			}
		}

		private void checkIfPreivousDaysMatchEachOther(ValidationResult validationResult, IScheduleDay firstDayAfterPeriod, IEnumerable<IScheduleDay> period, IPerson person, IExtraPreferences blockOption)
		{
			var personAssignment = firstDayAfterPeriod.PersonAssignment();
			var shiftCategory = personAssignment.ShiftCategory;
			var startTime = personAssignment.Period.StartDateTime.TimeOfDay;
			foreach (var scheduleDay in period)
			{
				if (scheduleDay.HasDayOff() || !scheduleDay.HasProjection())
				{
					break;
				}
				if (blockOption.UseBlockSameShiftCategory && scheduleDay.PersonAssignment().ShiftCategory != shiftCategory)
				{
					validationResult.Add(new PersonValidationError
					{
						PersonName = person.Name.ToString(),
						PersonId = person.Id.Value,
						ValidationError =
							string.Format(Resources.PreviousShiftNotMatchShiftCategory, scheduleDay.PersonAssignment().Date,
								shiftCategory.Description.Name)
					}, GetType());
					break;
				}
				if (blockOption.UseBlockSameStartTime && scheduleDay.PersonAssignment().Period.StartDateTime.TimeOfDay != startTime)
				{
					validationResult.Add(new PersonValidationError
					{
						PersonName = person.Name.ToString(),
						PersonId = person.Id.Value,
						ValidationError =
							string.Format(Resources.PreviousShiftNotMatchStartTime, scheduleDay.PersonAssignment().Date,
								startTime)
					}, GetType());
					break;
				}
				if (blockOption.UseBlockSameShift && !_scheduleDayEquator.MainShiftEquals(scheduleDay, firstDayAfterPeriod))
				{
					validationResult.Add(new PersonValidationError
					{
						PersonName = person.Name.ToString(),
						PersonId = person.Id.Value,
						ValidationError =
							string.Format(Resources.PreviousShiftNotMatchShift, scheduleDay.PersonAssignment().Date,
								personAssignment.Date)
					}, GetType());
					break;
				}
			}
		}
	}
}