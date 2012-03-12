using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class SharedSignInStepDefinitions
	{
		private static SignInPageBase Page
		{
			get { return (Pages.Pages.Current as SignInPageBase); }
		}

		[Given(@"I dont have permission to sign in")]
		public void GivenIDontHavePermissionToSignIn()
		{
			Page.HasPermission = false;
		}

		[When(@"I sign in by user name")]
		public void WhenISignInByApplicationAuthentication()
		{
			string userName;
			if (Page.HasPermission)
			{
				userName = Page.SingleBusinessUnit
				           	? UserTestData.PersonApplicationUserSingleBusinessUnitUserName
				           	: UserTestData.PersonApplicationUserName;
			}
			else
			{
				userName = UserTestData.PersonWithNoPermissionUserName;
			}
			Page.SignInApplication(userName, TestData.CommonPassword);
		}

		[Given(@"I am a (?:mobileuser|user) with multiple business units")]
		public void GivenIAmAUserWithMultipleBusinessUnits()
		{
			Page.SingleBusinessUnit = false;
			Page.HasPermission = true;
		}

		[Given(@"I am a mobileuser with a single business unit")]
		[Given(@"I am a user with a single business unit")]
		public void GivenIAmAUserWithASingleBusinessUnit()
		{
			Page.SingleBusinessUnit = true;
			Page.HasPermission = true;
		}

		[When(@"I select a business unit")]
		public void WhenISelectABusinessUnit()
		{
			Page.SelectFirstBusinessUnit();
			Page.ClickBusinessUnitOkButton();
		}

		[Then(@"I should be signed in")]
		public void ThenIShouldBeSignedIn()
		{
			EventualAssert.That(() => Browser.Current.Link("signout").Exists || Browser.Current.Link("signout-button").Exists, Is.True);
		}

		[When(@"I sign in by user name and wrong password")]
		public void WhenISignInByUserNameAndWrongPassword()
		{
			Page.SignInApplication(UserTestData.PersonApplicationUserSingleBusinessUnitUserName, "wrong password");
		}

		[Then(@"I should see an log on error")]
		public void ThenIShouldSeeAnLogOnError()
		{
			Page.Document.ContainsText(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Then(@"I should not be signed in")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Browser.Current.Link("signout").Exists, Is.False);
		}
	}
}