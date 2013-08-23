using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class NavigationStepDefinitions
	{
		[When(@"Someone is viewing sharing link")]
		public void WhenSomeoneIsViewingSharingLink()
		{
			UserFactory.User().MakeUser();
			Navigation.GotoRaw(UserFactory.User().UserData<CalendarLinkConfigurable>().SharingUrl);
		}

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
			Navigation.GotoWeekSchedulePageNoWait();
		}

		[When(@"I view my week schedule")]
		[When(@"I am viewing week schedule")]
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
			Navigation.GotoAvailability(date);
		}

		[When(@"I am viewing the performance view")]
		public void WhenIAmViewingThePerformanceView()
		{
			TestControllerMethods.Logon();
			Navigation.GoToPerformanceTool();
		}

		[When(@"I view schedules for '([0-9\-\\\/]*)'")]
		public void WhenIViewSchedulesForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date);
		}
		
		[When(@"I view schedules for '(.*)' on '(.*)'")]
		public void WhenIViewSchedulesWithTeamAndDate(string teamName, DateTime date)
		{
			TestControllerMethods.Logon();
			var teamSetups = UserFactory.User().UserDatasOfType<TeamConfigurable>();
			var teamId = (from t in teamSetups
						  let team = t.Team
						  where team.Description.Name.Equals(teamName)
						  select team.Id.Value).First();
			Navigation.GotoAnywhereTeamSchedule(date, teamId);
		}

		[When(@"I view person schedule for '(.*)' on '(.*)'")]
		public void WhenIViewPersonScheduleForPersonOnDate(string name, DateTime date)
		{
			TestControllerMethods.Logon();
			var personId = UserFactory.User(name).Person.Id.Value;
			Navigation.GotoAnywherePersonSchedule(personId, date);
		}

		[When(@"I view person schedules add full day absence form for '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddFullDayAbsenceFormForPersonOnDate(string name, DateTime date)
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
			TestControllerMethods.LogonWithReadModelsUpdated();
			Navigation.GotoTeamSchedule();
		}

		[Given(@"I am viewing team schedule for tomorrow")]
		public void GivenIAmViewingTeamScheduleForTomorrow()
		{
			TestControllerMethods.LogonWithReadModelsUpdated();
			Navigation.GotoTeamSchedule(DateTime.Today.AddDays(1));
		}

		[When(@"I view team schedule for '(.*)'")]
		public void WhenIViewTeamScheduleFor(DateTime date)
		{
			TestControllerMethods.LogonWithReadModelsUpdated();
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




		// navigation to signin and root
		// does not logon, but creates user

		[Given(@"I am viewing the sign in page")]
		public void GivenIAmAtTheSignInPage()
		{
			UserFactory.User().MakeUser();
			Navigation.GotoGlobalSignInPage();
		}

		[When(@"I navigate to the site home page")]
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