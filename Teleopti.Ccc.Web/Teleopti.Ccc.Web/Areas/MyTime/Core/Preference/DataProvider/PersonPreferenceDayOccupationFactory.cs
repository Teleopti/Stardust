﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PersonPreferenceDayOccupationFactory : IPersonPreferenceDayOccupationFactory
	{
		private readonly IPersonRuleSetBagProvider _personRuleSetBagProvider;
		private readonly IPreferenceProvider _preferenceProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IUserTimeZone _userTimezone;
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;
		private readonly IToggleManager _toggleManager;

		public PersonPreferenceDayOccupationFactory(IScheduleProvider scheduleProvider,
			IPreferenceProvider preferenceProvider,
			IPersonRuleSetBagProvider personRuleSetBagProvider, IUserTimeZone userTimezone,
			IWorkTimeMinMaxCalculator workTimeMinMaxCalculator,
			IToggleManager toggleManager)
		{
			_scheduleProvider = scheduleProvider;
			_preferenceProvider = preferenceProvider;
			_personRuleSetBagProvider = personRuleSetBagProvider;
			_userTimezone = userTimezone;
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
			_toggleManager = toggleManager;
		}

		public PersonPreferenceDayOccupation GetPreferenceDayOccupation(IPerson person, DateOnly date)
		{
			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_PreferencePerformanceForMultipleUsers_43322))
			{
				return GetPreferencePeriodOccupation(person, new DateOnlyPeriod(date, date))[date];
			}

			var schedule = _scheduleProvider.GetScheduleForPersons(date, new List<IPerson> {person}).FirstOrDefault();

			var personPreferenceDayOccupation = new PersonPreferenceDayOccupation();
			if (schedule?.PersonAssignment() != null)
			{
				var personAssignment = schedule.PersonAssignment();

				personPreferenceDayOccupation.HasDayOff = personAssignment.DayOff() != null;

				if (!personPreferenceDayOccupation.HasDayOff && personAssignment.MainActivities().Any())
				{
					personPreferenceDayOccupation.HasShift = true;

					var localStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.StartDateTime,
						_userTimezone.TimeZone());
					var localEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.EndDateTime,
						_userTimezone.TimeZone());

					var startTime = new TimeSpan(localStartDateTime.Hour, localStartDateTime.Minute, localStartDateTime.Second);
					var endTime = new TimeSpan(localEndDateTime.Hour, localEndDateTime.Minute, localEndDateTime.Second);

					if (localEndDateTime.Date != localStartDateTime.Date)
					{
						endTime = endTime.Add(TimeSpan.FromDays(1));
					}

					personPreferenceDayOccupation.StartTimeLimitation = new StartTimeLimitation(startTime, startTime);
					personPreferenceDayOccupation.EndTimeLimitation = new EndTimeLimitation(endTime, endTime);

					var personAbsence = schedule.PersonAbsenceCollection()
						.FirstOrDefault(pa => pa.Period.Contains(personAssignment.Period));
					personPreferenceDayOccupation.HasFullDayAbsence = personAbsence != null;
				}
			}

			if (personPreferenceDayOccupation.HasDayOff || personPreferenceDayOccupation.HasShift)
			{
				return personPreferenceDayOccupation;
			}

			var preference = _preferenceProvider.GetPreferencesForDate(date);
			if (preference == null)
			{
				return personPreferenceDayOccupation;
			}

			var restriction = preference.Restriction;
			var scheduleDay = _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date)) ?? new IScheduleDay[] { };
			var minMax = _workTimeMinMaxCalculator.WorkTimeMinMax(date, _personRuleSetBagProvider.ForDate(person, date),
				scheduleDay.SingleOrDefault());

			personPreferenceDayOccupation.HasPreference = true;

			var noStartTimeRestriction = restriction.StartTimeLimitation == new StartTimeLimitation(null, null) &&
										 minMax?.WorkTimeMinMax != null;
			personPreferenceDayOccupation.StartTimeLimitation = noStartTimeRestriction
				? minMax.WorkTimeMinMax.StartTimeLimitation
				: restriction.StartTimeLimitation;

			var noEndTimeRestriction = restriction.EndTimeLimitation == new EndTimeLimitation(null, null) &&
									   minMax?.WorkTimeMinMax != null;
			personPreferenceDayOccupation.EndTimeLimitation = noEndTimeRestriction
				? minMax.WorkTimeMinMax.EndTimeLimitation
				: restriction.EndTimeLimitation;

			return personPreferenceDayOccupation;
		}

		public Dictionary<DateOnly, PersonPreferenceDayOccupation> GetPreferencePeriodOccupation(IPerson person,
			DateOnlyPeriod period)
		{
			var result = new Dictionary<DateOnly, PersonPreferenceDayOccupation>();
			var schedules = _scheduleProvider.GetScheduleForPersonsInPeriod(period, new List<IPerson> {person}).ToList();
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(period).ToArray();

			var preferences = _preferenceProvider.GetPreferencesForPeriod(period).ToList();

			foreach (var date in period.DayCollection())
			{
				var personPreferenceDayOccupation = new PersonPreferenceDayOccupation();
				var schedule = schedules.FirstOrDefault(s => s.DateOnlyAsPeriod.DateOnly == date);

				if (schedule?.PersonAssignment() != null)
				{
					var personAssignment = schedule.PersonAssignment();

					personPreferenceDayOccupation.HasDayOff = personAssignment.DayOff() != null;

					if (!personPreferenceDayOccupation.HasDayOff && personAssignment.MainActivities().Any())
					{
						personPreferenceDayOccupation.HasShift = true;

						var localStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.StartDateTime,
							_userTimezone.TimeZone());
						var localEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.EndDateTime,
							_userTimezone.TimeZone());

						var startTime = new TimeSpan(localStartDateTime.Hour, localStartDateTime.Minute, localStartDateTime.Second);
						var endTime = new TimeSpan(localEndDateTime.Hour, localEndDateTime.Minute, localEndDateTime.Second);

						if (localEndDateTime.Date != localStartDateTime.Date)
						{
							endTime = endTime.Add(TimeSpan.FromDays(1));
						}

						personPreferenceDayOccupation.StartTimeLimitation = new StartTimeLimitation(startTime, startTime);
						personPreferenceDayOccupation.EndTimeLimitation = new EndTimeLimitation(endTime, endTime);

						var personAbsence =
							schedule.PersonAbsenceCollection().FirstOrDefault(pa => pa.Period.Contains(personAssignment.Period));
						personPreferenceDayOccupation.HasFullDayAbsence = personAbsence != null;
					}
				}

				if (personPreferenceDayOccupation.HasDayOff || personPreferenceDayOccupation.HasShift)
				{
					result.Add(date, personPreferenceDayOccupation);
					continue;
				}

				var preference = preferences.FirstOrDefault(p => p.RestrictionDate == date);
				if (preference == null)
				{
					result.Add(date, personPreferenceDayOccupation);
					continue;
				}

				var restriction = preference.Restriction;

				var scheduleDay = scheduleDays.FirstOrDefault(s => s.DateOnlyAsPeriod.DateOnly == date);
				var minMax = _workTimeMinMaxCalculator.WorkTimeMinMax(date, _personRuleSetBagProvider.ForDate(person, date),
					scheduleDay);

				personPreferenceDayOccupation.HasPreference = true;

				var noStartTimeRestriction = restriction.StartTimeLimitation == new StartTimeLimitation(null, null) &&
											 minMax?.WorkTimeMinMax != null;
				personPreferenceDayOccupation.StartTimeLimitation = noStartTimeRestriction
					? minMax.WorkTimeMinMax.StartTimeLimitation
					: restriction.StartTimeLimitation;

				var noEndTimeRestriction = restriction.EndTimeLimitation == new EndTimeLimitation(null, null) &&
										   minMax?.WorkTimeMinMax != null;
				personPreferenceDayOccupation.EndTimeLimitation = noEndTimeRestriction
					? minMax.WorkTimeMinMax.EndTimeLimitation
					: restriction.EndTimeLimitation;

				result.Add(date, personPreferenceDayOccupation);
			}
			return result;
		}
	}
}