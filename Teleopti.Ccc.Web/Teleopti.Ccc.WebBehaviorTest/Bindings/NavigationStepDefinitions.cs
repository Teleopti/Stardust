using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class NavigationStepDefinitions
	{
		[Given(@"I am viewing ASM")]
		[When(@"I view ASM")]
		public void WhenIClickASMLink()
		{
			TestControllerMethods.Logon();
			Navigation.GotoAsm();
		}
		
		[When(@"I am still viewing ASM")]
		public void WhenIAmStillViewingASM()
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Asm.MakeSureWeAreLoggedOn();");
		}

		[When(@"Someone is viewing sharing link")]
		public void WhenSomeoneIsViewingSharingLink()
		{
			Navigation.GotoRaw(DataMaker.Data().UserData<CalendarLinkConfigurable>().SharingUrl);
		}

		[Given(@"I am viewing an application page")]
		[When(@"I am viewing an application page")]
		public void WhenIAmViewingAnApplicationPage()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent());
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

		[Given(@"I view schedules for '([0-9\-\\\/]*)'")]
		[When(@"I view schedules for '([0-9\-\\\/]*)'")]
		[When(@"I view group schedules staffing metrics for '([0-9\-\\\/]*)'")]
		public void WhenIViewSchedulesForDate(DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date);
		}
		
		[When(@"I view schedules for '(.*)' on '(.*)'")]
		public void WhenIViewSchedulesWithTeamAndDate(string teamName, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, IdForTeam(teamName));
		}

		private static Guid IdForTeam(string teamName)
		{
			var teamId = (from t in DataMaker.Data().UserDatasOfType<TeamConfigurable>()
			              let team = t.Team
			              where team.Description.Name.Equals(teamName)
			              select team.Id.Value).First();
			return teamId;
		}

		[Given(@"I am viewing group schedules staffing metrics for '([0-9\-\\\/]*)' and '(.*)'")]
		[When(@"I view group schedules staffing metrics for '([0-9\-\\\/]*)' and '(.*)'")]
		public void WhenIViewTeamSchedulesStaffingMetricsForDate(DateTime date, string skill)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date);
			TeamSchedulePageStepDefinitions.SelectSkill(skill);
		}

		[Given(@"I am viewing group schedules staffing metrics for '(.*)' and '([0-9\-\\\/]*)' and '(.*)'")]
		[When(@"I view group schedules staffing metrics for '(.*)' and '([0-9\-\\\/]*)' and '(.*)'")]
		public void WhenIViewTeamSchedulesStaffingMetricsForDateAndTeam(string team, DateTime date, string skill)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, IdForTeam(team));
			TeamSchedulePageStepDefinitions.SelectSkill(skill);
		}

		[When(@"I view person schedule for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonScheduleForInOn(string person, string group, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(person).Person.Id.Value;
			var groupId = IdForTeam(group);
			Navigation.GotoAnywherePersonSchedule(groupId, personId, date);
		}

		[When(@"I view person schedules add full day absence form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddFullDayAbsenceFormForInOn(string name, string group, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			var groupId = IdForTeam(group);
			Navigation.GotoAnywherePersonScheduleFullDayAbsenceForm(groupId, personId, date);
		}

		[When(@"I view person schedules add intraday absence form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddIntradayAbsenceFormForInOn(string name, string @group, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GotoAnywherePersonScheduleIntradayAbsenceForm(IdForTeam(@group), personId, date);
		}


		[When(@"I view person schedules add activity form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddActivityFormForAndOn(string name, string @group, DateTime date)
		{
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GotoAnywherePersonScheduleAddActivityForm(personId, groupIdByName(@group), date);
		}

		private static Guid groupIdByName(string team)
		{
			Guid groupid = Guid.Empty;
			ScenarioUnitOfWorkState.UnitOfWorkAction(uow =>
				{
					groupid = (from p in new GroupPageRepository(uow).LoadAll()
					           from g in p.RootGroupCollection
					           where g.Description.Name.Equals(team)
					           select g.Id.Value).Single();
				});
			return groupid;
		}

		[When(@"I navigate to the preferences page")]
		public void WhenINavigateToThePreferencesPage()
		{
			Navigation.GotoPreference();
		}

		[Given(@"I view group schedule")]
		[Given(@"I am viewing group schedule")]
		[Given(@"I am viewing group schedule for today")]
		[When(@"I view group schedule")]
		public void WhenIViewTeamSchedule()
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule();
		}

		[Given(@"I am viewing group schedule for tomorrow")]
		public void GivenIAmViewingTeamScheduleForTomorrow()
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule(DateOnlyForBehaviorTests.TestToday.Date.AddDays(1));
		}

        [When(@"I view group schedule for '(.*)'")]
        [Given(@"I am viewing group schedule for '(.*)'")]
        public void WhenIViewTeamScheduleFor(DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
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

		[When(@"I navigate to the group schedule")]
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
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoGlobalSignInPage();
		}

		[When(@"I navigate to the site home page")]
		[When(@"I navigate to the site's root")]
		public void WhenINavigateToTheSiteSRoot()
		{
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoSiteHomePage();
		}

		[When(@"I navigate to MyTime")]
		public void WhenINavigateToMyTime()
		{
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoMyTime();
		}

		[When(@"I navigate to Mobile Reports")]
		public void WhenINavigateToMobileReposrts()
		{
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoMobileReports();
		}

		[When(@"I navigate to Anywhere")]
		public void WhenINavigateToAnywhere()
		{
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoAnywhere();
		}

		[When(@"I navigate to shift trade for '(.*)'")]
		public void WhenINavigateToShiftTradeFor(DateTime date)
		{
			Navigation.GotoRequestsShiftTrade(date);
		}



	}
}