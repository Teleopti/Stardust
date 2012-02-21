namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Settings
{
	public class SettingsViewModel
	{
		public CultureViewModel[] Cultures { get; set; }
		public CultureViewModel ChoosenUiCulture { get; set; }
		public CultureViewModel ChoosenCulture { get; set; }
	}

	public class CultureViewModel
	{
		public int LCID { get; set; }
		public string DisplayName { get; set; }
	}
}