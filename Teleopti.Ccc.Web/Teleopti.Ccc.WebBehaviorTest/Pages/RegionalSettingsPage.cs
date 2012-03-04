using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class RegionalSettingsPage : PortalPage
	{
		[FindBy(Id = "cultureSelect-container")]
		public SelectBox CultureSelect;

		[FindBy(Id = "cultureUiSelect-container")]
		public SelectBox CultureUiSelect;
	}
}