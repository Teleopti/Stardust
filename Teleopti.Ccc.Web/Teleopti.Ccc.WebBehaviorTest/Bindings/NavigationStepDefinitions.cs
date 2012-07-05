using System;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class NavigationStepDefinitions
	{
		[When(@"I sign in")]
		[When(@"I sign in by user name")]
		public void WhenISignIn()
		{
			var userName = UserFactory.User().MakeUser();
			if (!(Browser.Current.Url.Contains("/SignIn") || Browser.Current.Url.Contains("/MobileSignIn")))
				Navigation.GotoGlobalSignInPage();
			Pages.Pages.CurrentSignInPage.SignInApplication(userName, TestData.CommonPassword);
		}

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

		[Given(@"I am viewing an application page")]
		public void WhenIAmViewingAnApplicationPage()
		{
			if (!UserFactory.User().HasSetup<IUserRoleSetup>())
				UserFactory.User().Setup(new Agent());
			TestControllerMethods.Logon();
			Navigation.GotoAnApplicationPage();
		}

		[When(@"I view my week schedule")]
		[When(@"I am viewing week schedule")]
		[Given(@"I view my week schedule")]
		[Given(@"I am viewing week schedule")]
		[Given(@"I am viewing schedule")]
		public void WhenIViewMyWeekSchedule()
		{
			TestControllerMethods.Logon();
			Navigation.GotoWeekSchedulePage();
		}

		[Given(@"I am viewing preferences")]
		[When(@"I view preferences")]
		public void GivenIAmViewingPreferences()
		{
			TestControllerMethods.Logon();
			Navigation.GotoPreference();
		}

		[When(@"I navigate to the preferences page")]
		public void WhenINavigateToThePreferencesPage()
		{
			Navigation.GotoPreference();
		}

		[Given(@"I view team schedule")]
		[Given(@"I am viewing team schedule")]
		[Given(@"I am viewing team schedule for today")]
		[When(@"I view team schedule")]
		public void WhenIViewTeamSchedule()
		{
			//var userName = UserFactory.User().MakeUser();
			//Navigation.GotoGlobalSignInPage();
			//var page = Browser.Current.Page<SignInPage>();
			//page.SignInApplication(userName, TestData.CommonPassword);
			//if (page.BusinessUnitList.Exists)
			//{
			//    page.SelectFirstBusinessUnit();
			//    page.ClickBusinessUnitOkButton();
			//}
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule();
		}

		[Given(@"I view my week schedule one month ago")]
		public void GivenIViewMyWeekScheduleOneMonthAgo()
		{
			TestControllerMethods.Logon();
			Navigation.GotoWeekSchedulePage(DateTime.Now.AddMonths(1));
		}

		[Given(@"I am viewing team schedule for tomorrow")]
		public void GivenIAmViewingTeamScheduleForTomorrow()
		{
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule(DateTime.Today.AddDays(1));
		}

		[When(@"I navigate to the team schedule")]
		public void WhenINavigateToTheTeamSchedule()
		{
			Navigation.GotoTeamSchedule();
		}

		[When(@"I navigate the internet")]
		public void WhenIBrowseTheInternet()
		{
			Navigation.GotoTheInternet();
		}

		[When(@"I navigate to an application page")]
		public void WhenIBrowseToAnApplicationPage()
		{
			Navigation.GotoAnApplicationPageOutsidePortal();
		}

		[When(@"I navigate to the site home page")]
		public void WhenIBrowseToTheSiteHomePage()
		{
			Navigation.GotoSiteHomePage();
		}

		[When(@"I click the current week button")]
		public void WhenIClickTheCurrentWeekButton()
		{
			Pages.Pages.WeekSchedulePage.TodayButton.EventualClick();
		}
	}
}