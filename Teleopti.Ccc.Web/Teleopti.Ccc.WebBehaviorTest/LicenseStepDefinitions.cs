using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class LicenseStepDefinitions
	{
		[Given(@"I am a user that does not have license to web mytime")]
		public void GivenIAmAUserThatDoesNotHaveLicenseToWebMytime()
		{
			//ta bort license
			ScenarioContext.Current.Pending();
		}

		[When(@"I open the sign in page")]
		public void WhenIOpenTheSignInPage()
		{
			Navigation.GotoGlobalSignInPage();
		}

		[Then(@"I should get a message telling me I dont have a license")]
		public void ThenIShouldGetAMessageTellingMeIDontHaveALicense()
		{
			Assert.That(() => Browser.Current.ContainsText("error"), Is.True.After(5000, 10));
		}

		[Then(@"I should not see the sign in page")]
		public void ThenIShouldNotSeeTheSignInPage()
		{
			Assert.That(() => Browser.Current.Link("Authentication/SignIn").Exists, Is.False.After(5000, 10));
		}

		[Then(@"I Should see licensed to information")]
		public void ThenIShouldSeeLicensedToInformation()
		{
			var page = Browser.Current.Page<PortalPage>();

			EventualAssert.WhenElementExists(page.LicensedToLabel, c => c.Text, Contains.Substring(UserTexts.Resources.LicensedToColon));
			EventualAssert.WhenElementExists(page.LicensedToText, c => c.Text, Contains.Substring("Teleopti RD NOT for production use!"));
		}
	}
}