using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class NavigationStepDefinitions
	{

		[Given(@"I am viewing an application page")]
		[When(@"I am viewing an application page")]
		public void WhenIAmViewingAnApplicationPage()
		{
			if (!UserFactory.User().HasSetup<IUserRoleSetup>())
				UserFactory.User().Setup(new Agent());
			TestControllerMethods.Logon();
			Navigation.GotoAnApplicationPage();
		}

		[When(@"I manually navigate to week schedule page")]
		public void WhenIManuallyNavigateToWeekSchedulePage()
		{
			Navigation.GotoAnApplicationPage();
		}

		[When(@"I view my week schedule")]
		[When(@"I am viewing week schedule")]
		[Given(@"I view my week schedule")]
		[Given(@"I am viewing week schedule")]
		public void WhenIViewMyWeekSchedule()
		{
			TestControllerMethods.Logon();
			Navigation.GotoWeekSchedulePage();
		}

        [Given(@"I view my week schedule for date '(.*)'")]
        [When(@"I view my week schedule for date '(.*)'")]
        public void WhenIViewMyWeekScheduleForDate(DateTime date)
        {
            TestControllerMethods.Logon();
            Navigation.GotoWeekSchedulePage(date);
        }

        [When(@"I navigate to week schedule page for date '(.*)'")]
        public void WhenINavigateToWeekSchedulePageForDate(DateTime date)
        {
            Navigation.GotoWeekSchedulePage(date);
        }

		[Given(@"I am viewing preferences")]
		[When(@"I am viewing preferences")]
		[When(@"I view preferences")]
		public void GivenIAmViewingPreferences()
		{
			TestControllerMethods.Logon();
			Navigation.GotoPreference();
		}

		[Given(@"I am viewing preferences for date '(.*)'")]
		[When(@"I view preferences for date '(.*)'")]
		public void WhenIViewPreferencesForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoPreference(date);
		}

		[Given(@"I am viewing student availability for date '(.*)'")]
		public void GivenIAmViewingStudentAvailabilityForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoStudentAvailability(date);
		}

		[When(@"I view schedules for '(.*)'")]
		public void WhenIViewSchedules(string date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date);
		}

		[When(@"I view person schedule for '(.*)' on '(.*)'")]
		public void WhenIViewPersonScheduleForPersonOnDate(string name, DateTime date)
		{
			TestControllerMethods.Logon();
			var personId = UserFactory.User(name).Person.Id.Value;
			Navigation.GotoAnywherePersonSchedule(personId, date);
		}

		[When(@"I view agent schedules add full day absence form for '(.*)' on '(.*)'")]
		public void WhenIViewAgentSchedulesAddFullDayAbsenceFormForAgentOnDate(string name, DateTime date)
		{
			TestControllerMethods.Logon();
			var personId = UserFactory.User(name).Person.Id.Value;
			Navigation.GotoAnywherePersonScheduleFullDayAbsenceForm(personId, date);
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
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule();
		}

		[Given(@"I am viewing team schedule for tomorrow")]
		public void GivenIAmViewingTeamScheduleForTomorrow()
		{
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule(DateTime.Today.AddDays(1));
		}

		[When(@"I view team schedule for '(.*)'")]
		public void WhenIViewTeamScheduleFor(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule(date);
		}


		[Given(@"I am viewing requests")]
		[When(@"I am viewing requests")]
		[When(@"I view requests")]
		public void GivenIAmViewingRequests()
		{
			TestControllerMethods.Logon();
			Navigation.GotoRequests();
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







		// navigation to signin and root
		// does not logon, but creates user

		[Given(@"I am viewing the sign in page")]
		public void GivenIAmAtTheSignInPage()
		{
			UserFactory.User().MakeUser();
			Navigation.GotoGlobalSignInPage();
		}

		[When(@"I navigate to the site's root")]
		public void WhenINavigateToTheSiteSRoot()
		{
			UserFactory.User().MakeUser();
			Navigation.GotoSiteHomePage();
		}

		[When(@"I navigate to MyTime")]
		public void WhenINavigateToMyTime()
		{
			UserFactory.User().MakeUser();
			Navigation.GotoMyTime();
		}

		[When(@"I navigate to Mobile Reports")]
		public void WhenINavigateToMobileReposrts()
		{
			UserFactory.User().MakeUser();
			Navigation.GotoMobileReports();
		}

		[When(@"I navigate to Anywhere")]
		public void WhenINavigateToAnywhere()
		{
			UserFactory.User().MakeUser();
			Navigation.GotoAnywhere();
		}
	}
}