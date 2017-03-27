using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PersonPreferenceDayOccupationFactory : IPersonPreferenceDayOccupationFactory
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceProvider _preferenceProvider;
		private readonly IPersonRuleSetBagProvider _personRuleSetBagProvider;
		private readonly IUserTimeZone _userTimezone;
		private readonly IWorkTimeMinMaxCalculator _workTimeMinMaxCalculator;

		public PersonPreferenceDayOccupationFactory(IScheduleProvider scheduleProvider, IPreferenceProvider preferenceProvider,
			IPersonRuleSetBagProvider personRuleSetBagProvider, IUserTimeZone userTimezone,
			IWorkTimeMinMaxCalculator workTimeMinMaxCalculator)
		{
			_scheduleProvider = scheduleProvider;
			_preferenceProvider = preferenceProvider;
			_personRuleSetBagProvider = personRuleSetBagProvider;
			_userTimezone = userTimezone;
			_workTimeMinMaxCalculator = workTimeMinMaxCalculator;
		}

		public PersonPreferenceDayOccupation GetPreferenceDayOccupation(IPerson person, DateOnly date)
		{
			var schedule = _scheduleProvider.GetScheduleForPersons(date, new List<IPerson> { person }).FirstOrDefault();

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

					var personAbsence = schedule.PersonAbsenceCollection().FirstOrDefault(pa => pa.Period.Contains(personAssignment.Period));
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

			var noStartTimeRestriction = restriction.StartTimeLimitation == new StartTimeLimitation(null, null) && minMax?.WorkTimeMinMax != null;
			personPreferenceDayOccupation.StartTimeLimitation = noStartTimeRestriction
				? minMax.WorkTimeMinMax.StartTimeLimitation
				: restriction.StartTimeLimitation;

			var noEndTimeRestriction = restriction.EndTimeLimitation == new EndTimeLimitation(null, null) && minMax?.WorkTimeMinMax != null;
			personPreferenceDayOccupation.EndTimeLimitation = noEndTimeRestriction
				? minMax.WorkTimeMinMax.EndTimeLimitation
				: restriction.EndTimeLimitation;

			return personPreferenceDayOccupation;
		}
	}
}