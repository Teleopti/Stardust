using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class SignInNewPage : Page, ISignInNewPage
	{
		[FindBy(Id = "DataSources")]
		public List DataSourcesList;

		public void SelectTestDataApplicationLogon()
		{
			DataSourcesList.WaitUntilExists();
			DataSourcesList.ListItem(Find.ByText("TestData")).EventualClick();
		}
	}
}