﻿using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping
{
	public class SettingsMappingProfile : Profile
	{
		private readonly Func<IPermissionProvider> _permissionProvider;


		public SettingsMappingProfile(Func<IPermissionProvider> permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		protected override void Configure()
		{

			CreateMap<CultureInfo, CultureViewModel>()
				.ForMember(d => d.id, o => o.MapFrom(s => s.LCID))
			.ForMember(d => d.text, o => o.MapFrom(s => s.DisplayName));


			CreateMap<CultureInfo, CultureViewModel>()
				.ForMember(d => d.id, o => o.MapFrom(s => s.LCID))
				.ForMember(d => d.text, o => o.MapFrom(s => s.DisplayName));

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
				.ForMember(d => d.SettingsPermission, o => o.ResolveUsing(s =>
				{
					var permission = new SettingsPermissionViewModel
					{
						ShareCalendarPermission = 
							_permissionProvider.Invoke().HasApplicationFunctionPermission(
							DefinedRaptorApplicationFunctionPaths.ShareCalendar)
					};
					return permission;
				}))
				.AfterMap((source, target) =>
				          	{
				          		var browserDefault = new CultureViewModel {id = -1, text = Resources.BrowserDefault};
									if(target.ChoosenCulture==null)
										target.ChoosenCulture = browserDefault;
									if (target.ChoosenUiCulture == null)
										target.ChoosenUiCulture = browserDefault;
									target.Cultures.Insert(0, browserDefault);
				          	});
		}

		private static CultureInfo[] allCulturesSortedByNamePlusBrowserDefault()
		{
			return CultureInfo.GetCultures(CultureTypes.SpecificCultures).OrderBy(culture => culture.DisplayName).ToArray();
		}
	}
}