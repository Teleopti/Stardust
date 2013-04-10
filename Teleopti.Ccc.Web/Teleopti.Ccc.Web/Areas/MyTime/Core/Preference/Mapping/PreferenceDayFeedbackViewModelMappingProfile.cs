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
				              		var result = _preferenceFeedbackProvider.WorkTimeMinMaxForDate(s) ?? new WorkTimeMinMaxCalculationResult();
				              		if (result.WorkTimeMinMax == null)
				              		{
				              			if (result.RestrictionNeverHadThePossibilityToMatchWithShifts)
											return _mapper.Value.Map<Tuple<DateOnly, string>, PreferenceDayFeedbackViewModel>(new Tuple<DateOnly, string>(s, ""));
										return _mapper.Value.Map<Tuple<DateOnly, string>, PreferenceDayFeedbackViewModel>(new Tuple<DateOnly, string>(s, Resources.NoAvailableShifts));
									}
				              		var source = new Tuple<DateOnly, IWorkTimeMinMax>(s, result.WorkTimeMinMax);
				              		return _mapper.Value.Map<Tuple<DateOnly, IWorkTimeMinMax>, PreferenceDayFeedbackViewModel>(source);
				              	});

			CreateMap<Tuple<DateOnly, string>, PreferenceDayFeedbackViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.Item1.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.FeedbackError, o => o.MapFrom(s => s.Item2))
				.ForMember(d => d.PossibleStartTimes, o => o.Ignore())
				.ForMember(d => d.PossibleEndTimes, o => o.Ignore())
				.ForMember(d => d.PossibleContractTimeMinutesLower, o => o.Ignore())
				.ForMember(d => d.PossibleContractTimeMinutesUpper, o => o.Ignore())
				;

			CreateMap<Tuple<DateOnly, IWorkTimeMinMax>, PreferenceDayFeedbackViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.Item1.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.FeedbackError, o => o.Ignore())
				.ForMember(d => d.PossibleStartTimes, o => o.MapFrom(s => s.Item2.StartTimeLimitation.StartTimeString.ToLower() + "-" + s.Item2.StartTimeLimitation.EndTimeString.ToLower()))
				.ForMember(d => d.PossibleEndTimes, o => o.MapFrom(s => s.Item2.EndTimeLimitation.StartTimeString.ToLower() + "-" + s.Item2.EndTimeLimitation.EndTimeString.ToLower()))
				.ForMember(d => d.PossibleContractTimeMinutesLower, o => o.MapFrom(s => s.Item2.WorkTimeLimitation.StartTime != null ? s.Item2.WorkTimeLimitation.StartTime.Value.TotalMinutes.ToString() : null))
				.ForMember(d => d.PossibleContractTimeMinutesUpper, o => o.MapFrom(s => s.Item2.WorkTimeLimitation.EndTime != null ? s.Item2.WorkTimeLimitation.EndTime.Value.TotalMinutes.ToString() : null))
				;

		}
	}
}