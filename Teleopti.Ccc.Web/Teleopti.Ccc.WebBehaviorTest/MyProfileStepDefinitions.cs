using System.Globalization;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class MyProfileStepDefinitions
	{
		[When(@"I view my regional settings")]
		public void WhenIViewMyRegionalSettings()
		{
			TestControllerMethods.Logon();
			Navigation.GotoRegionalSettings();
		}

		[When(@"I view my password")]
		public void WhenIViewMyPassword()
		{
			TestControllerMethods.Logon();
			Navigation.GotoPasswordPage();
		}

		[When(@"I view password setting page")]
		public void WhenIViewPasswordSettingPage()
		{
			Navigation.GotoPasswordPage();
		}

		[When(@"I change my password to '(.*)'")]
		public void WhenIChangeMyPassword(string newPassword)
		{
			var page = Browser.Current.Page<PasswordPage>();
			page.Password.Value = newPassword;
			page.PasswordValidation.Value = newPassword;
			page.OldPassword.Value = TestData.CommonPassword;
			Browser.Current.Eval("$('input#password').keyup();");
			page.ConfirmButton.EventualClick();
		}

		[When(@"I change my password in my profile with")]
		public void WhenIChangeMyPasswordInMyProfileWith(Table table)
		{
			var password = table.CreateInstance<PasswordConfigurable>();
			var page = Browser.Current.Page<PasswordPage>();
			page.Password.Value = password.Password;
			page.PasswordValidation.Value = password.ConfirmedPassword;
			page.OldPassword.Value = password.OldPassword;
			Browser.Current.Eval("$('input#password').keyup();");
			page.ConfirmButton.EventualClick();
		}


		[When(@"I change my password using incorrect current password")]
		public void WhenIChangeMyPasswordUsingIncorrectCurrentPassword()
		{
			var page = Browser.Current.Page<PasswordPage>();
			page.Password.Value = "newP@ssw0rd";
			page.PasswordValidation.Value = "newP@ssw0rd";
			page.OldPassword.Value = TestData.CommonPassword + "fel";
			Browser.Current.Eval("$('input#password').keyup();");
			page.ConfirmButton.EventualClick();
		}

		[When(@"I am changing password using incorrect confirm password")]
		public void WhenIAmChangingPasswordUsingIncorrectConfirmPassword()
		{
			var page = Browser.Current.Page<PasswordPage>();
			page.Password.Value = "newP@ssw0rd";
			page.PasswordValidation.Value = "newP@ssw0rd" +"fel";
			page.OldPassword.Value = TestData.CommonPassword;
			Browser.Current.Eval("$('input#password').keyup();");
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
			ChangeCulture(CultureInfo.GetCultureInfo(1033).DisplayName);
		}

		[When(@"I change culture to browser's default")]
		public void WhenIChangeCultureToBrowserSDefault()
		{
			ChangeCulture(UserTexts.Resources.BrowserDefault);
		}

		[When(@"I change language to english")]
		public void WhenIChangeLanguageToEnglish()
		{
			ChangeUiCulture(CultureInfo.GetCultureInfo(1033).DisplayName);
		}

		[When(@"I change language to browser's default")]
		public void WhenIChangeLanguageToBrowserSDefault()
		{
			ChangeUiCulture(UserTexts.Resources.BrowserDefault);
		}

		[Then(@"I should see US date format")]
		public void ThenIShouldSeeUSDateFormat()
		{
			Navigation.GotoTeamSchedule();
			var page = Browser.Current.Page<TeamSchedulePage>();
			EventualAssert.That(() => page.DatePicker.DateFormat, Is.EqualTo("m/d/yy"));
		}


		[Then(@"I should see english text")]
		public void ThenIShouldSeeEnglishText()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			EventualAssert.That(() => page.RequestsLink.Text, Is.EqualTo("Requests"));
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

		[Then(@"I should see password change failed with message")]
		public void ThenIShouldSeePasswordChangeFailedWithMessage()
		{
			var page = Browser.Current.Page<PasswordPage>();
			EventualAssert.That(() => page.InvalidNewPassword.DisplayVisible(), Is.True);
		}

		private static void ChangeCulture(string culture)
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			TestControllerMethods.TestMessage("Page have not refreshed");
			EventualAssert.That(() => Browser.Current.Text, Is.StringContaining("Page have not refreshed"));
			page.CultureSelect.Select(culture);
			EventualAssert.That(() => Browser.Current.Text, Is.Not.StringContaining("Page have not refreshed"));
		}

		private static void ChangeUiCulture(string culture)
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			TestControllerMethods.TestMessage("Page have not refreshed");
			EventualAssert.That(() => Browser.Current.Text, Is.StringContaining("Page have not refreshed"));
			page.CultureUiSelect.Select(culture);
			EventualAssert.That(() => Browser.Current.Text, Is.Not.StringContaining("Page have not refreshed"));
		}

	}
}