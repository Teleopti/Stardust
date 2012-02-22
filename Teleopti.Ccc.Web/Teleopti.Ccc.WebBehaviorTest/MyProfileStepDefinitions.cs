using System.Globalization;
using NUnit.Framework;
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
	}
}