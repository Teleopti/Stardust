using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PersonPreferenceDayOccupationFactory : IPersonPreferenceDayOccupationFactory
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceProvider _preferenceProvider;

		public PersonPreferenceDayOccupationFactory(ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider, IPreferenceProvider preferenceProvider)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_preferenceProvider = preferenceProvider;
		}

		public PersonPreferenceDayOccupation GetPreferenceDayOccupation(DateOnly date)
		{
			var schedule = _scheduleProvider.GetScheduleForPersons(date, new List<IPerson>{ _loggedOnUser.CurrentUser() }).FirstOrDefault();

			var personPreferenceDayOccupation = new PersonPreferenceDayOccupation();
			if (schedule != null && schedule.PersonAssignment() != null)
			{
				var personAssignment = schedule.PersonAssignment();

				personPreferenceDayOccupation.HasDayOff = personAssignment.DayOff() != null;

				if (!personPreferenceDayOccupation.HasDayOff)
				{
					personPreferenceDayOccupation.HasShift = true;

					var startTime = new TimeSpan(personAssignment.Period.StartDateTime.Hour, personAssignment.Period.StartDateTime.Minute, personAssignment.Period.StartDateTime.Second);
					var endTime = new TimeSpan(personAssignment.Period.EndDateTime.Hour, personAssignment.Period.EndDateTime.Minute, personAssignment.Period.EndDateTime.Second);

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