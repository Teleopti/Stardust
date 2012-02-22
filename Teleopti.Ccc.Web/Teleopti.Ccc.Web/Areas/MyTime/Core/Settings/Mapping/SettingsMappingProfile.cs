using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.UserTexts;
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
				.ForMember(d => d.ChoosenCulture, o =>
				                                  	{
				                                  		o.MapFrom(s => s.PermissionInformation.Culture()); 
																	o.Condition(s => s.PermissionInformation.CultureLCID().HasValue); 
																})
				.ForMember(d => d.ChoosenUiCulture, o =>
				                                    	{
				                                    		o.MapFrom(s => s.PermissionInformation.UICulture());
																		o.Condition(s => s.PermissionInformation.UICultureLCID().HasValue);
				                                    	})
				.ForMember(d => d.Cultures, o => o.MapFrom(s => allCulturesSortedByNamePlusBrowserDefault()))
				.AfterMap((source, target) =>
				          	{
				          		var browserDefault = new CultureViewModel {LCID = -1, DisplayName = Resources.BrowserDefault};
									if(target.ChoosenCulture==null)
										target.ChoosenCulture = browserDefault;
									if (target.ChoosenUiCulture == null)
										target.ChoosenUiCulture = browserDefault;
									target.Cultures.Insert(0, browserDefault);
				          	});
		}

		private static CultureInfo[] allCulturesSortedByNamePlusBrowserDefault()
		{
			return CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(culture => culture.DisplayName).ToArray();
		}
	}
}