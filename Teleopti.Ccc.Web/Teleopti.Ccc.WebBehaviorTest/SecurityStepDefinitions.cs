using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;

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

		[Given(@"I am signed in")]
		public void GivenIAmSignedIn()
		{
			Navigation.GotoGlobalSignInPage();
			var page = Browser.Current.Page<SignInPage>();
			page.SignInWindows();
			page.SelectFirstBusinessUnit();
			page.BusinessUnitOkButton.EventualClick();
			Resources.Culture = UserTestData.AgentWindowsUser.PermissionInformation.Culture();
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