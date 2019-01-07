using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayFeedbackViewModelMapper
	{
		private const string timeSpanFormat = @"hh\:mm";
		private readonly PreferenceFeedbackProvider _preferenceFeedbackProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferenceDayFeedbackViewModelMapper(PreferenceFeedbackProvider preferenceFeedbackProvider, ILoggedOnUser loggedOnUser)
		{
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
			_loggedOnUser = loggedOnUser;
		}

		public PreferenceDayFeedbackViewModel Map(DateOnly date)
		{
			return Map(date.ToDateOnlyPeriod()).FirstOrDefault();
		}

		public IEnumerable<PreferenceDayFeedbackViewModel> Map(DateOnlyPeriod period)
		{
			var result = new List<PreferenceDayFeedbackViewModel>();

			var currentUser = _loggedOnUser.CurrentUser();
			var workTimeMinMaxCalculationResults = _preferenceFeedbackProvider.WorkTimeMinMaxForPeriod(currentUser, period);
			var nightResultResults = _preferenceFeedbackProvider.CheckNightRestViolation(currentUser, period, workTimeMinMaxCalculationResults);

			foreach (var date in period.DayCollection())
			{
				var nightRestResult = nightResultResults[date];

				var mappedResult = new PreferenceDayFeedbackViewModel
				{
					Date = date.ToFixedClientDateOnlyFormat(),
					DateInternal = date.Date,
					RestTimeToNextDayTimeSpan = nightRestResult.RestTimeToNextDay.ToString(timeSpanFormat),
					RestTimeToPreviousDayTimeSpan = nightRestResult.RestTimeToPreviousDay.ToString(timeSpanFormat),
					ExpectedNightRestTimeSpan = nightRestResult.ExpectedNightRest.ToString(timeSpanFormat),
					HasNightRestViolationToPreviousDay = nightRestResult.HasViolationToPreviousDay,
					HasNightRestViolationToNextDay = nightRestResult.HasViolationToNextDay
				};

				var workTimeMinMaxCalculationResult = workTimeMinMaxCalculationResults != null &&
												  workTimeMinMaxCalculationResults.ContainsKey(date) &&
												  workTimeMinMaxCalculationResults[date] != null
					? workTimeMinMaxCalculationResults[date]
					: new WorkTimeMinMaxCalculationResult();

				var workTimeMinMax = workTimeMinMaxCalculationResult.WorkTimeMinMax;
				if (workTimeMinMax != null)
				{
					var workTimelimitation = workTimeMinMax.WorkTimeLimitation;
					var startTime = workTimelimitation.StartTime;
					var endTime = workTimelimitation.EndTime;
					mappedResult.PossibleContractTimeMinutesLower = startTime?.TotalMinutes.ToString(CultureInfo.CurrentCulture);
					mappedResult.PossibleContractTimeMinutesUpper = endTime?.TotalMinutes.ToString(CultureInfo.CurrentCulture);

					mappedResult.PossibleStartTimes = workTimeMinMax.StartTimeLimitation.ToString().ToLower();
					mappedResult.PossibleEndTimes = workTimeMinMax.EndTimeLimitation.ToString().ToLower();
				}
				else
				{
					mappedResult.FeedbackError = workTimeMinMaxCalculationResult.RestrictionNeverHadThePossibilityToMatchWithShifts
						? ""
						: Resources.NoAvailableShifts;
				}

				result.Add(mappedResult);
			}


			return result;
		}
	}
}