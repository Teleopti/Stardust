using System.Globalization;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class MyProfileStepDefinitions
	{
		[When(@"I view my settings")]
		[When(@"I view my regional settings")]
		public void WhenIViewMyRegionalSettings()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent());
			TestControllerMethods.Logon();
			Navigation.GotoAnApplicationPage();
			Browser.Interactions.Click(".user-name-link");
			Browser.Interactions.Click("#regional-settings");
		}

		[When(@"I view password setting page")]
		[When(@"I view my password")]
		public void WhenIViewMyPassword()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent());
			TestControllerMethods.Logon();
			Navigation.GotoAnApplicationPage();
			Browser.Interactions.Click(".user-name-link");
			Browser.Interactions.Click("#change-your-password");
		}

		[When(@"I change my password to '(.*)'")]
		public void WhenIChangeMyPassword(string newPassword)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#password", newPassword);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#passwordValidation", newPassword);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#oldPassword", TestData.CommonPassword);
			Browser.Interactions.ClickUsingJQuery("#passwordButton");
		}

		[When(@"I change my password in my profile with")]
		public void WhenIChangeMyPasswordInMyProfileWith(Table table)
		{
			var password = table.CreateInstance<PasswordInfo>();
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#password", password.Password);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#passwordValidation", password.ConfirmedPassword);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#oldPassword", password.OldPassword);
			Browser.Interactions.ClickUsingJQuery("#passwordButton");
		}

		public class PasswordInfo
		{
			public string Password { get; set; }
			public string ConfirmedPassword { get; set; }
			public string OldPassword { get; set; }
		}

		[When(@"I change my password using incorrect current password")]
		public void WhenIChangeMyPasswordUsingIncorrectCurrentPassword()
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#password", "newP@ssw0rd");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#passwordValidation", "newP@ssw0rd");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#oldPassword", TestData.CommonPassword + "fel");
			Browser.Interactions.ClickUsingJQuery("#passwordButton");
		}

		[When(@"I am changing password using incorrect confirm password")]
		public void WhenIAmChangingPasswordUsingIncorrectConfirmPassword()
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#password", "newP@ssw0rd");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#passwordValidation", "newP@ssw0rd" + "fel");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#oldPassword", TestData.CommonPassword);
		}

		[Then(@"I should see my culture")]
		public void ThenIShouldSeeMyCulture()
		{
			var user = DataMaker.Data();
			Browser.Interactions.AssertFirstContains("#s2id_Culture-Picker a span", user.MePerson.PermissionInformation.Culture().DisplayName);
		}

		[Then(@"I should see my language")]
		public void ThenIShouldSeeMyLanguage()
		{
			var user = DataMaker.Data();
			Browser.Interactions.AssertFirstContains("#s2id_CultureUi-Picker a span", user.MePerson.PermissionInformation.UICulture().DisplayName);
		}

		[When(@"I change culture to US")]
		public void WhenIChangeCultureToUS()
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery("#Culture-Picker", CultureInfo.GetCultureInfo(1033).DisplayName);
		}

		[When(@"I change language to english")]
		public void WhenIChangeLanguageToEnglish()
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery("#CultureUi-Picker", CultureInfo.GetCultureInfo(1033).DisplayName);
		}

		[Then(@"I should see US date format"), SetCulture("en-US")]
		public void ThenIShouldSeeUSDateFormat()
		{
			Navigation.GotoTeamSchedule();
			Browser.Interactions.AssertExists(string.Format(@"div:[data-mytime-periodselection*=""""Display"": ""{0}""""]",
																			DateOnlyForBehaviorTests.TestToday.Date.ToShortDateString()));
			//Browser.Interactions.AssertExists(string.Format(@"div:[data-mytime-periodselection*="{0}"]",
			//													DateOnlyForBehaviorTests.TestToday.Date.ToShortDateString()));
			//Browser.Interactions.AssertAnyContains("#TeamSchedule-body", string.Format("{0}", DateOnlyForBehaviorTests.TestToday.Date.ToShortDateString()));
			//Browser.Interactions.AssertExistsUsingJQuery("'{0}'", DateOnlyForBehaviorTests.TestToday.Date.ToShortDateString());
		}


		[Then(@"I should see english text")]
		public void ThenIShouldSeeEnglishText()
		{
			Browser.Interactions.AssertFirstContains("a[href='#RequestsTab']","Requests");
		}

		[Then(@"I should see a message saying the password is not confirmed correctly")]
		public void ThenIShouldSeeAMessageSayingThePasswordIsNotConfirmedCorrectly()
		{
			Browser.Interactions.AssertExists("#nonMatchingPassword");
		}

		[Then(@"Confirm button should be disabled")]
		public void ThenConfirmButtonShouldBeDisabled()
		{
			Browser.Interactions.AssertExists(".btn-primary:disabled");
		}

		[Then(@"I should see a message saying the password is incorrect")]
		public void ThenIShouldSeeAMessageSayingThePasswordIsIncorrect()
		{
			Browser.Interactions.AssertExists("#incorrectOldPassword");
		}

		[Then(@"I should see password change failed with message")]
		public void ThenIShouldSeePasswordChangeFailedWithMessage()
		{
			Browser.Interactions.AssertExists("#invalidNewPassword");
		}
	}
}