using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class MobileSignInStepDefinitions
	{
		[Given(@"I navigate to the mobile signin page with subpage preference")]
		public void GivenINavigateToTheSigninPageWithSubpagePreference()
		{
			Navigation.GotoMobileReportsSignInPage("#wanyprefs");
		}

		[Then(@"I should se the login page")]
		public void ThenIShouldSeTheLoginPage()
		{
			var page = Pages.Pages.Current as MobileSignInPage;
			EventualAssert.That(() => page.ApplicationSignIn.DisplayVisible(), Is.True, "Signin page should be visible");
		}
	}
}