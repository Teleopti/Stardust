using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class RegionalSettingsPage : PortalPage
	{
		[FindBy(Id = "cultureSelect")]
		public SelectList CultureSelect;

		[FindBy(Id = "cultureUiSelect")]
		public SelectList CultureUiSelect;
	}
}