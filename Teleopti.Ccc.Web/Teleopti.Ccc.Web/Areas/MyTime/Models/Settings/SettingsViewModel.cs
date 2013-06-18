﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Settings
{
	public class SettingsViewModel
	{
		public IList<CultureViewModel> Cultures { get; set; }
		public CultureViewModel ChoosenUiCulture { get; set; }
		public CultureViewModel ChoosenCulture { get; set; }
	}

	public class CultureViewModel
	{
		public int id { get; set; }
		public string text { get; set; }
	}
}