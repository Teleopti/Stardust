namespace Teleopti.Ccc.WebBehaviorTest
{
	using System.Linq;

	using NUnit.Framework;

	using TechTalk.SpecFlow;

	using Teleopti.Ccc.WebBehaviorTest.Core;
	using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
	using Teleopti.Ccc.WebBehaviorTest.Pages;

	[Binding]
	public class MobileSignInStepDefinitions
	{
		[BeforeScenario("mobilesignin")]
		public void BeforeScenarioSignInWindows()
		{
			var strings = ScenarioContext.Current.ScenarioInfo.Tags;
			if (strings != null && strings.Contains("IgnoreBeforeScenario"))
			{
				return;
			}
			Navigation.GotoMobileReportsSignInPage(string.Empty);
		}

		[Given(@"I Navigate to the signin page with subpage preference")]
		public void GivenINavigateToTheSigninPageWithSubpagePreference()
		{
			Navigation.GotoMobileReportsSignInPage("#wanyprefs");
		}

		[Then(@"I Should se the login page")]
		public void ThenIShouldSeTheLoginPage()
		{
			var page = Pages.Pages.Current as MobileSignInPage;
			EventualAssert.That(() => page.ApplicationSignIn.DisplayVisible(), Is.True, "Signin page should be visible");
		}
	}
}