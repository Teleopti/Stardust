using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayInputResultMappingProfile : Profile
	{
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
				.ForMember(d => d.HexColor, o => o.MapFrom(s =>
				                                                 	{
				                                                 		if (s.Restriction.DayOffTemplate != null)
																			return s.Restriction.DayOffTemplate.DisplayColor.ToHtml();
				                                                 		if (s.Restriction.Absence != null)
																			return s.Restriction.Absence.DisplayColor.ToHtml();
				                                                 		if (s.Restriction.ShiftCategory != null)
																			return s.Restriction.ShiftCategory.DisplayColor.ToHtml();
				                                                 		return "";
				                                                 	}))
				;
		}
	}
}