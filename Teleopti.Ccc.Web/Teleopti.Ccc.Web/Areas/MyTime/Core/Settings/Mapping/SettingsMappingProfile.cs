using System.Globalization;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping
{
	public class SettingsMappingProfile : Profile
	{
		protected override void Configure()
		{
			CreateMap<CultureInfo, CultureViewModel>();

			CreateMap<IPerson, SettingsViewModel>()
				.ForMember(d => d.ChoosenCulture, o => o.MapFrom(s => s.PermissionInformation.Culture()))
				.ForMember(d => d.ChoosenUiCulture, o => o.MapFrom(s => s.PermissionInformation.UICulture()))
				.ForMember(d => d.Cultures, o => o.MapFrom(s => CultureInfo.GetCultures(CultureTypes.AllCultures)));
		}
	}
}