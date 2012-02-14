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
			Resources.Culture = UserTestData.PersonWindowsUser.PermissionInformation.Culture();
		}

		[When(@"I browse to an application page")]
		public void WhenIBrowseToAnApplicationPage()
		{
			Navigation.GotoAnApplicationPage();
		}

		[When(@"I browse to the site home page")]
		public void WhenIBrowseToTheSiteHomePage()
		{
			Navigation.GotoSiteHomePage();
		}

		[Then(@"I should see an application page")]
		public void ThenIShouldSeeAnApplicationPage()
		{
			Assert.That(() => Browser.Current.Link("signout").Exists, Is.True.After(5000, 10));
		}

		[Then(@"I should be redirected to the sign in page")]
		public void ThenIShouldBeRedirectedToTheSignInPage()
		{
			Assert.That(() => Browser.Current.Url, Is.StringContaining("Authentication/SignIn").After(5000, 10));
		}

	}
}