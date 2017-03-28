using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayFeedbackViewModelMapper
	{
		private const string timeSpanFormat = @"hh\:mm\:ss";
		private readonly IPreferenceFeedbackProvider _preferenceFeedbackProvider;

		public PreferenceDayFeedbackViewModelMapper(IPreferenceFeedbackProvider preferenceFeedbackProvider)
		{
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
		}

		public PreferenceDayFeedbackViewModel Map(DateOnly date)
		{
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

			var workTimeResult = _preferenceFeedbackProvider.WorkTimeMinMaxForDate(date) ?? new WorkTimeMinMaxCalculationResult();

			var workTimeMinMax = workTimeResult.WorkTimeMinMax;
			if (workTimeMinMax != null)
			{
				mappedResult.PossibleStartTimes = workTimeMinMax.StartTimeLimitation.ToString().ToLower();
				mappedResult.PossibleEndTimes = workTimeMinMax.EndTimeLimitation.ToString().ToLower();

				var workTimeLimitation = workTimeMinMax.WorkTimeLimitation;
				mappedResult.PossibleContractTimeMinutesLower =
					workTimeLimitation.StartTime?.TotalMinutes.ToString(CultureInfo.CurrentCulture);

				mappedResult.PossibleContractTimeMinutesUpper =
					workTimeLimitation.EndTime?.TotalMinutes.ToString(CultureInfo.CurrentCulture);
			}
			else
			{
				mappedResult.FeedbackError = workTimeResult.RestrictionNeverHadThePossibilityToMatchWithShifts
					? ""
					: Resources.NoAvailableShifts;
			}

			return mappedResult;
		}
	}
}