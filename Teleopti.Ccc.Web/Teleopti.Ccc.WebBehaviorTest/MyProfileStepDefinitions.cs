using System.Globalization;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class MyProfileStepDefinitions
	{
		private static readonly string newPassword = TestData.CommonPassword + "new";

		[When(@"I view my regional settings")]
		public void WhenIViewMyRegionalSettings()
		{
			var userName = UserFactory.User().MakeUser();
			Navigation.GotoGlobalSignInPage();
			var page = Browser.Current.Page<SignInPage>();
			page.SignInApplication(userName, TestData.CommonPassword);
			Navigation.GotoRegionalSettings();
		}

		[When(@"I view my password")]
		public void WhenIViewMyPassword()
		{
			var userName = UserFactory.User().MakeUser();
			Navigation.GotoGlobalSignInPage();
			var signInpage = Browser.Current.Page<SignInPage>();
			signInpage.SignInApplication(userName, TestData.CommonPassword);
			Navigation.GotoPasswordPage();
		}

		[When(@"I change my password")]
		public void WhenIChangeMyPassword()
		{
			var page = Browser.Current.Page<PasswordPage>();
			page.Password.Value = newPassword;
			page.PasswordValidation.Value = newPassword;
			page.OldPassword.Value = TestData.CommonPassword;
			Browser.Current.Eval("$('input#password').keyup();");
			page.ConfirmButton.EventualClick();
		}

		[When(@"I change my password using incorrect current password")]
		public void WhenIChangeMyPasswordUsingIncorrectCurrentPassword()
		{
			var page = Browser.Current.Page<PasswordPage>();
			page.Password.Value = newPassword;
			page.PasswordValidation.Value = newPassword;
			page.OldPassword.Value = TestData.CommonPassword + "fel";
			Browser.Current.Eval("$('input#password').keyup();");
			page.ConfirmButton.EventualClick();
		}

		[When(@"I am changing password using incorrect confirm password")]
		public void WhenIAmChangingPasswordUsingIncorrectConfirmPassword()
		{
			var page = Browser.Current.Page<PasswordPage>();
			page.Password.Value = newPassword;
			page.PasswordValidation.Value = newPassword + "fel";
			page.OldPassword.TypeText(TestData.CommonPassword);
			Browser.Current.Eval("$('input#password').keyup();");
		}

		[When(@"I sign in using my new password")]
		public void WhenISignInUsingMyNewPassword()
		{
			var userName = UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
			var signInpage = Browser.Current.Page<SignInPage>();
			signInpage.SignInApplication(userName, newPassword);
		}


		[Then(@"I should see my culture")]
		public void ThenIShouldSeeMyCulture()
		{
			var user = UserFactory.User();
			var page = Browser.Current.Page<RegionalSettingsPage>();
			EventualAssert.That(() => page.CultureSelect.SelectedText, Is.StringContaining(user.Person.PermissionInformation.Culture().DisplayName));
		}

		[Then(@"I should see my language")]
		public void ThenIShouldSeeMyLanguage()
		{
			var user = UserFactory.User();
			var page = Browser.Current.Page<RegionalSettingsPage>();
			EventualAssert.That(() => page.CultureUiSelect.SelectedText, Is.StringContaining(user.Person.PermissionInformation.UICulture().DisplayName));
		}

		[When(@"I change culture to US")]
		public void WhenIChangeCultureToUS()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureSelect.Select(CultureInfo.GetCultureInfo(1033).DisplayName);
		}

		[When(@"I change culture to browser's default")]
		public void WhenIChangeCultureToBrowserSDefault()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureSelect.Select(UserTexts.Resources.BrowserDefault);
		}

		[When(@"I change language to english")]
		public void WhenIChangeLanguageToEnglish()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureUiSelect.Select(CultureInfo.GetCultureInfo(1033).DisplayName);
		}

		[When(@"I change language to browser's default")]
		public void WhenIChangeLanguageToBrowserSDefault()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureUiSelect.Select(UserTexts.Resources.BrowserDefault);
		}

		[Then(@"I should see US date format")]
		public void ThenIShouldSeeUSDateFormat()
		{
			Navigation.GotoTeamSchedule();
			var page = Browser.Current.Page<TeamSchedulePage>();
			EventualAssert.That(() => page.DatePicker.DateFormat, Is.EqualTo("m/d/yy"));
		}

		[Then(@"I should see the browser's language's date format")]
		public void ThenIShouldSeeTheBrowserSLanguageSDateFormat()
		{
			// don't know a good way to read http header from browser to server
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see english text")]
		public void ThenIShouldSeeEnglishText()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			EventualAssert.That(() => page.RequestsLink.Text, Is.EqualTo("Requests"));
		}

		[Then(@"I should see text in the the browser's language")]
		public void ThenIShouldSeeTextInTheTheBrowserSLanguage()
		{
			// don't know a good way to read http header from browser to server
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see a message saying the password is not confirmed correctly")]
		public void ThenIShouldSeeAMessageSayingThePasswordIsNotConfirmedCorrectly()
		{
			var page = Browser.Current.Page<PasswordPage>();
			EventualAssert.That(() => page.NonMatchingNewPassword.DisplayVisible(), Is.True);
		}

		[Then(@"Confirm button should be disabled")]
		public void ThenConfirmButtonShouldBeDisabled()
		{
			var page = Browser.Current.Page<PasswordPage>();
			EventualAssert.That(() => page.ConfirmButton.Enabled, Is.False);
		}

		[Then(@"I should see a message saying the password is incorrect")]
		public void ThenIShouldSeeAMessageSayingThePasswordIsIncorrect()
		{
			var page = Browser.Current.Page<PasswordPage>();
			EventualAssert.That(() => page.IncorrectPassword.DisplayVisible(), Is.True);
		}
	}
}