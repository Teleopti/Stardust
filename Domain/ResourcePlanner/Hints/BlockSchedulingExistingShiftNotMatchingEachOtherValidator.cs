using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class BlockSchedulingExistingShiftNotMatchingEachOtherValidator : IScheduleValidator
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public BlockSchedulingExistingShiftNotMatchingEachOtherValidator(IScheduleDayEquator scheduleDayEquator)
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

				if (blockOption.BlockTypeValue != BlockFinderType.SchedulePeriod) continue;

				var personSchedulePeriods = person.PersonSchedulePeriods(period);

				var lastEndDate = period.StartDate;

				foreach (var personSchedulePeriod in personSchedulePeriods)
				{
					var realSchedulePeriod = personSchedulePeriod.GetSchedulePeriod(lastEndDate);
					while (realSchedulePeriod.HasValue && period.EndDate >= realSchedulePeriod.Value.StartDate)
					{
						var scheduleDays = schedule.Value.ScheduledDayCollection(realSchedulePeriod.Value);

						if (blockOption.UseBlockSameShiftCategory)
						{
							var allShiftCategory =
								scheduleDays.Where(x => x.PersonAssignment() != null && x.PersonAssignment().ShiftCategory != null)
									.Select(x => x.PersonAssignment().ShiftCategory.Id.GetValueOrDefault())
									.Distinct();

							if (allShiftCategory.Count() > 1)
							{
								validationResult.Add(new PersonValidationError
								{
									PersonName = person.Name.ToString(),
									PersonId = person.Id.Value,
									ValidationError = Resources.ExistingShiftNotMatchShiftCategory
								}, GetType());
								break;
							}
						}

						if (blockOption.UseBlockSameStartTime)
						{
							var allStartTimes =
								scheduleDays.Where(x => x.PersonAssignment() != null)
									.Select(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay)
									.Distinct();

							if (allStartTimes.Count() > 1)
							{
								validationResult.Add(new PersonValidationError
								{
									PersonName = person.Name.ToString(),
									PersonId = person.Id.Value,
									ValidationError = Resources.ExistingShiftNotMatchStartTime
								}, GetType());
								break;
							}
						}

						if (blockOption.UseBlockSameShift)
						{
							var scheduleDaysArray = scheduleDays.ToArray();
							var length = scheduleDaysArray.Length;
							var flag = false;
							for (var i = 0; i < length - 1; i++)
							{
								for (var j = i; j < length; j++)
								{
									if (!_scheduleDayEquator.MainShiftEquals(scheduleDaysArray[i], scheduleDaysArray[j]))
									{
										validationResult.Add(new PersonValidationError
										{
											PersonName = person.Name.ToString(),
											PersonId = person.Id.Value,
											ValidationError = Resources.ExistingShiftNotMatchShift
										}, GetType());
										flag = true;
										break;
									}
								}
								if (flag)
									break;
							}
						}

						lastEndDate = realSchedulePeriod.Value.EndDate.AddDays(1);
						realSchedulePeriod = personSchedulePeriod.GetSchedulePeriod(lastEndDate);
					}
				}
			}
		}
	}
}