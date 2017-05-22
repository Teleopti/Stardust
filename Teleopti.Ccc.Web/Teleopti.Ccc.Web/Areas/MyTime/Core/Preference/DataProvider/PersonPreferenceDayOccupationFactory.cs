using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
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
				return GetPreferencePeriodOccupation(person, date.ToDateOnlyPeriod())[date];
			}

			var loadOptions = new ScheduleDictionaryLoadOptions(true, false);
			var schedule = _scheduleProvider.GetScheduleForPersonsWithOptions(date, new List<IPerson> {person}, loadOptions)
				.FirstOrDefault();

			var personPreferenceDayOccupation = new PersonPreferenceDayOccupation();
			var personAssignment = schedule?.PersonAssignment();
			if (personAssignment != null)
			{
				personPreferenceDayOccupation.HasDayOff = personAssignment.DayOff() != null;

				if (!personPreferenceDayOccupation.HasDayOff && personAssignment.MainActivities().Any())
				{
					personPreferenceDayOccupation.HasShift = true;

					var timeZone = _userTimezone.TimeZone();
					var localStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.StartDateTime,
						timeZone);
					var localEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.EndDateTime,
						timeZone);

					var startTime = localStartDateTime.TimeOfDay;
					var endTime = localEndDateTime.Subtract(localStartDateTime.Date);
					
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
			WorkTimeMinMaxCalculationResult minMax = null;
			if (schedule != null)
			{
				minMax = _workTimeMinMaxCalculator.WorkTimeMinMax(date, _personRuleSetBagProvider.ForDate(person, date), schedule);
			}

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
			var loadOptions = new ScheduleDictionaryLoadOptions(true, false);
			var schedules = _scheduleProvider.GetScheduleForPersonsInPeriod(period, new List<IPerson> {person}, loadOptions)
				.ToDictionary(d => d.DateOnlyAsPeriod.DateOnly);

			var ruleSetBags = _personRuleSetBagProvider.ForPeriod(person, period);
			var preferences = _preferenceProvider.GetPreferencesForPeriod(period).ToList();
			var timeZone = _userTimezone.TimeZone();

			foreach (var date in period.DayCollection())
			{
				var personPreferenceDayOccupation = new PersonPreferenceDayOccupation();

				IScheduleDay schedule;
				if (schedules.TryGetValue(date, out schedule))
				{
					var personAssignment = schedule.PersonAssignment();
					if (personAssignment != null)
					{
						personPreferenceDayOccupation.HasDayOff = personAssignment.DayOff() != null;

						if (!personPreferenceDayOccupation.HasDayOff && personAssignment.MainActivities().Any())
						{
							personPreferenceDayOccupation.HasShift = true;

							var localStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.StartDateTime, timeZone);
							var localEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.EndDateTime, timeZone);

							var startTime = localStartDateTime.TimeOfDay;
							var endTime = localEndDateTime.Subtract(localStartDateTime.Date);

							personPreferenceDayOccupation.StartTimeLimitation = new StartTimeLimitation(startTime, startTime);
							personPreferenceDayOccupation.EndTimeLimitation = new EndTimeLimitation(endTime, endTime);

							var personAbsence =
								schedule.PersonAbsenceCollection().FirstOrDefault(pa => pa.Period.Contains(personAssignment.Period));
							personPreferenceDayOccupation.HasFullDayAbsence = personAbsence != null;
						}
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

				WorkTimeMinMaxCalculationResult minMax = null;
				if (schedule != null)
				{
					minMax = _workTimeMinMaxCalculator.WorkTimeMinMax(date, ruleSetBags[date], schedule);
				}

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