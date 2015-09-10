using System;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Interfaces.Domain;
using SiteConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.SiteConfigurable;
using TeamConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.TeamConfigurable;

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
			Navigation.GotoAsm();
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Asm.MakeSureWeAreLoggedOn();");
		}

		[Given(@"I am viewing messages")]
		[When(@"I am viewing messages")]
		public void WhenIAmViewingMessages()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMessagePage();
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

		[When(@"I manually navigate to mobile week schedule page")]
		public void WhenIManuallyNavigateToMobileWeekSchedulePage()
		{
            Navigation.GotoMobileWeekSchedulePageNoWait();
		}

		[Given(@"I am viewing my mobile week schedule")]
        [When(@"I view my mobile week schedule")]
		public void WhenIViewMyMobileWeekSchedule()
		{
            TestControllerMethods.Logon();
            Navigation.GotoMobileWeekSchedulePage();
        }

        [When(@"I view my mobile week schedule for date '(.*)'")]
		  [Given(@"I view my mobile week schedule for date '(.*)'")]
        public void WhenIViewMyMobileWeekScheduleForDate(DateTime date)
        {
            TestControllerMethods.Logon();
            Navigation.GotoMobileWeekSchedulePage(date);
        }


		[When(@"I view my week schedule")]
		[When(@"I am viewing week schedule")]
		[Given(@"I am viewing week schedule")]
		public void WhenIViewMyWeekSchedule()
		{
			TestControllerMethods.Logon();
			Navigation.GotoWeekSchedulePage();
		}

		[When(@"I am viewing leaderboard report")]
		[Given(@"I am viewing leaderboard report")]
		public void WhenIAmViewingLeaderboardReport()
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GoToLeaderboardReport();
		}

		[When(@"I go to mytime web")]
		public void WhenIGoToMytimeWeb()
		{
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoWeekSchedulePageNoWait();
		}

		[When(@"I logon to mytime web")]
		public void WhenILogonToMytimeWeb()
		{
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoWeekSchedulePage();
		}

		[Given(@"I view my week schedule for date '(.*)'")]
		[When(@"I view my week schedule for date '(.*)'")]
		public void WhenIViewMyWeekScheduleForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoWeekSchedulePage(date);
		}

		[Given(@"I view my month schedule for date '(.*)'")]
		[When(@"I view my month schedule for date '(.*)'")]
		public void WhenIViewMyMonthScheduleForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoMonthSchedulePage(date);
		}

		[When(@"I view my month schedule")]
		public void WhenIViewMyMonthSchedule()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMonthSchedulePage();
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

		[When(@"I am viewing the performance view for '(.*)'")]
		public void WhenIAmViewingThePerformanceViewFor(string scenarioName)
		{
			TestControllerMethods.Logon();
			Navigation.GoToPerformanceTool(scenarioName);
		}


		[When(@"I am viewing the diagnosis view")]
		public void WhenIAmViewingTheDiagnosisView()
		{
			TestControllerMethods.Logon();
			Navigation.GoToMessageBrokerTool();
		}

		[When(@"I am viewing the health check view")]
		public void WhenIAmViewingTheHealthCheckView()
		{
			TestControllerMethods.Logon();
			Navigation.GoToHealthCheck();
		}

		[When(@"I am viewing the RTA Tool")]
		public void WhenIAmViewingTheRtaTool()
		{
			TestControllerMethods.Logon();
			Navigation.GoToRtaTool();
		}

		[Given(@"I view schedules for '([0-9\-\\\/]*)'")]
		[When(@"I view schedules for '([0-9\-\\\/]*)'")]
		[When(@"I view group schedules staffing metrics for '([0-9\-\\\/]*)'")]
		public void WhenIViewSchedulesForDate(DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, DefaultBusinessUnit.BusinessUnitFromFakeState.Id.GetValueOrDefault());
		}

		[When(@"I view schedules for '(.*)' on '(.*)'")]
		public void WhenIViewSchedulesWithTeamAndDate(string teamName, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, IdForTeam(teamName), buIdForTeam(teamName));
		}

		private static Guid IdForTeam(string teamName)
		{
			var teamId = (from t in DataMaker.Data().UserDatasOfType<TeamConfigurable>()
										let team = t.Team
										where team.Description.Name.Equals(teamName)
										select team.Id.GetValueOrDefault()).First();
			return teamId;
		}

		private static Guid IdForSite(string siteName)
		{
			var siteId = (from t in DataMaker.Data().UserDatasOfType<SiteConfigurable>()
										let site = t.Site
										where site.Description.Name.Equals(siteName)
										select site.Id.GetValueOrDefault()).First();
			return siteId;
		}

		private static Guid buIdForSite(string siteName)
		{
			var buId = (from t in DataMaker.Data().UserDatasOfType<SiteConfigurable>()
										let site = t.Site
										where site.Description.Name.Equals(siteName)
										select site.BusinessUnit.Id.GetValueOrDefault()).First();
			return buId;
		}

		private static Guid buIdForTeam(string teamName)
		{
			var buId = (from t in DataMaker.Data().UserDatasOfType<TeamConfigurable>()
						let team = t.Team
						where team.Description.Name.Equals(teamName)
						select team.BusinessUnitExplicit.Id.GetValueOrDefault()).First();
			return buId;
		}

		[Given(@"I am viewing group schedules staffing metrics for '([0-9\-\\\/]*)' and '(.*)'")]
		[When(@"I view group schedules staffing metrics for '([0-9\-\\\/]*)' and '(.*)'")]
		public void WhenIViewTeamSchedulesStaffingMetricsForDate(DateTime date, string skill)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, DefaultBusinessUnit.BusinessUnitFromFakeState.Id.GetValueOrDefault());
			TeamSchedulePageStepDefinitions.SelectSkill(skill);
		}

		[Given(@"I am viewing group schedules staffing metrics for '(.*)' and '([0-9\-\\\/]*)' and '(.*)'")]
		[When(@"I view group schedules staffing metrics for '(.*)' and '([0-9\-\\\/]*)' and '(.*)'")]
		public void WhenIViewTeamSchedulesStaffingMetricsForDateAndTeam(string team, DateTime date, string skill)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, IdForTeam(team), buIdForTeam(team));
			TeamSchedulePageStepDefinitions.SelectSkill(skill);
		}

		[When(@"I view person schedule for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonScheduleForInOn(string person, string group, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(person).Person.Id.Value;
			var groupId = IdForTeam(group);
			Navigation.GotoAnywherePersonSchedule(buIdForTeam(group), groupId, personId, date);
		}

		[When(@"I view person schedules add full day absence form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddFullDayAbsenceFormForInOn(string name, string group, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			var groupId = IdForTeam(group);
			Navigation.GotoAnywherePersonScheduleFullDayAbsenceForm(buIdForTeam(group), groupId, personId, date);
		}

		[When(@"I view person schedules add intraday absence form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddIntradayAbsenceFormForInOn(string name, string @group, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GotoAnywherePersonScheduleIntradayAbsenceForm(buIdForTeam(@group), IdForTeam(@group), personId, date);
		}


		[When(@"I view person schedules add activity form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddActivityFormForAndOn(string name, string @group, DateTime date)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GotoAnywherePersonScheduleAddActivityForm(buIdForTeam(@group), personId, IdForTeam(@group), date);
		}

		[When(@"I view Resource planner")]
		public void WhenIViewResourcePlanner()
		{
			TestControllerMethods.Logon();
			Navigation.GotoResourcePlanner();
		}


		[When(@"I view Permissions")]
		public void WhenIViewPermissions()
		{
			TestControllerMethods.Logon();
			Navigation.GotoPermissions();
		}

		[When(@"I view Seat map")]
		public void WhenIViewSeatMap()
		{
			TestControllerMethods.Logon();
			Navigation.GotoSeatMap();
		}

		[When(@"I view Seat plan")]
		public void WhenIViewSeatPlan()
		{
			TestControllerMethods.Logon();
			Navigation.GotoSeatPlan(null);
		}
		
		[When(@"I view Seat plan on ""(.*)""")]
		public void WhenIViewSeatPlanOn(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoSeatPlan(date);
		}
		
		[When(@"I view Real time adherence overview")]
		public void WhenIViewRealTimeAdherenceOverview()
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereRealTimeAdherenceOverview(true);
		}

		[When(@"I try to view Real time adherence overview")]
		public void WhenITryToViewRealTimeAdherenceOverview()
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereRealTimeAdherenceOverview(false);
		}

		[When(@"I try to view real time adherence for team '(.*)'")]
		public void WhenITryToViewRealTimeAdherenceForTeam(string team)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereRealTimeAdherenceTeamOverviewNoWait(buIdForTeam(team),IdForTeam(team));
		}

		[When(@"I view Real time adherence for site '(.*)'")]
		public void WhenIViewRealTimeAdherenceForSite(string site)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereRealTimeAdherenceOverview(buIdForSite(site), IdForSite(site));
		}

		[When(@"I view manage adherence view for agent '(.*)'")]
		public void WhenIViewManageAdherenceViewForAgent(string name)
		{
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GotoAnywhereRealTimeManageAdherenceOverview(DefaultBusinessUnit.BusinessUnitFromFakeState.Id.GetValueOrDefault(), personId);
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
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoTeamSchedule();
		}

		[When(@"I navigate to my report")]
		public void WhenINavigateToMyReport()
		{
			TestControllerMethods.Logon();
			Navigation.GoToMyReport();
		}

		[Given(@"I view my adherence report for '(.*)'")]
		[When(@"I view my adherence report for '(.*)'")]
		public void GivenIViewMyAdherenceReportFor(DateTime dateTime)
		{
			TestControllerMethods.Logon();
			Navigation.GoToMyDetailedAdherence(dateTime);
		}

		[When(@"I view group schedule for '(.*)'")]
		[Given(@"I am viewing team schedule for '(.*)'")]
		[When(@"I am viewing team schedule for '(.*)'")]
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

        [Given(@"I view my queue metrics report for '(.*)'")]
        [When(@"I view my queue metrics report for '(.*)'")]
        public void WhenIViewMyQueueMetricsReportFor(DateTime dateTime)
        {
            TestControllerMethods.Logon();
            Navigation.GoToMyQueueMetrics(dateTime);
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

		[Given(@"I navigate to Anywhere")]
		[When(@"I navigate to Anywhere")]
		public void WhenINavigateToAnywhere()
		{
			DataMaker.Data().ApplyDelayed();
			Navigation.GotoAnywhere();
		}

		[When(@"I view Anywhere")]
        [Given(@"I am viewing Anywhere")]
        public void WhenIViewAnywhere()
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhere();
		}

		[Given(@"I view my report for '(.*)'")]
		[When(@"I navigate to my report for '(.*)'")]
		public void WhenINavigateToMyReportFor(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GoToMyReport(date);
		}

		[When(@"I view real time adherence for team '(.*)'")]
		public void WhenIViewRealTimeAdherenceForTeam(string team)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereRealTimeAdherenceTeamOverview(buIdForTeam(team), IdForTeam(team));
		}

		[When(@"I view real time adherence view for team '(.*)'")]
		public void WhenIViewRealTimeAdherenceViewForTeam(string team)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereRealTimeAdherenceTeamOverview(buIdForTeam(team),IdForTeam(team));
		}

		[When(@"I view people")]
		public void WhenIViewPeople()
		{
			TestControllerMethods.Logon();
			Navigation.GoToPeople();
		}

		[When(@"I view outbound")]
		public void WhenIViewOutbound()
		{
			TestControllerMethods.Logon();
			Navigation.GoToOutbound();
		}

		[When(@"I view campaign '(.*)'")]
		public void WhenIViewCampaign(string campaginName)
		{
			var campaignId = (from c in DataMaker.Data().UserDatasOfType<OutboundCampaignConfigurable>()
						  let campaign = c.Campaign
						  where campaign.Name.Equals(campaginName)
						  select campaign.Id.GetValueOrDefault())
						  .First();
			TestControllerMethods.Logon();
			Navigation.GoToOutboundCampaign(campaignId);			
		}

		[Given(@"I view forecasting")]
		public void GivenIViewForecasting()
		{
			TestControllerMethods.Logon();
			Navigation.GoToForecasting();
		}
	}
}