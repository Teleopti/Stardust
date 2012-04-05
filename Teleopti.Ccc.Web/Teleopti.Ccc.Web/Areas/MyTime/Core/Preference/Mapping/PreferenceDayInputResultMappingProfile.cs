using AutoMapper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayInputResultMappingProfile : Profile
	{
		private readonly IResolve<IPreferenceFeedbackProvider> _preferenceFeedbackProvider;

		public PreferenceDayInputResultMappingProfile(IResolve<IPreferenceFeedbackProvider> preferenceFeedbackProvider)
		{
			_preferenceFeedbackProvider = preferenceFeedbackProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IPreferenceDay, PreferenceDayInputResult>()
				.ConstructUsing(s => new PreferenceDayInputResult())
				.ForMember(d => d.Date, o => o.MapFrom(s => s.RestrictionDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.PreferenceRestriction, o => o.MapFrom(s =>
				                                                        	{
				                                                        		if (s.Restriction.DayOffTemplate != null)
				                                                        			return s.Restriction.DayOffTemplate.Description.Name;
				                                                        		if (s.Restriction.Absence != null)
				                                                        			return s.Restriction.Absence.Description.Name;
				                                                        		if (s.Restriction.ShiftCategory != null)
				                                                        			return s.Restriction.ShiftCategory.Description.Name;
				                                                        		return null;
				                                                        	}))
				.ForMember(d => d.StyleClassName, o => o.MapFrom(s =>
				                                                 	{
				                                                 		if (s.Restriction.DayOffTemplate != null)
				                                                 			return s.Restriction.DayOffTemplate.DisplayColor.ToStyleClass();
				                                                 		if (s.Restriction.Absence != null)
				                                                 			return s.Restriction.Absence.DisplayColor.ToStyleClass();
				                                                 		if (s.Restriction.ShiftCategory != null)
				                                                 			return s.Restriction.ShiftCategory.DisplayColor.ToStyleClass();
				                                                 		return null;
				                                                 	}))
				.ForMember(d => d.PossibleStartTimes, o => o.Ignore())
				.ForMember(d => d.PossibleEndTimes, o => o.Ignore())
				.ForMember(d => d.PossibleContractTimes, o => o.Ignore())
				.ForMember(d => d.FeedbackError, o => o.Ignore())
				.AfterMap((s, d) =>
				          	{
				          		var workTimeMinMax = _preferenceFeedbackProvider.Invoke().WorkTimeMinMaxForDate(s.RestrictionDate);
								if (workTimeMinMax == null)
								{
									d.FeedbackError = Resources.NoAvailableShifts;
									return;
								}
				          		d.PossibleStartTimes = workTimeMinMax.StartTimeLimitation.StartTimeString + "-" + workTimeMinMax.StartTimeLimitation.EndTimeString;
								d.PossibleEndTimes = workTimeMinMax.EndTimeLimitation.StartTimeString + "-" + workTimeMinMax.EndTimeLimitation.EndTimeString;
								d.PossibleContractTimes = workTimeMinMax.WorkTimeLimitation.StartTimeString + "-" + workTimeMinMax.WorkTimeLimitation.EndTimeString;
				          	})
				;
		}
	}
}