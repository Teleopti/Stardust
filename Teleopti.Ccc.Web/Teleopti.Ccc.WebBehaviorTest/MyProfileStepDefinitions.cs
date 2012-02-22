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
		[When(@"I view my regional settings")]
		public void WhenIViewMyRegionalSettings()
		{
			var userName = UserFactory.User().MakeUser();
			Navigation.GotoGlobalSignInPage();
			var page = Browser.Current.Page<SignInPage>();
			page.SignInApplication(userName, TestData.CommonPassword);
			Navigation.GotoRegionalSettings();
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
			//not nice - but somehow I need to wait for a portal refresh here
			Thread.Sleep(300);
		}

		[When(@"I change culture to browser's default")]
		public void WhenIChangeCultureToBrowserSDefault()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureSelect.SelectByValue("-1");
			Browser.Current.Eval("$('#cultureSelect').change();");
			//not nice - but somehow I need to wait for a portal refresh here
			Thread.Sleep(300);
		}

		[When(@"I change language to english")]
		public void WhenIChangeLanguageToEnglish()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.CultureUiSelect.SelectByValue("1033");
			Browser.Current.Eval("$('#cultureUiSelect').change();");
			//not nice - but somehow I need to wait for a portal refresh here
			Thread.Sleep(300);
		}

		[Then(@"I should see US date format")]
		public void ThenIShouldSeeUSDateFormat()
		{
			Navigation.GotoTeamSchedule();
			EventualAssert.That(() => Browser.Current.Page<TeamSchedulePage>().DatePicker.DateFormat, Is.EqualTo("m/d/yy"));
		}

		[Then(@"I should see the browser's language's date format")]
		public void ThenIShouldSeeTheBrowserSLanguageSDateFormat()
		{
			Navigation.GotoTeamSchedule();
			var dateFormat = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern; //wrong here!
			EventualAssert.That(() => Browser.Current.Page<TeamSchedulePage>().DatePicker.DateFormat, Is.EqualTo(dateFormat));
		}

		[Then(@"I should see english text")]
		public void ThenIShouldSeeEnglishText()
		{
			var page = Browser.Current.Page<RegionalSettingsPage>();
			page.RequestsLink.Text.Should().Be.EqualTo("Requests");
		}
	}
}