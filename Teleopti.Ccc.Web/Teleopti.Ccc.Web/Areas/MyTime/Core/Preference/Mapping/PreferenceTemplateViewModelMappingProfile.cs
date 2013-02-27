using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceTemplateViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>()
				.ForMember(d => d.Name, o => o.MapFrom(s => s.Name));
		}
	}
}