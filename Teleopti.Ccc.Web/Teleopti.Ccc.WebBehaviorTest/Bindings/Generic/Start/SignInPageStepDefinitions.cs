using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.MyTime;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Start
{
	[Binding]
	public class SignInPageStepDefinitions
	{

		public void SelectTestDataSource()
		{
			Browser.Interactions.ClickContaining("#DataSources a", "TestData");
		}
		
		private void SignInApplication(string username, string password)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Username-input", username);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Password-input", password);
			Browser.Interactions.Click("#Login-button");
		}

		[Given(@"I sign in")]
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

		[When(@"I try to signin with")]
		public void WhenITryToSigninWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			var userName = user.UserName;
			var password = user.Password;
			SignInApplication(userName, password);
		}


		[Given(@"I select application logon data source")]
		[When(@"I select application logon data source")]
		public void WhenISelectApplicationLogonDataSource()
		{
			SelectTestDataSource();
		}

		[When(@"I select windows logon data source")]
		public void WhenISelectWindowsLogonDataSource()
		{
			SelectTestDataSource();
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
			Browser.Interactions.ClickContaining("div a", businessUnit);
		}

		[When(@"I sign in again")]
		public void WhenISignInAgain()
		{
			Navigation.GotoGlobalSignInPage();
			SelectTestDataSource();
			SignInApplication(DataMaker.Data().MePerson.ApplicationAuthenticationInfo.ApplicationLogOnName, TestData.CommonPassword);
		}

		[Given(@"I am signed in")]
		public void IAmSignedIn()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent());
			TestControllerMethods.Logon();
		}

		[Given(@"I am not signed in")]
		public void GivenIAmNotSignedIn()
		{
			// really make sure we'r not signed in by setting an incorrect cookie through the TestController. if required.
		}

		[Then(@"I should (be|stay) signed in")]
		public void ThenIShouldBeSignedIn(string beOrStay)
		{
			Browser.Interactions.AssertExists("#regional-settings");
		}

		[Then(@"I should be signed in for Anywhere")]
		public void ThenIShouldBeSignedInForAnywhere()
		{
			Browser.Interactions.AssertExists(".user-name");
		}

		[Then(@"I should be signed in as another user '(.*)'")]
		public void ThenIShouldBeSignedInAsAnotherUser(string name)
		{
			Browser.Interactions.AssertExists("#regional-settings");
			Browser.Interactions.AssertAnyContains(".user-name-link .user-name", name);
		}


		[Then(@"I should see a log on error '(.*)'")]
		public void ThenIShouldSeeALogOnError(string resourceText)
		{
			Browser.Interactions.AssertFirstContainsResourceTextUsingJQuery("#Signin-error", resourceText);
		}

		[Then(@"I should see the sign in page")]
		[Then(@"I should be redirected to the sign in page")]
		[Then(@"I should not be signed in")]
		public void ThenIAmNotSignedIn()
		{
			Browser.Interactions.AssertExists("#Username-input");
		}

		[Then(@"I should see the global sign in page")]
		public void ThenIShouldSeeTheGlobalSignInPage()
		{
			Browser.Interactions.AssertExists("#Username-input");
			Browser.Interactions.AssertUrlContains("/SSO/OpenId/Provider");
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
			SelectTestDataSource();
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
			var password = table.CreateInstance<MyProfileStepDefinitions.PasswordInfo>();
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