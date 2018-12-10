using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceNightRestChecker : IPreferenceNightRestChecker
	{
		private readonly IPersonPreferenceDayOccupationFactory _occupationFactory;

		public PreferenceNightRestChecker(IPersonPreferenceDayOccupationFactory occupationFactory)
		{
			_occupationFactory = occupationFactory;
		}

		public PreferenceNightRestCheckResult CheckNightRestViolation(IPerson person, DateOnly date)
		{
			return CheckNightRestViolation(person, date.ToDateOnlyPeriod(), new Dictionary<DateOnly, WorkTimeMinMaxCalculationResult>())[date];
		}

		public IDictionary<DateOnly, PreferenceNightRestCheckResult> CheckNightRestViolation(IPerson person,
			DateOnlyPeriod periods, IDictionary<DateOnly, WorkTimeMinMaxCalculationResult> workTimeMinMaxCalculationResult)
		{
			var result = new Dictionary<DateOnly, PreferenceNightRestCheckResult>();
			var preferenceDayOccupations = _occupationFactory.GetPreferencePeriodOccupation(person, periods.Inflate(1));

			foreach (var currentDate in periods.DayCollection())
			{

				var personPeriod = person.Period(currentDate);
				var checkResult = initCheckResult(personPeriod);

				var currentDayOccupation = preferenceDayOccupations[currentDate];
				if (!currentDayOccupation.HasPreference)
				{
					result.Add(currentDate, checkResult);
					continue;
				}

				var previousDate = currentDate.AddDays(-1);
				var nextDate = currentDate.AddDays(1);
				var previousDayOccupation = preferenceDayOccupations[previousDate];
				var nextDayOccupation = preferenceDayOccupations[nextDate];

				updatePreferenceTimeLimitationWithWorkTimeLimitation(workTimeMinMaxCalculationResult, previousDayOccupation, previousDate);
				updatePreferenceTimeLimitationWithWorkTimeLimitation(workTimeMinMaxCalculationResult, currentDayOccupation, currentDate);
				updatePreferenceTimeLimitationWithWorkTimeLimitation(workTimeMinMaxCalculationResult, nextDayOccupation, nextDate);
				
				checkRestViolationForPreviousDay(previousDayOccupation, currentDayOccupation, currentDate, checkResult);
				checkRestViolationForNextDay(currentDayOccupation, nextDayOccupation, currentDate, checkResult);

				result.Add(currentDate, checkResult);
			}
			return result;
		}

		private static void updatePreferenceTimeLimitationWithWorkTimeLimitation(
			IDictionary<DateOnly, WorkTimeMinMaxCalculationResult> workTimeMinMaxCalculationResult,
			PersonPreferenceDayOccupation personPreferenceDayOccupation, DateOnly date)
		{
			if (personPreferenceDayOccupation == null) return;
			if (workTimeMinMaxCalculationResult == null || !workTimeMinMaxCalculationResult.ContainsKey(date)) return;

			var workTimeMinMax = workTimeMinMaxCalculationResult[date]?.WorkTimeMinMax;
			if (workTimeMinMax == null) return;

			personPreferenceDayOccupation.StartTimeLimitation =
				workTimeMinMaxCalculationResult[date].WorkTimeMinMax.StartTimeLimitation;
			personPreferenceDayOccupation.EndTimeLimitation =
				workTimeMinMaxCalculationResult[date].WorkTimeMinMax.EndTimeLimitation;
		}


		private static void checkRestViolationForNextDay(PersonPreferenceDayOccupation currentDayOccupation,
			PersonPreferenceDayOccupation nextDayOccupation, DateOnly currentDate,
			PreferenceNightRestCheckResult checkResult)
		{
			var currentEndLimitStartTime = currentDayOccupation.EndTimeLimitation.StartTime;
			var nextDayStartLimitEndTime = nextDayOccupation.StartTimeLimitation.EndTime;
			if (!hasValidSchedule(nextDayOccupation) || !nextDayStartLimitEndTime.HasValue || !currentEndLimitStartTime.HasValue)
			{
				return;
			}

			var restTime = getRestTime(currentDate, currentEndLimitStartTime.Value, nextDayStartLimitEndTime.Value);
			checkResult.RestTimeToNextDay = restTime;
			checkResult.HasViolationToNextDay = restTime < checkResult.ExpectedNightRest;
		}

		private static void checkRestViolationForPreviousDay(PersonPreferenceDayOccupation previousDayOccupation,
			PersonPreferenceDayOccupation currentDayOccupation, DateOnly currentDate,
			PreferenceNightRestCheckResult checkResult)
		{
			var previousEndLimitStartTime = previousDayOccupation.EndTimeLimitation.StartTime;
			var currentStartLimitEndTime = currentDayOccupation.StartTimeLimitation.EndTime;
			if (!hasValidSchedule(previousDayOccupation) || !previousEndLimitStartTime.HasValue ||
				!currentStartLimitEndTime.HasValue)
			{
				return;
			}

			var restTime = getRestTime(currentDate.AddDays(-1), previousEndLimitStartTime.Value, currentStartLimitEndTime.Value);
			checkResult.RestTimeToPreviousDay = restTime;
			checkResult.HasViolationToPreviousDay = restTime < checkResult.ExpectedNightRest;
		}

		private static PreferenceNightRestCheckResult initCheckResult(IPersonPeriod period)
		{
			return new PreferenceNightRestCheckResult
			{
				ExpectedNightRest = period?.PersonContract.Contract.WorkTimeDirective.NightlyRest ?? TimeSpan.Zero
			};
		}

		private static TimeSpan getRestTime(DateOnly date, TimeSpan firstTimeSpan, TimeSpan secondTimeSpan)
		{
			var firstTimePoint = date.Date.Add(firstTimeSpan);
			var secondTimePoint = date.AddDays(1).Date.Add(secondTimeSpan);
			return secondTimePoint - firstTimePoint;
		}

		private static bool hasValidSchedule(PersonPreferenceDayOccupation occupation)
		{
			return (occupation.HasPreference || occupation.HasShift) && !(occupation.HasDayOff || occupation.HasFullDayAbsence);
		}
	}
}