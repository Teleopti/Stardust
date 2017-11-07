using System;
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

				if (blockOption.BlockTypeValue == BlockFinderType.SchedulePeriod)
				{
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
									addValidationResult(validationResult, person, Resources.ExistingShiftNotMatchShiftCategory);
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
									addValidationResult(validationResult, person, Resources.ExistingShiftNotMatchStartTime);
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
											addValidationResult(validationResult, person, Resources.ExistingShiftNotMatchShift);
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
				else if (blockOption.BlockTypeValue == BlockFinderType.BetweenDayOff)
				{
					var scheduleDays = schedule.Value.ScheduledDayCollection(period);
					var personPeriod = person.PersonPeriods(period).First();
					IShiftCategory firstShiftCategory = null;
					TimeSpan? firstStartTime = null;
					IScheduleDay firstScheduleDay = null;
					foreach (var scheduleDay in scheduleDays)
					{

						var isDayoff = scheduleDay.HasDayOff() ||
									   (scheduleDay.PersonAssignment() == null &&
										!personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate,
											scheduleDay.DateOnlyAsPeriod.DateOnly, person.FirstDayOfWeek));
						if (isDayoff)
						{
							firstShiftCategory = null;
							firstStartTime = null;
							firstScheduleDay = null;
							continue;
						}
						if (scheduleDay.PersonAssignment() != null)
						{
							var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
							var startTime = scheduleDay.PersonAssignment().Period.StartDateTime.TimeOfDay;
							if (blockOption.UseBlockSameShiftCategory)
							{
								if (firstShiftCategory == null)
								{
									firstShiftCategory = shiftCategory;
								}
								else
								{
									if (firstShiftCategory != shiftCategory)
									{
										addValidationResult(validationResult, person, Resources.ExistingShiftNotMatchShiftCategory);
										break;
									}
								}
							}

							if (blockOption.UseBlockSameStartTime)
							{
								if (firstStartTime == null)
								{
									firstStartTime = startTime;
								}
								else
								{
									if (firstStartTime.Value != startTime)
									{
										addValidationResult(validationResult, person, Resources.ExistingShiftNotMatchStartTime);
										break;
									}
								}
							}

							if (blockOption.UseBlockSameShift)
							{
								if (firstScheduleDay == null)
								{
									firstScheduleDay = scheduleDay;
								}
								else
								{
									if (!_scheduleDayEquator.MainShiftEquals(firstScheduleDay, scheduleDay))
									{
										addValidationResult(validationResult, person, Resources.ExistingShiftNotMatchShift);
										break;
									}
								}
							}

						}
					}
				}
			}
		}

		private void addValidationResult(ValidationResult validationResult, IPerson person, string message)
		{
			validationResult.Add(new PersonValidationError
			{
				PersonName = person.Name.ToString(),
				PersonId = person.Id.Value,
				ValidationError = message
			}, GetType());
		}
	}
}