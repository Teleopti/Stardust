using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.MyTime;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Start
{
	[Binding]
	public class SignInPageStepDefinitions
	{
		private void SignInApplication(string username, string password)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Username-input", username);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Password-input", password);
			Browser.Interactions.Click("#Signin-button");
		}

		[Given(@"I sign in")]
		[When(@"I sign in")]
		[When(@"I sign in by user name")]
		public void WhenISignIn()
		{
			var userName = DataMaker.Me().LogOnName;
			SignInApplication(userName, DefaultPassword.ThePassword);
		}

		[When(@"I try to sign in with")]
		public void WhenITryToSignInWith(Table table)
		{
			Navigation.GotoGlobalSignInPage();
			var user = table.CreateInstance<PersonUserConfigurable>();
			var userName = user.UserName;
			var password = user.Password;
			SignInApplication(userName, password);
		}

		[When(@"I try to signin with")]
		public void WhenITryToSigninWith(Table table)
		{
			var user = table.CreateInstance<PersonUserConfigurable>();
			var userName = user.UserName;
			var password = user.Password;
			SignInApplication(userName, password);
		}

		[When(@"I sign in by user name and wrong password")]
		public void WhenISignInByUserNameAndWrongPassword()
		{
			var userName = DataMaker.Me().LogOnName;
			SignInApplication(userName, "wrong password");
		}

		[When(@"I sign in by username '(.*)' and password '(.*)'")]
		public void WhenISignInByUserNameAnPassword(string username, string password)
		{
			SignInApplication(username, password);
		}

		[When(@"I select business unit '(.*)'")]
		public void WhenISelectBusinessUnit(string businessUnit)
		{
			Browser.Interactions.ClickContaining("div a", businessUnit);
		}

		[Given(@"I am signed in")]
		public void IAmSignedIn()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent_ThingThatReallyAppliesSetupsInConstructor());
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
			Browser.Interactions.AssertExists("#regional-settings, .user-name");
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

		[When(@"I sign in using my new password '(.*)'")]
		public void WhenISignInUsingMyNewPassword(string newPassword)
		{
			var userName = DataMaker.Me().LogOnName;
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
			Browser.Interactions.AssertExistsUsingJQuery("#New-password:enabled");
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
		[When(@"I see must change password page with warning '(.*)'")]
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