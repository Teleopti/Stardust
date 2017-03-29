using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceNightRestChecker : IPreferenceNightRestChecker
	{
		private readonly IPersonPreferenceDayOccupationFactory _personPreferenceDayOccupationFactory;
		private readonly IToggleManager _toggleManager;

		public PreferenceNightRestChecker(IPersonPreferenceDayOccupationFactory personPreferenceDayOccupationFactory,
			IToggleManager toggleManager)
		{
			_personPreferenceDayOccupationFactory = personPreferenceDayOccupationFactory;
			_toggleManager = toggleManager;
		}

		public PreferenceNightRestCheckResult CheckNightRestViolation(IPerson person, DateOnly date)
		{
			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_PreferencePerformanceForMultipleUsers_43322))
			{
				return CheckNightRestViolation(person, date.ToDateOnlyPeriod())[date];
			}

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

			if (!thisDayOccupation.HasPreference)
			{
				return result;
			}

			if ((previousDayOccupation.HasPreference || previousDayOccupation.HasShift) &&
				!(previousDayOccupation.HasDayOff || previousDayOccupation.HasFullDayAbsence) &&
				previousDayOccupation.EndTimeLimitation.StartTime.HasValue &&
				thisDayOccupation.StartTimeLimitation.EndTime.HasValue)
			{
				var compareDate1 = previousDay.Date.Add(previousDayOccupation.EndTimeLimitation.StartTime.Value);
				var compareDate2 = date.Date.Add(thisDayOccupation.StartTimeLimitation.EndTime.Value);

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
				var compareDate1 = nextDay.Date.Add(nextDayOccupation.StartTimeLimitation.EndTime.Value);
				var compareDate2 = date.Date.Add(thisDayOccupation.EndTimeLimitation.StartTime.Value);

				result.RestTimeToNextDay = compareDate1 - compareDate2;
				if (result.RestTimeToNextDay < result.ExpectedNightRest)
				{
					result.HasViolationToNextDay = true;
				}
			}

			return result;
		}

		public IDictionary<DateOnly, PreferenceNightRestCheckResult> CheckNightRestViolation(IPerson person,
			DateOnlyPeriod periods)
		{
			var result = new Dictionary<DateOnly, PreferenceNightRestCheckResult>();
			
			var preferenceDayOccupations = _personPreferenceDayOccupationFactory.GetPreferencePeriodOccupation(person, periods.Inflate(1));

			foreach (var currentDate in periods.DayCollection())
			{
				var personPeriod = person.Period(currentDate);

				var checkResult = new PreferenceNightRestCheckResult
				{
					ExpectedNightRest = personPeriod?.PersonContract.Contract.WorkTimeDirective.NightlyRest ?? TimeSpan.Zero
				};

				var previousDay = currentDate.AddDays(-1);
				var nextDay = currentDate.AddDays(1);

				var previousDayOccupation = preferenceDayOccupations[previousDay];
				var nextDayOccupation = preferenceDayOccupations[nextDay];
				var thisDayOccupation = preferenceDayOccupations[currentDate];

				if (!thisDayOccupation.HasPreference)
				{
					result.Add(currentDate, checkResult);
					continue;
				}

				if ((previousDayOccupation.HasPreference || previousDayOccupation.HasShift) &&
					!(previousDayOccupation.HasDayOff || previousDayOccupation.HasFullDayAbsence) &&
					previousDayOccupation.EndTimeLimitation.StartTime.HasValue &&
					thisDayOccupation.StartTimeLimitation.EndTime.HasValue)
				{
					var compareDate1 =
						previousDay.Date.AddMinutes(previousDayOccupation.EndTimeLimitation.StartTime.Value.TotalMinutes);
					var compareDate2 = currentDate.Date.AddMinutes(thisDayOccupation.StartTimeLimitation.EndTime.Value.TotalMinutes);

					checkResult.RestTimeToPreviousDay = compareDate2 - compareDate1;
					if (checkResult.RestTimeToPreviousDay < checkResult.ExpectedNightRest)
					{
						checkResult.HasViolationToPreviousDay = true;
					}
				}

				if ((nextDayOccupation.HasPreference || nextDayOccupation.HasShift) &&
					!(nextDayOccupation.HasDayOff || nextDayOccupation.HasFullDayAbsence) &&
					nextDayOccupation.StartTimeLimitation.EndTime.HasValue && thisDayOccupation.EndTimeLimitation.StartTime.HasValue)
				{
					var compareDate1 = nextDay.Date.AddMinutes(nextDayOccupation.StartTimeLimitation.EndTime.Value.TotalMinutes);
					var compareDate2 = currentDate.Date.AddMinutes(thisDayOccupation.EndTimeLimitation.StartTime.Value.TotalMinutes);

					checkResult.RestTimeToNextDay = compareDate1 - compareDate2;
					if (checkResult.RestTimeToNextDay < checkResult.ExpectedNightRest)
					{
						checkResult.HasViolationToNextDay = true;
					}
				}

				result.Add(currentDate, checkResult);
			}
			return result;
		}
	}
}