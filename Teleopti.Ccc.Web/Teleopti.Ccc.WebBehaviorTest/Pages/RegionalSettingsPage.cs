using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class RegionalSettingsPage : PortalPage
	{
		[FindBy(Id = "Culture-Picker")]
		public Select2Box CultureSelect;

		[FindBy(Id = "CultureUi-Picker")]
		public Select2Box CultureUiSelect;
	}
}