using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class SignInStepDefinitions
	{
		[When(@"I select application logon data source")]
		public void WhenISelectApplicationLogonDataSource()
		{
			Pages.Pages.CurrentSignInNewPage.SelectTestDataApplicationLogon();
		}

	}
}