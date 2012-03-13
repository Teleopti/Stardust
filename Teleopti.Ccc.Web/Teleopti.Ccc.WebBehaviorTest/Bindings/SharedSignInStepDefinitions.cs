using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class SharedSignInStepDefinitions
	{
		private bool _hasPermission = true;
		private bool _singleBusinessUnit = false;

		[Given(@"I dont have permission to sign in")]
		public void GivenIDontHavePermissionToSignIn()
		{
			_hasPermission = false;
		}

		[When(@"I sign in by user name")]
		public void WhenISignInByApplicationAuthentication()
		{
			string userName;
			if (_hasPermission)
			{
				userName = _singleBusinessUnit
				           	? UserTestData.PersonApplicationUserSingleBusinessUnitUserName
				           	: UserTestData.PersonApplicationUserName;
			}
			else
			{
				userName = UserTestData.PersonWithNoPermissionUserName;
			}
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, TestData.CommonPassword);
		}

		[Given(@"I am a (?:mobileuser|user) with multiple business units")]
		public void GivenIAmAUserWithMultipleBusinessUnits()
		{
			_singleBusinessUnit = false;
			_hasPermission = true;
		}

		[Given(@"I am a mobileuser with a single business unit")]
		[Given(@"I am a user with a single business unit")]
		public void GivenIAmAUserWithASingleBusinessUnit()
		{
			_singleBusinessUnit = true;
			_hasPermission = true;
		}

		[When(@"I select a business unit")]
		public void WhenISelectABusinessUnit()
		{
			Pages.Pages.CurrentSignInPage.SelectFirstBusinessUnit();
			Pages.Pages.CurrentSignInPage.ClickBusinessUnitOkButton();
		}

		[Then(@"I should be signed in")]
		public void ThenIShouldBeSignedIn()
		{
			EventualAssert.That(() => Browser.Current.Link("signout").Exists || Browser.Current.Link("signout-button").Exists, Is.True);
		}

		[When(@"I sign in by user name and wrong password")]
		public void WhenISignInByUserNameAndWrongPassword()
		{
			Pages.Pages.CurrentSignInPage.SignInApplication(UserTestData.PersonApplicationUserSingleBusinessUnitUserName, "wrong password");
		}

		[Then(@"I should see an log on error")]
		public void ThenIShouldSeeAnLogOnError()
		{
			Browser.Current.ContainsText(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Then(@"I should not be signed in")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Browser.Current.Link("signout").Exists, Is.False);
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
		}

		[BeforeScenario("signin")]
		public void BeforeScenarioSignInWindows()
		{
			Navigation.GotoGlobalSignInPage();
		}

		[When(@"I sign in by Windows credentials")]
		public void WhenISignInByWindowsAuthentication()
		{
			if (_singleBusinessUnit)
			{
				ScenarioContext.Current.Pending();
				return;
			}
			Pages.Pages.CurrentSignInPage.SignInWindows();
		}

	}
}