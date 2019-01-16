using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingExistingShiftNotMatchingEachOtherHint : ISchedulePostHint
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public BlockSchedulingExistingShiftNotMatchingEachOtherHint(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		public void FillResult(HintResult validationResult, SchedulePostHintInput input)
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
				var agentTimezone = person.PermissionInformation.DefaultTimeZone();
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
												assignmenti.ShiftCategory.Description.ShortName, scheduleDaysArray[i].DateOnlyAsPeriod.DateOnly.Date,
												assignmentj.ShiftCategory.Description.ShortName, scheduleDaysArray[j].DateOnlyAsPeriod.DateOnly.Date);
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
											assignmenti.Period.StartDateTimeLocal(agentTimezone).TimeOfDay != assignmentj.Period.StartDateTimeLocal(agentTimezone).TimeOfDay)
										{
											addValidationResult(validationResult, person,
												nameof(Resources.ExistingShiftNotMatchStartTime),
												assignmenti.Period.StartDateTime, scheduleDaysArray[i].DateOnlyAsPeriod.DateOnly.Date,
												assignmentj.Period.StartDateTime, scheduleDaysArray[j].DateOnlyAsPeriod.DateOnly.Date);
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
									if (scheduleDaysArray[i].GetEditorShift() == null)
										continue;
									for (var j = i + 1; j < length; j++)
									{
										if (scheduleDaysArray[j].GetEditorShift() == null)
											continue;

										if (!_scheduleDayEquator.MainShiftBasicEquals(scheduleDaysArray[i].GetEditorShift(), scheduleDaysArray[j].GetEditorShift(), TimeZoneInfo.Utc))
										{
											addValidationResult(validationResult, person,
												nameof(Resources.ExistingShiftNotMatchShift),
												scheduleDaysArray[i].DateOnlyAsPeriod.DateOnly.Date,
												scheduleDaysArray[j].DateOnlyAsPeriod.DateOnly.Date);
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
							if (realSchedulePeriod.Value.EndDate == person.TerminalDate)
								break;
						}
					}
				}
				else if (blockOption.BlockTypeValue == BlockFinderType.BetweenDayOff)
				{
					var scheduleDays = schedule.Value.ScheduledDayCollection(period);
					var periods = person.PersonPeriods(period);
					if(periods.IsEmpty())
						continue;
					var personPeriod = periods.First();

					if (personPeriod.StartDate > scheduleDays.First().DateOnlyAsPeriod.DateOnly)
						continue;

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
					DateTime? firstStartTime = null;
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
						if (personAssignment != null && personAssignment.ShiftLayers.Count() != 0 && personAssignment.ShiftCategory!= null)
						{
							var shiftCategory = personAssignment.ShiftCategory;
							var startTime = personAssignment.Period.StartDateTimeLocal(agentTimezone);
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
											firstDate.Value.Date,
											shiftCategory.Description.ShortName,
											personAssignment.Date.Date);
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
									if (firstStartTime.Value.TimeOfDay != startTime.TimeOfDay)
									{
										addValidationResult(validationResult, person,
											nameof(Resources.ExistingShiftNotMatchStartTime),
											TimeZoneHelper.ConvertToUtc(firstStartTime.Value, agentTimezone),
											firstDate.Value.Date,
											TimeZoneHelper.ConvertToUtc(startTime, agentTimezone),
											personAssignment.Date.Date);
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
									if (firstScheduleDay.GetEditorShift() == null || scheduleDay.GetEditorShift() == null) continue;
									if (!_scheduleDayEquator.MainShiftBasicEquals(firstScheduleDay.GetEditorShift(), scheduleDay.GetEditorShift(), TimeZoneInfo.Utc))
									{
										addValidationResult(validationResult, person,
											nameof(Resources.ExistingShiftNotMatchShift),
											firstDate.Value.Date,
											personAssignment.Date.Date);
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