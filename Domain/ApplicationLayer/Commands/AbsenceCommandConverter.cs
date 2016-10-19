﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AbsenceCommandConverter : IAbsenceCommandConverter
	{
		private readonly ICurrentScenario _scenario;
		private readonly IProxyForId<IPerson> _personRepository;
		private readonly IProxyForId<IAbsence> _absenceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IUserTimeZone _timeZone;

		public AbsenceCommandConverter(ICurrentScenario scenario, IProxyForId<IPerson> personRepository, IProxyForId<IAbsence> absenceRepository, IScheduleStorage scheduleStorage, IUserTimeZone timeZone)
		{
			_scenario = scenario;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_scheduleStorage = scheduleStorage;
			_timeZone = timeZone;
		}

		public AbsenceCreatorInfo GetCreatorInfoForFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);
			var period = new DateOnlyPeriod(new DateOnly(command.StartDate.AddDays(-1)), new DateOnly(command.EndDate));

			var scheduleRange = getScheduleRangeForPeriod(period, person);
			var scheduleDay = scheduleRange.ScheduledDay(new DateOnly(command.StartDate));

			var absenceTimePeriod = GetFullDayAbsencePeriod(person, command.StartDate, command.EndDate);

			var creatorInfo = new AbsenceCreatorInfo()
			{
				Absence = absence,
				AbsenceTimePeriod = absenceTimePeriod,
				Person = person,
				ScheduleDay = scheduleDay,
				ScheduleRange = scheduleRange,
				TrackedCommandInfo = command.TrackedCommandInfo
			};

			return creatorInfo;
		}

		public DateTimePeriod GetFullDayAbsencePeriod(IPerson person, DateTime start, DateTime end)
		{
			var period = new DateOnlyPeriod(new DateOnly(start.AddDays(-1)), new DateOnly(end));

			var scheduleRange = getScheduleRangeForPeriod(period, person);
			var scheduleDays = scheduleRange.ScheduledDayCollection(period).ToList();

			var previousDay = scheduleDays.First();
			return getDateTimePeriodForAbsence(scheduleDays.Except(new[] { previousDay }), previousDay);
		}

		public AbsenceCreatorInfo GetCreatorInfoForIntradayAbsence(AddIntradayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);
			var absenceTimePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()),
														TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			var scheduleRange = getScheduleRangeForPeriod(absenceTimePeriod.ToDateOnlyPeriod(_timeZone.TimeZone()), person);
			var scheduleDay = scheduleRange.ScheduledDay(new DateOnly(command.StartTime));

			var creatorInfo = new AbsenceCreatorInfo()
			{
				Absence = absence,
				Person = person,
				AbsenceTimePeriod = absenceTimePeriod,
				ScheduleDay = scheduleDay,
				ScheduleRange = scheduleRange,
				TrackedCommandInfo = command.TrackedCommandInfo
			};

			return creatorInfo;
		}

		private IScheduleRange getScheduleRangeForPeriod(DateOnlyPeriod period, IPerson person)
		{
			var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person,
					new ScheduleDictionaryLoadOptions(false, false),
					period,
					_scenario.Current());

			return dictionary[person];
		}

		private DateTimePeriod getDateTimePeriodForAbsence(IEnumerable<IScheduleDay> scheduleDaysInPeriod, IScheduleDay previousDay)
		{
			var firstDayDateTime = getDateTimePeriodForDay(scheduleDaysInPeriod.First());
			var lastDayDateTime = getDateTimePeriodForDay(scheduleDaysInPeriod.Last());
			var previousday = getDateTimePeriodForDay(previousDay);
			var startTime = previousday.EndDateTime > firstDayDateTime.StartDateTime ? previousday.EndDateTime : firstDayDateTime.StartDateTime;
			var endTime = lastDayDateTime.EndDateTime < firstDayDateTime.EndDateTime ? firstDayDateTime.EndDateTime : lastDayDateTime.EndDateTime;
			if (startTime > endTime)
			{
				startTime = firstDayDateTime.StartDateTime;
			}
			return new DateTimePeriod(startTime, endTime);
		}

		private DateTimePeriod getDateTimePeriodForDay(IScheduleDay scheduleDay)
		{
			var startTime = DateTime.MaxValue;
			var endTime = DateTime.MinValue;
			var personAssignment = scheduleDay.PersonAssignment();

			if (personAssignment != null)
			{
				DateTime personAssignmentStartTime;
				DateTime personAssignmentEndTime;
				if (!personAssignment.ShiftLayers.Any())
				{
					personAssignmentStartTime = new DateTime(scheduleDay.DateOnlyAsPeriod.DateOnly.Year, scheduleDay.DateOnlyAsPeriod.DateOnly.Month, scheduleDay.DateOnlyAsPeriod.DateOnly.Day);
					personAssignmentStartTime = TimeZoneHelper.ConvertToUtc(personAssignmentStartTime, scheduleDay.TimeZone);
					personAssignmentEndTime = new DateTime(scheduleDay.DateOnlyAsPeriod.DateOnly.Year, scheduleDay.DateOnlyAsPeriod.DateOnly.Month, scheduleDay.DateOnlyAsPeriod.DateOnly.Day, 23, 59, 0);
					personAssignmentEndTime = TimeZoneHelper.ConvertToUtc(personAssignmentEndTime, scheduleDay.TimeZone);
				}
				else
				{
					personAssignmentStartTime = personAssignment.Period.StartDateTime;
					personAssignmentEndTime = personAssignment.Period.EndDateTime;
				}


				if (personAssignmentStartTime <= startTime)
				{
					startTime = personAssignmentStartTime;
				}

				if (personAssignmentEndTime >= endTime)
				{
					endTime = personAssignmentEndTime;
				}
			}
			else
			{
				startTime = scheduleDay.Period.StartDateTime;
				endTime = scheduleDay.Period.EndDateTime.AddMinutes(-1);
			}

			return new DateTimePeriod(startTime, endTime);
		}
	}
}
