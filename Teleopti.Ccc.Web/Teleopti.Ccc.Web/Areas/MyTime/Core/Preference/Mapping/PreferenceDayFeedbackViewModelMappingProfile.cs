using AutoMapper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayFeedbackViewModelMappingProfile : Profile
	{
		private readonly IPreferenceFeedbackProvider _preferenceFeedbackProvider;

		public PreferenceDayFeedbackViewModelMappingProfile(IPreferenceFeedbackProvider preferenceFeedbackProvider)
		{
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
		}

		protected override void Configure()
		{
			CreateMap<DateOnly, PreferenceDayFeedbackViewModel>()
				.ConvertUsing(s =>
				{
					var mappedResult = new PreferenceDayFeedbackViewModel
					{
						Date = s.ToFixedClientDateOnlyFormat(),
						DateInternal = s.Date,
					};

					var nightRestResult = _preferenceFeedbackProvider.CheckNightRestViolation(s);
					mappedResult.RestTimeToNextDay = nightRestResult.RestTimeToNextDay;
					mappedResult.RestTimeToPreviousDay = nightRestResult.RestTimeToPreviousDay;
					mappedResult.HasNightRestViolationToPreviousDay = nightRestResult.HasViolationToPreviousDay;
					mappedResult.HasNightRestViolationToNextDay = nightRestResult.HasViolationToNextDay;
					mappedResult.ExpectedNightRest = nightRestResult.ExpectedNightRest;

					var workTimeResult = _preferenceFeedbackProvider.WorkTimeMinMaxForDate(s) ??
										 new WorkTimeMinMaxCalculationResult();

					if (workTimeResult.WorkTimeMinMax == null)
					{
						mappedResult.FeedbackError = workTimeResult.RestrictionNeverHadThePossibilityToMatchWithShifts
							? ""
							: Resources.NoAvailableShifts;
					}
					else
					{
						mappedResult.PossibleStartTimes =
							workTimeResult.WorkTimeMinMax.StartTimeLimitation.StartTimeString.ToLower() + "-" +
							workTimeResult.WorkTimeMinMax.StartTimeLimitation.EndTimeString.ToLower();

						mappedResult.PossibleEndTimes =
							workTimeResult.WorkTimeMinMax.EndTimeLimitation.StartTimeString.ToLower() + "-" +
							workTimeResult.WorkTimeMinMax.EndTimeLimitation.EndTimeString.ToLower();

						mappedResult.PossibleContractTimeMinutesLower =
							workTimeResult.WorkTimeMinMax.WorkTimeLimitation.StartTime?.TotalMinutes.ToString();

						mappedResult.PossibleContractTimeMinutesUpper =
							workTimeResult.WorkTimeMinMax.WorkTimeLimitation.EndTime?.TotalMinutes.ToString();
					}

					return mappedResult;
				});
		}
	}
}