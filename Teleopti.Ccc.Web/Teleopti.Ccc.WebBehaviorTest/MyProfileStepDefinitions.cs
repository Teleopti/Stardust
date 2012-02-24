using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
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
			page.Password.TypeText(newPassword);
			page.PasswordValidation.TypeText(newPassword);
			page.OldPassword.TypeText(TestData.CommonPassword);
			page.ConfirmButton.Click();
		}

		[When(@"I sign in using my new password")]
		public void WhenISignInUsingMyNewPassword()
		{
			var userName = UserFactory.User().Person.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName;
			var signInpage = Browser.Current.Page<SignInPage>();
			signInpage.SignInApplication(userName, newPassword);
		}


		[Then(@"I should see my culture")]
		public void ThenIShouldSeeMyCulture()
		{
			var user = UserFactory.User();
			var page = Browser.Current.Page<RegionalSettingsPage>();
			EventualAssert.That(() => page.CultureSelect.SelectedItem, Is.EqualTo(user.Person.PermissionInformation.Culture().DisplayName));
		}

		[Then(@"I should see my language")]
		public void ThenIShouldSeeMyLanguage()
		{
			var user = UserFactory.User();
			var page = Browser.Current.Page<RegionalSettingsPage>();
			EventualAssert.That(() => page.CultureUiSelect.SelectedItem, Is.EqualTo(user.Person.PermissionInformation.UICulture().DisplayName));
		}

		[When(@"I change culture to US")]
		public void WhenIChangeCultureToUS()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureSelect.SelectByValue("1033");
			Browser.Current.Eval("$('#cultureSelect').change();"); 
		}

		[When(@"I change culture to browser's default")]
		public void WhenIChangeCultureToBrowserSDefault()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureSelect.SelectByValue("-1");
			Browser.Current.Eval("$('#cultureSelect').change();");
		}

		[When(@"I change language to english")]
		public void WhenIChangeLanguageToEnglish()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureUiSelect.SelectByValue("1033");
			Browser.Current.Eval("$('#cultureUiSelect').change();");
		}

		[When(@"I change language to browser's default")]
		public void WhenIChangeLanguageToBrowserSDefault()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureUiSelect.SelectByValue("-1");
			Browser.Current.Eval("$('#cultureUiSelect').change();");
		}

		[Then(@"I should see US date format")]
		public void ThenIShouldSeeUSDateFormat()
		{
			//not nice - but somehow I need to wait for a portal refresh here
			Thread.Sleep(300);
			Navigation.GotoTeamSchedule();
			EventualAssert.That(() => Browser.Current.Page<TeamSchedulePage>().DatePicker.DateFormat, Is.EqualTo("m/d/yy"));
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
			//not nice - but somehow I need to wait for a portal refresh here
			Thread.Sleep(300);
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.RequestsLink.Text.Should().Be.EqualTo("Requests");
		}

		[Then(@"I should see text in the the browser's language")]
		public void ThenIShouldSeeTextInTheTheBrowserSLanguage()
		{
			// don't know a good way to read http header from browser to server
			ScenarioContext.Current.Pending();
		}
	}
}