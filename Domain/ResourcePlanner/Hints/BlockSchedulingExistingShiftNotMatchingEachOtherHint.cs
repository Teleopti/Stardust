using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingExistingShiftNotMatchingEachOtherHint : IScheduleHint
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public BlockSchedulingExistingShiftNotMatchingEachOtherHint(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		public void FillResult(HintResult validationResult, HintInput input)
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

							var scheduleDaysArray = scheduleDays.ToArray();
							var length = scheduleDaysArray.Length;

							if (blockOption.UseBlockSameShiftCategory)
							{
								var flag = false;
								for (var i = 0; i < length - 1; i++)
								{
									for (var j = i; j < length; j++)
									{
										var assignmenti = scheduleDaysArray[i].PersonAssignment();
										var assignmentj = scheduleDaysArray[j].PersonAssignment();
										if (assignmenti != null &&
											assignmentj != null &&
											assignmenti.ShiftCategory != null &&
											assignmentj.ShiftCategory != null &&
											assignmenti.ShiftCategory != assignmentj.ShiftCategory)
										{
											addValidationResult(validationResult, person, nameof(Resources.ExistingShiftNotMatchShiftCategory),
												assignmenti.ShiftCategory.Description.ShortName, scheduleDaysArray[i].DateOnlyAsPeriod.DateOnly.ToShortDateString(),
												assignmentj.ShiftCategory.Description.ShortName, scheduleDaysArray[j].DateOnlyAsPeriod.DateOnly.ToShortDateString());
											flag = true;
											break;
										}
									}
									if (flag)
										break;
								}
							}

							if (blockOption.UseBlockSameStartTime)
							{
								var flag = false;
								for (var i = 0; i < length - 1; i++)
								{
									for (var j = i; j < length; j++)
									{
										var assignmenti = scheduleDaysArray[i].PersonAssignment();
										var assignmentj = scheduleDaysArray[j].PersonAssignment();
										if (assignmenti != null &&
											assignmentj != null &&
											assignmenti.ShiftLayers.Count() != 0 &&
											assignmentj.ShiftLayers.Count() != 0 &&
											assignmenti.Period.StartDateTime.TimeOfDay != assignmentj.Period.StartDateTime.TimeOfDay)
										{
											addValidationResult(validationResult, person,
												nameof(Resources.ExistingShiftNotMatchStartTime),
													assignmenti.Period.StartDateTime.TimeOfDay.ToString(), scheduleDaysArray[i].DateOnlyAsPeriod.DateOnly.ToShortDateString(),
													assignmentj.Period.StartDateTime.TimeOfDay.ToString(), scheduleDaysArray[j].DateOnlyAsPeriod.DateOnly.ToShortDateString());
											flag = true;
											break;
										}
									}
									if (flag)
										break;
								}
							}

							if (blockOption.UseBlockSameShift)
							{
								var flag = false;
								for (var i = 0; i < length - 1; i++)
								{
									for (var j = i; j < length; j++)
									{
										if (!_scheduleDayEquator.MainShiftEquals(scheduleDaysArray[i], scheduleDaysArray[j]))
										{
											addValidationResult(validationResult, person,
												nameof(Resources.ExistingShiftNotMatchShift), 
												scheduleDaysArray[i].DateOnlyAsPeriod.DateOnly.ToShortDateString(),
												scheduleDaysArray[j].DateOnlyAsPeriod.DateOnly.ToShortDateString());
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
					var shiftsOnNonworkingDays = scheduleDays.Any(scheduleDay =>
					{
						var personAssignment = scheduleDay.PersonAssignment();
						return personAssignment != null && personAssignment.ShiftLayers.Count() != 0 &&
							   !personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate,
								   scheduleDay.DateOnlyAsPeriod.DateOnly, person.FirstDayOfWeek);
					});
					if (shiftsOnNonworkingDays)
						continue;

					IShiftCategory firstShiftCategory = null;
					TimeSpan? firstStartTime = null;
					IScheduleDay firstScheduleDay = null;
					DateOnly? firstDate = null;
					foreach (var scheduleDay in scheduleDays)
					{
						var personAssignment = scheduleDay.PersonAssignment();
						var isDayoff = scheduleDay.HasDayOff() ||
									   (personAssignment == null &&
										!personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate,
											scheduleDay.DateOnlyAsPeriod.DateOnly, person.FirstDayOfWeek));
						if (isDayoff)
						{
							firstShiftCategory = null;
							firstStartTime = null;
							firstScheduleDay = null;
							firstDate = null;
							continue;
						}
						if (personAssignment != null && personAssignment.ShiftLayers.Count() != 0)
						{
							var shiftCategory = personAssignment.ShiftCategory;
							var startTime = personAssignment.Period.StartDateTime.TimeOfDay;
							if (blockOption.UseBlockSameShiftCategory)
							{
								if (firstShiftCategory == null)
								{
									firstShiftCategory = shiftCategory;
									firstDate = personAssignment.Date;
								}
								else
								{
									if (firstShiftCategory != shiftCategory)
									{
										addValidationResult(validationResult, person,
											nameof(Resources.ExistingShiftNotMatchShiftCategory), 
											firstShiftCategory.Description.ShortName, 
											firstDate.Value.ToShortDateString(), 
											shiftCategory.Description.ShortName, 
											personAssignment.Date.ToShortDateString());
										break;
									}
								}
							}

							if (blockOption.UseBlockSameStartTime)
							{
								if (firstStartTime == null)
								{
									firstStartTime = startTime;
									firstDate = personAssignment.Date;
								}
								else
								{
									if (firstStartTime.Value != startTime)
									{
										addValidationResult(validationResult, person,
											nameof(Resources.ExistingShiftNotMatchStartTime), 
											firstStartTime.Value.ToString(), 
											firstDate.Value.ToShortDateString(), 
											startTime.ToString(),
											personAssignment.Date.ToShortDateString());
										break;
									}
								}
							}

							if (blockOption.UseBlockSameShift)
							{
								if (firstScheduleDay == null)
								{
									firstScheduleDay = scheduleDay;
									firstDate = personAssignment.Date;
								}
								else
								{
									if (!_scheduleDayEquator.MainShiftEquals(firstScheduleDay, scheduleDay))
									{
										addValidationResult(validationResult, person,
											nameof(Resources.ExistingShiftNotMatchShift),
											firstDate.Value.ToShortDateString(), 
											personAssignment.Date.ToShortDateString());
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		private void addValidationResult(HintResult validationResult, IPerson person, string resouceName, params object[] resourceParameters)
		{
			validationResult.Add(new PersonHintError
			{
				PersonName = person.Name.ToString(),
				PersonId = person.Id.Value,
				ErrorResource = resouceName,
				ErrorResourceData = resourceParameters.ToList()
			}, GetType(), ValidationResourceType.BlockScheduling);
		}
	}
}