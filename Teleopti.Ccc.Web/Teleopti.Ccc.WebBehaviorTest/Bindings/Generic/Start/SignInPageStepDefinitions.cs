using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Start
{
	[Binding]
	public class SignInPageStepDefinitions
	{

		public void SelectApplicationTestDataSource()
		{
			Browser.Interactions.Click("#DataSources .application a:contains(TestData)");
		}

		public void SelectWindowsTestDataSource()
		{
			Browser.Interactions.Click("#DataSources .windows a:contains(TestData)");
		}

		private void SignInApplication(string username, string password)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Username-input", username);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Password-input", password);
			Browser.Interactions.Click("#Login-button");
		}

		[When(@"I sign in")]
		[When(@"I sign in by user name")]
		public void WhenISignIn()
		{
			if (!(Browser.Current.Url.Contains("/Authentication")))
				Navigation.GotoGlobalSignInPage();
			var userName = UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
			SignInApplication(userName, TestData.CommonPassword);
		}

		[When(@"I try to sign in with")]
		public void WhenITryToSignInWith(Table table)
		{
			Navigation.GotoGlobalSignInPage();
			var user = table.CreateInstance<UserConfigurable>();
			var userName = user.UserName;
			var password = user.Password;
			WhenISelectApplicationLogonDataSource();
			SignInApplication(userName, password);
		}

		[When(@"I sign in by windows credentials")]
		public void WhenISignInByWindowsAuthentication()
		{
			Pages.Pages.SignInPage.SignInWindows();
		}

		[When(@"I select application logon data source")]
		public void WhenISelectApplicationLogonDataSource()
		{
			SelectApplicationTestDataSource();
		}

		[When(@"I select windows logon data source")]
		public void WhenISelectWindowsLogonDataSource()
		{
			SelectWindowsTestDataSource();
		}

		[When(@"I sign in by user name and wrong password")]
		public void WhenISignInByUserNameAndWrongPassword()
		{
			var userName = UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
			SignInApplication(userName, "wrong password");
		}

		[When(@"I select business unit '(.*)'")]
		public void WhenISelectBusinessUnit(string businessUnit)
		{
			Pages.Pages.SignInPage.SelectBusinessUnitByName(businessUnit);
		}

		[When(@"I sign in again")]
		public void WhenISignInAgain()
		{
			Navigation.GotoGlobalSignInPage();
			SelectApplicationTestDataSource();
			SignInApplication(UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName, TestData.CommonPassword);
		}

		[Given(@"I am signed in")]
		public void IAmSignedIn()
		{
			if (!UserFactory.User().HasSetup<IUserRoleSetup>())
				UserFactory.User().Setup(new Agent());
			TestControllerMethods.Logon();
		}

		[Then(@"I should be signed in")]
		public void ThenIShouldBeSignedIn()
		{
			Browser.Interactions.AssertExists("#signout, #signout-button");
		}

		[Then(@"I should see a log on error '(.*)'")]
		public void ThenIShouldSeeALogOnError(string resourceText)
		{
			EventualAssert.That(() => Pages.Pages.SignInPage.ValidationSummary.Text, new StringContainsAnyLanguageResourceConstraint(resourceText));
		}

		[Then(@"I should see the sign in page")]
		[Then(@"I should not be signed in")]
		[Then(@"I should be signed out")]
		[Then(@"I should be signed out from MobileReports")]
		public void ThenIAmNotSignedIn()
		{
			Browser.Interactions.AssertExists("#Username-input");
		}

		[Then(@"I should see the global sign in page")]
		public void ThenIShouldSeeTheGlobalSignInPage()
		{
			Browser.Interactions.AssertExists("#Username-input");
			Browser.Interactions.AssertUrlNotContains("/Authentication", "/MyTime");
			Browser.Interactions.AssertUrlNotContains("/Authentication", "/MobileReports");
		}

		[Then(@"I should see MyTime's sign in page")]
		public void ThenIShouldSeeMyTimesSignInPage()
		{
			Browser.Interactions.AssertExists("#Username-input");
			Browser.Interactions.AssertUrlContains("/MyTime/Authentication");
		}

		[Then(@"I should see Mobile Report's sign in page")]
		public void ThenIShouldSeeMobileReportsSignInPage()
		{
			Browser.Interactions.AssertExists("#Username-input");
			Browser.Interactions.AssertUrlContains("/MobileReports/Authentication");
		}

		[Then(@"I should see the global menu")]
		public void ThenIShouldSeeTheGlobalMenu()
		{
			Browser.Interactions.AssertUrlContains("/Authentication#menu");
		}

		[When(@"I sign in using my new password '(.*)'")]
		public void WhenISignInUsingMyNewPassword(string newPassword)
		{
			var userName = UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
			SelectApplicationTestDataSource();
			SignInApplication(userName, newPassword);
		}

	}
}