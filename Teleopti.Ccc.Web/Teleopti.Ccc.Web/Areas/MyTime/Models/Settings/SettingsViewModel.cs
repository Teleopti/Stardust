﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Settings
{
	public class SettingsViewModel
	{
		public IList<CultureViewModel> Cultures { get; set; }
		public IEnumerable<NameFormatViewModel> NameFormats { get; set; }
		public NameFormatViewModel ChosenNameFormat { get; set; }
		public CultureViewModel ChoosenUiCulture { get; set; }
		public CultureViewModel ChoosenCulture { get; set; }
		public string MyTimeWebBaseUrl { get; set; }
	}

	public class CultureViewModel
	{
		public int id { get; set; }
		public string text { get; set; }
	}

	public class NameFormatViewModel
	{
		public int id { get; set; }
		public string text { get; set; }
	}
}