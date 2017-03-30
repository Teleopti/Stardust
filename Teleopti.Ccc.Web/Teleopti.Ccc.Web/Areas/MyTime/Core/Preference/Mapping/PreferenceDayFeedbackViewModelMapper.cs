using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayFeedbackViewModelMapper
	{
		private const string timeSpanFormat = @"hh\:mm";
		private readonly IPreferenceFeedbackProvider _preferenceFeedbackProvider;
		private readonly IToggleManager _toggleManager;

		public PreferenceDayFeedbackViewModelMapper(IPreferenceFeedbackProvider preferenceFeedbackProvider,
			IToggleManager toggleManager)
		{
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
			_toggleManager = toggleManager;
		}

		public PreferenceDayFeedbackViewModel Map(DateOnly date)
		{
			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_PreferencePerformanceForMultipleUsers_43322))
			{
				return Map(new DateOnlyPeriod(date, date)).FirstOrDefault();
			}

			var nightRestResult = _preferenceFeedbackProvider.CheckNightRestViolation(date);
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

			var workTimeResult = _preferenceFeedbackProvider.WorkTimeMinMaxForDate(date) ??
								 new WorkTimeMinMaxCalculationResult();

			var workTimeMinMax = workTimeResult.WorkTimeMinMax;
			if (workTimeMinMax != null)
			{
				var workTimeLimitation = workTimeMinMax.WorkTimeLimitation;
				var startTime = workTimeLimitation.StartTime;
				var endTime = workTimeLimitation.EndTime;
				mappedResult.PossibleContractTimeMinutesLower = startTime?.TotalMinutes.ToString(CultureInfo.CurrentCulture);
				mappedResult.PossibleContractTimeMinutesUpper = endTime?.TotalMinutes.ToString(CultureInfo.CurrentCulture);

				mappedResult.PossibleStartTimes = workTimeMinMax.StartTimeLimitation.ToString().ToLower();
				mappedResult.PossibleEndTimes = workTimeMinMax.EndTimeLimitation.ToString().ToLower();
			}
			else
			{
				mappedResult.FeedbackError = workTimeResult.RestrictionNeverHadThePossibilityToMatchWithShifts
					? ""
					: Resources.NoAvailableShifts;
			}

			return mappedResult;
		}

		public IEnumerable<PreferenceDayFeedbackViewModel> Map(DateOnlyPeriod period)
		{
			var result = new List<PreferenceDayFeedbackViewModel>();
			var nightResultResults = _preferenceFeedbackProvider.CheckNightRestViolation(period);
			var workTimeMinMaxCalculationResults = _preferenceFeedbackProvider.WorkTimeMinMaxForPeriod(period);

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