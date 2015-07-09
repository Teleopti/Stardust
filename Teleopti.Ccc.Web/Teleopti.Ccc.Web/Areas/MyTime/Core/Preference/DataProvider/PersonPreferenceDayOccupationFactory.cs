using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PersonPreferenceDayOccupationFactory : IPersonPreferenceDayOccupationFactory
	{	
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceProvider _preferenceProvider;
		private readonly IUserTimeZone _userTimezone;


		public PersonPreferenceDayOccupationFactory(IScheduleProvider scheduleProvider, IPreferenceProvider preferenceProvider, IUserTimeZone userTimezone)
		{
			_scheduleProvider = scheduleProvider;
			_preferenceProvider = preferenceProvider;
			_userTimezone = userTimezone;
		}

		public PersonPreferenceDayOccupation GetPreferenceDayOccupation(IPerson person, DateOnly date)
		{
			var schedule = _scheduleProvider.GetScheduleForPersons(date, new List<IPerson>{ person}).FirstOrDefault();

			var personPreferenceDayOccupation = new PersonPreferenceDayOccupation();
			if (schedule != null && schedule.PersonAssignment() != null)
			{
				var personAssignment = schedule.PersonAssignment();

				personPreferenceDayOccupation.HasDayOff = personAssignment.DayOff() != null;

				if (!personPreferenceDayOccupation.HasDayOff)
				{
					personPreferenceDayOccupation.HasShift = true;

					var localStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.StartDateTime,
						_userTimezone.TimeZone());
					var localEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(personAssignment.Period.EndDateTime,
						_userTimezone.TimeZone());

					var startTime = new TimeSpan(localStartDateTime.Hour, localStartDateTime.Minute, localStartDateTime.Second);

				    var endTime =
				        localEndDateTime.Date != localStartDateTime.Date
				            ? new TimeSpan(1, localEndDateTime.Hour, localEndDateTime.Minute, localEndDateTime.Second)
				            : new TimeSpan(localEndDateTime.Hour, localEndDateTime.Minute, localEndDateTime.Second);
                        

					personPreferenceDayOccupation.StartTimeLimitation = new StartTimeLimitation(startTime, startTime);
					personPreferenceDayOccupation.EndTimeLimitation = new EndTimeLimitation(endTime, endTime);

					var personAbsence = schedule.PersonAbsenceCollection().FirstOrDefault(pa => pa.Period.Contains(personAssignment.Period));
					personPreferenceDayOccupation.HasFullDayAbsence = personAbsence != null;
				
				}
			}

			if (!(personPreferenceDayOccupation.HasDayOff || personPreferenceDayOccupation.HasShift))
			{
				var preference = _preferenceProvider.GetPreferencesForDate(date);

				if (preference != null)
				{
					personPreferenceDayOccupation.HasPreference = true;
					personPreferenceDayOccupation.StartTimeLimitation = preference.Restriction.StartTimeLimitation;
					personPreferenceDayOccupation.EndTimeLimitation = preference.Restriction.EndTimeLimitation;
				}		
			}

			return personPreferenceDayOccupation;
		}
	}
}