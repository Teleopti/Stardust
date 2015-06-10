using System;
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
		private readonly Lazy<IMappingEngine> _mapper;

		public PreferenceDayFeedbackViewModelMappingProfile(IPreferenceFeedbackProvider preferenceFeedbackProvider, Lazy<IMappingEngine> mapper)
		{
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
			_mapper = mapper;
		}

		protected override void Configure()
		{
			CreateMap<DateOnly, PreferenceDayFeedbackViewModel>()
				.ConvertUsing(s =>
				{
					var nightRestResult = _preferenceFeedbackProvider.CheckNightRestViolation(s);
					var mappedResult = new PreferenceDayFeedbackViewModel
					{
						Date = s.ToFixedClientDateOnlyFormat(),

						HasViolationToPreviousDay = nightRestResult.HasViolationToPreviousDay,
						HasViolationToNextDay = nightRestResult.HasViolationToNextDay,
						ExpectedNightRest = nightRestResult.ExpectedNightRest
					};

					var workTimeResult = _preferenceFeedbackProvider.WorkTimeMinMaxForDate(s) ??
					                     new WorkTimeMinMaxCalculationResult();

					if (workTimeResult.WorkTimeMinMax == null)
					{
						mappedResult.FeedbackError = (workTimeResult.RestrictionNeverHadThePossibilityToMatchWithShifts)
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
							workTimeResult.WorkTimeMinMax.WorkTimeLimitation.StartTime != null
								? workTimeResult.WorkTimeMinMax.WorkTimeLimitation.StartTime.Value.TotalMinutes.ToString()
								: null;

						mappedResult.PossibleContractTimeMinutesUpper =
							workTimeResult.WorkTimeMinMax.WorkTimeLimitation.EndTime != null
								? workTimeResult.WorkTimeMinMax.WorkTimeLimitation.EndTime.Value.TotalMinutes.ToString()
								: null;
					}

					return mappedResult;

				});
		}
	}
}