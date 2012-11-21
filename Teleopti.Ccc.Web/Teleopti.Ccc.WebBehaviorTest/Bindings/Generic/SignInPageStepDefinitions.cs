using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class SignInPageStepDefinitions
	{
		private bool _newSignIn;

		[BeforeScenario("signinnew")]
		public void FlagForNewSignInPage()
		{
			_newSignIn = true;
		}

		[When(@"I sign in")]
		[When(@"I sign in by user name")]
		public void WhenISignIn()
		{
			//var userName = UserFactory.User().MakeUser();
			if (!(Browser.Current.Url.Contains("/SignIn") || Browser.Current.Url.Contains("/MobileSignIn")))
				Navigation.GotoGlobalSignInPage(_newSignIn);
			//Pages.Pages.CurrentSignInPage.SignInApplication(userName, TestData.CommonPassword);
			var userName = UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, TestData.CommonPassword);
		}

		[When(@"I sign in by windows credentials")]
		public void WhenISignInByWindowsAuthentication()
		{
			//UserFactory.User().MakeUser();
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

		[When(@"I select a business unit")]
		public void WhenISelectABusinessUnit()
		{
			Pages.Pages.CurrentSignInPage.SelectFirstBusinessUnit();
			Pages.Pages.CurrentSignInPage.ClickBusinessUnitOkButton();
		}

		[When(@"I select business unit '(.*)'")]
		public void WhenISelectBusinessUnit(string businessUnit)
		{
			Pages.Pages.CurrentSignInPage.SelectBusinessUnitByName(businessUnit);
		}


		//[When(@"I try to sign in by application logon")]
		//public void WhenITryToSignInByApplicationLogon()
		//{
		//    var userName = UserFactory.User().MakeUser();
		//    Pages.Pages.CurrentSignInNewPage.TrySignInApplication(userName, TestData.CommonPassword);
		//}

		//[When(@"I try to sign in by application logon with wrong password")]
		//public void WhenITryToSignInByApplicationLogonWithWrongPassword()
		//{
		//    var userName = UserFactory.User().MakeUser();
		//    Pages.Pages.CurrentSignInNewPage.TrySignInApplication(userName, "wrong password");
		//}

		//[When(@"I try to sign in by windows credentials")]
		//public void WhenITryToSignInByWindowsCredentials()
		//{
		//    UserFactory.User().UpdateWindowsUser();
		//    Pages.Pages.CurrentSignInNewPage.TrySignInWindows();
		//}

		//[When(@"I try to sign in with")]
		//public void WhenITryToSignInWith(Table table)
		//{
		//    var user = table.CreateInstance<UserConfigurable>();
		//    var userName = user.UserName;
		//    var password = user.Password;
		//    Navigation.GotoGlobalSignInPage();
		//    Pages.Pages.CurrentSignInPage.TrySignInApplication(userName, password);
		//}


		[When(@"I sign in again")]
		public void WhenISignInAgain()
		{
			Navigation.GotoGlobalSignInPage();
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
			EventualAssert.That(() => Browser.Current.Link("signout").Exists || Browser.Current.Link("signout-button").Exists, Is.True);
		}

		[Then(@"I should see a log on error")]
		public void ThenIShouldSeeAnLogOnError()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.ValidationSummary.Text, new StringContainsAnyLanguageResourceContraint("LogOnFailedInvalidUserNameOrPassword"));
		}

	}
}