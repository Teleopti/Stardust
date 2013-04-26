using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class SecurityStepDefinitions
	{
		[Given(@"I am not signed in")]
		public void GivenIAmNotSignedIn()
		{
			Browser.Current.ClearCookies();
		}

		[Then(@"I should see an application page")]
		public void ThenIShouldSeeAnApplicationPage()
		{
			Browser.Interactions.AssertExists("#signout, #signout-button");
		}

		[Then(@"I should be redirected to the sign in page")]
		public void ThenIShouldBeRedirectedToTheSignInPage()
		{
			Browser.Interactions.AssertExists("#Username-input");
		}

	}
}