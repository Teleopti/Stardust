using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping
{
	public class SettingsMapper
	{
		public CultureViewModel Map(CultureInfo s)
		{
			return new CultureViewModel
			{
				id = s.LCID,
				text = s.DisplayName
			};
		}

		public SettingsViewModel Map(IPerson s)
		{
			var map = new SettingsViewModel
			{
				ChoosenCulture = s.PermissionInformation.CultureLCID().HasValue ? Map(s.PermissionInformation.Culture()) : null,
				ChoosenUiCulture =
					s.PermissionInformation.UICultureLCID().HasValue ? Map(s.PermissionInformation.UICulture()) : null,
				Cultures = allCulturesSortedByNamePlusBrowserDefault().Select(Map).ToList()
			};

			var browserDefault = new CultureViewModel { id = -1, text = Resources.BrowserDefault };
			if (map.ChoosenCulture == null)
				map.ChoosenCulture = browserDefault;
			if (map.ChoosenUiCulture == null)
				map.ChoosenUiCulture = browserDefault;
			map.Cultures.Insert(0, browserDefault);

			return map;
		}
		
		private static CultureInfo[] allCulturesSortedByNamePlusBrowserDefault()
		{
			CultureInfo[] cInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
			var validCultures = new List<CultureInfo>();
			for (int i = 0; i < cInfo.Length - 1; i++)
			{
				try
				{
					CultureInfo.GetCultureInfo(cInfo[i].LCID);
				}
				catch (Exception)
				{
					continue;
				}
				validCultures.Add(cInfo[i]);
			}
			return validCultures.OrderBy(culture => culture.DisplayName).ToArray();
		}
	}
}