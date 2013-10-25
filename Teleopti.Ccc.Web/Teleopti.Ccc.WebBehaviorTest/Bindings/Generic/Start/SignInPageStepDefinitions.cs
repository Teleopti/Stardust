using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
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
			Browser.Interactions.ClickContaining("#DataSources .application a", "TestData");
		}

		public void SelectWindowsTestDataSource()
		{
			Browser.Interactions.ClickContaining("#DataSources .windows a", "TestData");
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
			var userName = DataMaker.Data().MePerson.ApplicationAuthenticationInfo.ApplicationLogOnName;
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
			Browser.Interactions.Click("#Login-button");
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
			var userName = DataMaker.Data().MePerson.ApplicationAuthenticationInfo.ApplicationLogOnName;
			SignInApplication(userName, "wrong password");
		}

		[When(@"I select business unit '(.*)'")]
		public void WhenISelectBusinessUnit(string businessUnit)
		{
			Browser.Interactions.ClickContaining("li a", businessUnit);
		}

		[When(@"I sign in again")]
		public void WhenISignInAgain()
		{
			Navigation.GotoGlobalSignInPage();
			SelectApplicationTestDataSource();
			SignInApplication(DataMaker.Data().MePerson.ApplicationAuthenticationInfo.ApplicationLogOnName, TestData.CommonPassword);
		}

		[Given(@"I am signed in")]
		public void IAmSignedIn()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent());
			TestControllerMethods.Logon();
		}

		[Then(@"I should (be|stay) signed in")]
		public void ThenIShouldBeSignedIn(string beOrStay)
		{
			Browser.Interactions.AssertExists("#signout, #signout-button");
		}

		[Then(@"I should see a log on error '(.*)'")]
		public void ThenIShouldSeeALogOnError(string resourceText)
		{
			Browser.Interactions.AssertFirstContainsResourceTextUsingJQuery("#Signin-error", resourceText);
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
			var userName = DataMaker.Data().MePerson.ApplicationAuthenticationInfo.ApplicationLogOnName;
			SelectApplicationTestDataSource();
			SignInApplication(userName, newPassword);
		}






		[When(@"I click skip button")]
		public void WhenIClickSkipButton()
		{
			Browser.Interactions.Click("#Skip-button");
		}

		[Then(@"I should not see skip button")]
		public void ThenIShouldNotSeeSkipButton()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Skip-button");
		}
		
		[When(@"I change my password with")]
		public void WhenIChangeMyPasswordWith(Table table)
		{
			var password = table.CreateInstance<PasswordConfigurable>();
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#New-password", password.Password);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Confirm-new-password", password.ConfirmedPassword);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Old-password", password.OldPassword);
			Browser.Interactions.Click("#Change-password-button");
		}

		[Then(@"I should see change password page with warning '(.*)'")]
		public void ThenIShouldSeeChangePasswordPageWithWarning(string resourceText)
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#PasswordExpireSoon");
			Browser.Interactions.AssertFirstContainsResourceTextUsingJQuery("#PasswordExpireSoon", resourceText);
		}

		[Then(@"I should see must change password page with warning '(.*)'")]
		public void ThenIShouldSeeMustChangePasswordPageWithWarning(string resourceText)
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#PasswordAlreadyExpired");
			Browser.Interactions.AssertFirstContainsResourceTextUsingJQuery("#PasswordAlreadyExpired", resourceText);
		}

		[Then(@"I should see an error '(.*)'")]
		public void ThenIShouldSeeAnError(string resourceText)
		{
			Browser.Interactions.AssertFirstContainsResourceTextUsingJQuery("#Password-change-error", resourceText);
		}
	}
}