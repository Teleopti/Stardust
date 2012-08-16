using System.Web;
using AutoMapper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayViewModelMappingProfile : Profile
	{
		private readonly IResolve<IMappingEngine> _mapper;
		private readonly IResolve<IExtendedPreferencePredicate> _extendedPreferencePredicate;
		private readonly IResolve<IPreferenceProvider> _preferenceDayProvider;

		public PreferenceDayViewModelMappingProfile(IResolve<IMappingEngine> mapper, IResolve<IExtendedPreferencePredicate> extendedPreferencePredicate, IResolve<IPreferenceProvider> preferenceDayProvider)
		{
			_mapper = mapper;
			_extendedPreferencePredicate = extendedPreferencePredicate;
			_preferenceDayProvider = preferenceDayProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IPreferenceDay, PreferenceDayViewModel>()
				.ForMember(d => d.Preference, o => o.MapFrom(s =>
				                                                        	{
				                                                        		if (s.Restriction.DayOffTemplate != null)
				                                                        			return s.Restriction.DayOffTemplate.Description.Name;
				                                                        		if (s.Restriction.Absence != null)
				                                                        			return s.Restriction.Absence.Description.Name;
				                                                        		if (s.Restriction.ShiftCategory != null)
				                                                        			return s.Restriction.ShiftCategory.Description.Name;
																				if (_extendedPreferencePredicate.Invoke().IsExtended(s))
																					return Resources.Extended;
				                                                        		return null;
				                                                        	}))
				.ForMember(d => d.Color, o => o.MapFrom(s =>
				                                                 	{
				                                                 		if (s.Restriction.DayOffTemplate != null)
																			return s.Restriction.DayOffTemplate.DisplayColor.ToHtml();
				                                                 		if (s.Restriction.Absence != null)
																			return s.Restriction.Absence.DisplayColor.ToHtml();
				                                                 		if (s.Restriction.ShiftCategory != null)
																			return s.Restriction.ShiftCategory.DisplayColor.ToHtml();
				                                                 		return "";
				                                                 	}))
				.ForMember(d => d.Extended, o => o.MapFrom(s => _extendedPreferencePredicate.Invoke().IsExtended(s)))
				.ForMember(d => d.ExtendedTitle, o => o.Ignore())
				;

		}
	}
}