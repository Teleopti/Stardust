using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Start
{
	[Binding]
	public class SignInPageStepDefinitions
	{
		[When(@"I sign in")]
		[When(@"I sign in by user name")]
		public void WhenISignIn()
		{
			if (!(Browser.Current.Url.Contains("/Authentication")))
				Navigation.GotoGlobalSignInPage();
			var userName = UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, TestData.CommonPassword);
		}

		[When(@"I sign in with")]
		public void WhenISignInWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			var userName = user.UserName;
			var password = user.Password;
			WhenISelectApplicationLogonDataSource();
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, password);
		}

		[When(@"I try to sign in with")]
		public void WhenITryToSignInWith(Table table)
		{
			Navigation.GotoGlobalSignInPage();
			WhenISignInWith(table);
		}

		[When(@"I sign in by windows credentials")]
		public void WhenISignInByWindowsAuthentication()
		{
			Pages.Pages.CurrentSignInPage.SignInWindows();
		}

		[When(@"I select application logon data source")]
		public void WhenISelectApplicationLogonDataSource()
		{
			Pages.Pages.CurrentSignInPage.SelectApplicationTestDataSource();
		}

		[When(@"I select windows logon data source")]
		public void WhenISelectWindowsLogonDataSource()
		{
			Pages.Pages.CurrentSignInPage.SelectWindowsTestDataSource();
		}

		[When(@"I sign in by user name and wrong password")]
		public void WhenISignInByUserNameAndWrongPassword()
		{
			var userName = UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, "wrong password");
		}

		[When(@"I select business unit '(.*)'")]
		public void WhenISelectBusinessUnit(string businessUnit)
		{
			Pages.Pages.CurrentSignInPage.SelectBusinessUnitByName(businessUnit);
		}

		[When(@"I sign in again")]
		public void WhenISignInAgain()
		{
			Navigation.GotoGlobalSignInPage();
			Pages.Pages.CurrentSignInPage.SelectApplicationTestDataSource();
			Pages.Pages.CurrentSignInPage.SignInApplication(UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName, TestData.CommonPassword);
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

		[Then(@"I should see a log on error")]
		public void ThenIShouldSeeAnLogOnError()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.ValidationSummary.Text, new StringContainsAnyLanguageResourceConstraint("LogOnFailedInvalidUserNameOrPassword"));
		}

		[Then(@"I should see a log on error '(.*)'")]
		public void ThenIShouldSeeALogOnError(string resourceText)
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.ValidationSummary.Text, new StringContainsAnyLanguageResourceConstraint(resourceText));
		}

		[Then(@"I should see the global sign in page")]
		public void ThenIShouldSeeTheGlobalSignInPage()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
			
			EventualAssert.That(() => Browser.Current.Url, Is.Not.StringContaining("/MyTime/Authentication"));
			EventualAssert.That(() => Browser.Current.Url, Is.Not.StringContaining("/MobileReports/Authentication"));
			
		}

		[Then(@"I should see MyTime's sign in page")]
		public void ThenIShouldSeeMyTimesSignInPage()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Url, Is.StringContaining("/MyTime/Authentication"));
		}

		[Then(@"I should see Mobile Report's sign in page")]
		public void ThenIShouldSeeMobileReportsSignInPage()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Url, Is.StringContaining("/MobileReports/Authentication"));
		}

		[Then(@"I should see the global menu")]
		public void ThenIShouldSeeTheGlobalMenu()
		{
			EventualAssert.That(() => Browser.Current.Url, Is.StringContaining("/Authentication#menu"));
		}


	}
}