﻿using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class RoutingStepDefinition
	{

		[Then(@"I should see the mobile global menu")]
		public void ThenIShouldSeeTheMobileGlobalMenu()
		{
			// The user should endup if mobile device detected. Mark this as pending..?
			// ScenarioContext.Current.Pending();
			Navigation.GotoGlobalMobileMenuPage();

			EventualAssert.That(() => Pages.Pages.MobileGlobalMenuPage.GlobalMenuList.ListItems.Count.Equals(2), Is.True);
		}

		[Then(@"I should see the global sign in page")]
		public void ThenIShouldSeeTheGlobalSignInPage()
		{
			EventualAssert.That(() => Pages.Pages.SignInPage.ApplicationTabLink.Exists, Is.True);
		}

		[Then(@"I should see MyTime's sign in page")]
		public void ThenIShouldSeeMyTimesSignInPage()
		{
			EventualAssert.That(() => Pages.Pages.SignInPage.ApplicationTabLink.Exists, Is.True);
			Browser.Current.Url.Should().EndWith("/MyTime/Authentication/SignIn");
		}

		[Then(@"I should see Mobile Report's sign in page")]
		public void ThenIShouldSeeMobileReportsSignInPage()
		{
			EventualAssert.That(() => Pages.Pages.MobileSignInPage.ApplicationSignIn.Exists, Is.True);
			Browser.Current.Url.Should().Contain("/MobileReports/Authentication/SignIn");
		}

		[Then(@"I should see the global menu")]
		public void ThenIShouldSeeTheGlobalMenu()
		{
			EventualAssert.That(() => Pages.Pages.GlobalMenuPage.GlobalMenuList.ListItems.Count.Equals(2), Is.True);
			Browser.Current.Url.Should().EndWith("/Start/Menu/Menu");
		}

		[Then(@"I should see MyTime")]
		public void ThenIShouldSeeMyTime()
		{
			EventualAssert.That(() => Pages.Pages.WeekSchedulePage.DatePicker.Exists, Is.True);
		}

		[Then(@"I should see Mobile Reports")]
		public void ThenIShouldSeeMobileReports()
		{
			// Settings is Now preferred "home"
			EventualAssert.That(() => Pages.Pages.MobileReportsPage.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True);
		}
	}
}
