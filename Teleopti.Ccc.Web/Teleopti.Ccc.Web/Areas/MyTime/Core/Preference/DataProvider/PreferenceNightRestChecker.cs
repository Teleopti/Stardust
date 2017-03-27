using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceNightRestChecker : IPreferenceNightRestChecker
	{
		private readonly IPersonPreferenceDayOccupationFactory _personPreferenceDayOccupationFactory;

		public PreferenceNightRestChecker(IPersonPreferenceDayOccupationFactory personPreferenceDayOccupationFactory)
		{
			_personPreferenceDayOccupationFactory = personPreferenceDayOccupationFactory;
		}

		public PreferenceNightRestCheckResult CheckNightRestViolation(IPerson person, DateOnly date)
		{
			var period = person.Period(date);

			var result = new PreferenceNightRestCheckResult
			{
				ExpectedNightRest = period?.PersonContract.Contract.WorkTimeDirective.NightlyRest ?? TimeSpan.Zero
			};

			var previousDay = date.AddDays(-1);
			var nextDay = date.AddDays(1);

			var previousDayOccupation = _personPreferenceDayOccupationFactory.GetPreferenceDayOccupation(person, previousDay);
			var nextDayOccupation = _personPreferenceDayOccupationFactory.GetPreferenceDayOccupation(person, nextDay);
			var thisDayOccupation = _personPreferenceDayOccupationFactory.GetPreferenceDayOccupation(person, date);

			if (!thisDayOccupation.HasPreference) return result;

			if ((previousDayOccupation.HasPreference || previousDayOccupation.HasShift) &&
				!(previousDayOccupation.HasDayOff || previousDayOccupation.HasFullDayAbsence) &&
				previousDayOccupation.EndTimeLimitation.StartTime.HasValue && thisDayOccupation.StartTimeLimitation.EndTime.HasValue)
			{
				var compareDate1 = previousDay.Date.AddMinutes(previousDayOccupation.EndTimeLimitation.StartTime.Value.TotalMinutes);
				var compareDate2 = date.Date.AddMinutes(thisDayOccupation.StartTimeLimitation.EndTime.Value.TotalMinutes);

				result.RestTimeToPreviousDay = compareDate2 - compareDate1;
				if (result.RestTimeToPreviousDay < result.ExpectedNightRest)
				{
					result.HasViolationToPreviousDay = true;
				}
			}

			if ((nextDayOccupation.HasPreference || nextDayOccupation.HasShift) &&
				!(nextDayOccupation.HasDayOff || nextDayOccupation.HasFullDayAbsence) &&
				nextDayOccupation.StartTimeLimitation.EndTime.HasValue && thisDayOccupation.EndTimeLimitation.StartTime.HasValue)
			{
				var compareDate1 = nextDay.Date.AddMinutes(nextDayOccupation.StartTimeLimitation.EndTime.Value.TotalMinutes);
				var compareDate2 = date.Date.AddMinutes(thisDayOccupation.EndTimeLimitation.StartTime.Value.TotalMinutes);

				result.RestTimeToNextDay = compareDate1 - compareDate2;
				if (result.RestTimeToNextDay < result.ExpectedNightRest)
				{
					result.HasViolationToNextDay = true;
				}
			}

			return result;
		}
	}
}