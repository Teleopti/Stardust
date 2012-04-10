using AutoMapper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayFeedbackViewModelMappingProfile : Profile
	{
		private readonly IResolve<IPreferenceFeedbackProvider> _preferenceFeedbackProvider;

		public PreferenceDayFeedbackViewModelMappingProfile(IResolve<IPreferenceFeedbackProvider> preferenceFeedbackProvider) {
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
		}

		protected override void Configure()
		{
			CreateMap<DateOnly, PreferenceDayFeedbackViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.FeedbackError, o => o.Ignore())
				.ForMember(d => d.PossibleStartTimes, o => o.Ignore())
				.ForMember(d => d.PossibleEndTimes, o => o.Ignore())
				.ForMember(d => d.PossibleContractTimes, o => o.Ignore())
				.AfterMap((s, d) =>
				          	{
				          		var minMaxWorkTime = _preferenceFeedbackProvider.Invoke().WorkTimeMinMaxForDate(s);
				          		if (minMaxWorkTime == null)
				          		{
				          			d.FeedbackError = Resources.NoAvailableShifts;
				          			return;
				          		}
				          		d.PossibleStartTimes = minMaxWorkTime.StartTimeLimitation.StartTimeString + "-" + minMaxWorkTime.StartTimeLimitation.EndTimeString;
				          		d.PossibleEndTimes = minMaxWorkTime.EndTimeLimitation.StartTimeString + "-" + minMaxWorkTime.EndTimeLimitation.EndTimeString;
				          		d.PossibleContractTimes = minMaxWorkTime.WorkTimeLimitation.StartTimeString + "-" + minMaxWorkTime.WorkTimeLimitation.EndTimeString;
				          	})
				;
		}
	}
}