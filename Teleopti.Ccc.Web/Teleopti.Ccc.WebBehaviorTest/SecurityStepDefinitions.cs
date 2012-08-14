using NUnit.Framework;
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
			EventualAssert.That(() => Pages.Pages.CurrentPortalPage.SignOutLink.Exists, Is.True);
		}

		[Then(@"I should be redirected to the sign in page")]
		public void ThenIShouldBeRedirectedToTheSignInPage()
		{
			EventualAssert.That(() => Browser.Current.Url, Is.StringContaining("Authentication/SignIn"));
		}

	}
}