using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
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
			Browser.Interactions.Javascript_IsFlaky("Teleopti.MyTimeWeb.Asm.MakeSureWeAreLoggedOn();");
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
				DataMaker.Data().Apply(new Agent_ThingThatReallyAppliesSetupsInConstructor());
			TestControllerMethods.Logon();
			Navigation.GotoAnApplicationPage();
		}

		[Given(@"I was logged on with remember me")]
		public void GivenIWasLoggedOnWithRememberMe()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent_ThingThatReallyAppliesSetupsInConstructor());
			TestControllerMethods.LogonWithRememberMe();
			Navigation.GotoAnApplicationPage();
		}


		[When(@"I am viewing an application page as '(.*)' with password '(.*)'")]
		public void WhenIAmViewingAnApplicationPage(string userName, string password)
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent_ThingThatReallyAppliesSetupsInConstructor());
			DataMaker.Data().Apply(
				new PersonUserConfigurable
				{
					UserName = userName,
					Password = password
				});
			TestControllerMethods.LogonForSpecificUser(userName, password);
			Navigation.GotoAnApplicationPage();
		}


		[When(@"I manually navigate to week schedule page")]
		public void WhenIManuallyNavigateToWeekSchedulePage()
		{
			Navigation.GotoWeekSchedulePageNoWait();
		}

		[Given(@"I view my next week schedule")]
		public void GivenIViewMyNextWeekSchedule()
		{
			TestControllerMethods.Logon();
			Navigation.GotoWeekSchedulePage(DateTime.Now.AddDays(7));
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
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GoToLeaderboardReport();
		}

		[When(@"I go to mytime web")]
		public void WhenIGoToMytimeWeb()
		{
			Navigation.GotoWeekSchedulePageNoWait();
		}

		[When(@"I am viewing MyTime")]
		public void WhenIAmViewingMyTime()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMyTime();
		}

		[When(@"I logon to mytime web")]
		public void WhenILogonToMytimeWeb()
		{
			Navigation.GotoWeekSchedulePage();
		}

		[Given(@"I view my week schedule for date '(.*)'")]
		[When(@"I view my week schedule for date '(.*)'")]
		[Then(@"I view my week schedule for date '(.*)'")]
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
		
		[Given(@"I view schedules for '([0-9\-\\\/]*)'")]
		[When(@"I view schedules for '([0-9\-\\\/]*)'")]
		[When(@"I view group schedules staffing metrics for '([0-9\-\\\/]*)'")]
		public void WhenIViewSchedulesForDate(DateTime date)
		{
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, DefaultBusinessUnit.BusinessUnit.Id.GetValueOrDefault());
		}

		[When(@"I view schedules for '(.*)' on '(.*)'")]
		public void WhenIViewSchedulesWithTeamAndDate(string teamName, DateTime date)
		{
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			Navigation.GotoAnywhereTeamSchedule(date, IdForTeam(teamName), buIdForTeam(teamName));
		}

		[When(@"I view my settings")]
		[Then(@"I view my settings")]
		[When(@"I view my regional settings")]
		public void WhenIViewMyRegionalSettings()
		{
			if (!DataMaker.Data().HasSetup<IUserRoleSetup>())
				DataMaker.Data().Apply(new Agent_ThingThatReallyAppliesSetupsInConstructor());
			TestControllerMethods.Logon();
			Navigation.GotoRegionalSettings();
		}

		private static Guid IdForTeam(string teamName)
		{
			var teamId = (from t in DataMaker.Data().UserDatasOfType<TeamConfigurable>()
										let team = t.Team
										where team.Description.Name.Equals(teamName)
										select team.Id.GetValueOrDefault()).First();
			return teamId;
		}

		private static IEnumerable<Guid> idsForTeams(string teamNames)
		{
			var teamNamesArray = teamNames.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
			return
				(from teamName in teamNamesArray
					select IdForTeam(teamName.Trim()))
					.ToArray();
		}

		private static Guid siteIdForTeam(string teamName)
		{
			return (from t in DataMaker.Data().UserDatasOfType<TeamConfigurable>()
						  let team = t.Team
						  where team.Description.Name.Equals(teamName)
						  select team.Site.Id.GetValueOrDefault()).First();
		}

		private static IEnumerable<Guid> idsForSites(string siteNames)
		{
			var sites = siteNames.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			return (from siteName in sites select idForSite(siteName.Trim())).ToArray();
		}

		private static Guid idForSite(string siteName)
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

		private static Guid IdForSkill(string skillName)
		{
			var skillId = (from s in DataMaker.Data().UserDatasOfType<SkillConfigurable>()
				where s.Name.Equals(skillName, StringComparison.InvariantCultureIgnoreCase)
				select s.Skill.Id.Value).First();
			return skillId;
		}

		private Guid IdForState(string state)
		{
			return (from s in DataMaker.Data().UserDatasOfType<RtaMapConfigurable>()
				where s.PhoneState == state
				select s.RtaStateGroup.Id.Value).Single();
		}

		private Guid IdForSkillArea(string skillArea)
		{
			return (from s in DataMaker.Data().UserDatasOfType<SkillGroupConfigurable>()
					where s.Name.Equals(skillArea, StringComparison.InvariantCultureIgnoreCase)
					select s.SkillGroup.Id.Value).First();
		}

		[When(@"I view person schedule for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonScheduleForInOn(string person, string group, DateTime date)
		{
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(person).Person.Id.Value;
			var groupId = IdForTeam(group);
			Navigation.GotoAnywherePersonSchedule(buIdForTeam(group), groupId, personId, date);
		}

		[When(@"I view person schedules add full day absence form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddFullDayAbsenceFormForInOn(string name, string group, DateTime date)
		{
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			var groupId = IdForTeam(group);
			Navigation.GotoAnywherePersonScheduleFullDayAbsenceForm(buIdForTeam(group), groupId, personId, date);
		}

		[When(@"I view person schedules add intraday absence form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddIntradayAbsenceFormForInOn(string name, string @group, DateTime date)
		{
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GotoAnywherePersonScheduleIntradayAbsenceForm(buIdForTeam(@group), IdForTeam(@group), personId, date);
		}


		[When(@"I view person schedules add activity form for '(.*)' in '(.*)' on '(.*)'")]
		public void WhenIViewPersonSchedulesAddActivityFormForAndOn(string name, string @group, DateTime date)
		{
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GotoAnywherePersonScheduleAddActivityForm(buIdForTeam(@group), personId, IdForTeam(@group), date);
		}


		[When(@"I view Permissions")]
		public void WhenIViewPermissions()
		{
			TestControllerMethods.Logon();
			Navigation.GotoPermissions();
		}

		[When(@"I view new Permissions")]
		public void WhenIViewNewPermissions()
		{
			TestControllerMethods.Logon();
			Navigation.GotoPermissionsNew();
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
		
		[When(@"I view Wfm")]
		public void WhenIViewWfm()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmLandingPage();
		}

		[When(@"I view Real time adherence sites")]
		public void WhenIViewRealTimeAdherenceSites()
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceSites();
		}
		
		[When(@"I view Real time adherence for skill '(.*)' for sites")]
		public void WhenIViewRealTimeAdherenceForSkillForSites(string skill)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillForSites(IdForSkill(skill));
		}

		[When(@"I view Real time adherence for skill '(.*)' for teams on site '(.*)'")]
		public void WhenIViewRealTimeAdherenceForSkillForTeamsOnSite(string skill, string site)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillForTeamsOnSite(IdForSkill(skill), idForSite(site));
		}

		[When(@"I view Real time adherence for skill area '(.*)' for sites")]
		public void WhenIViewRealTimeAdherenceForSkillAreaForSites(string skillarea)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillAreaForSites(IdForSkillArea(skillarea));
		}

		[When(@"I view Real time adherence for skill area '(.*)' for teams on site '(.*)'")]
		public void WhenIViewRealTimeAdherenceForSkillAreaForTeamsOnSite(string skillarea, string site)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillAreaForTeamsOnSite(IdForSkillArea(skillarea), idForSite(site));
		}

		
		[When(@"I view Real time adherence for teams on site '(.*)'")]
		public void WhenIViewRealTimeAdherenceForTeamsOnSite(string site)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForTeamsOnSite(idForSite(site));
		}

		[Given(@"I am viewing real time adherence for skill '(.*)' on teams '(.*)'")]
		public void GivenIAmViewingRealTimeAdherenceForSkillOnTeams(string skill, string teams)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillOnTeams(idsForTeams(teams), IdForSkill(skill));
		}

		[Given(@"I am viewing real time adherence for skill '(.*)' on sites '(.*)'")]
		public void GivenIAmViewingRealTimeAdherenceForSkillOnSites(string skill, string sites)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillOnSites(idsForSites(sites), IdForSkill(skill));
		}

		[Given(@"I am viewing real time adherence for skill area '(.*)' on sites '(.*)'")]
		public void GivenIAmViewingRealTimeAdherenceForSkillAreaOnSites(string skillArea, string sites)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillAreaOnSites(idsForSites(sites), IdForSkillArea(skillArea));
		}

		[Given(@"I am viewing real time adherence for skill area '(.*)' on teams '(.*)'")]
		public void GivenIAmViewingRealTimeAdherenceForSkillAreaOnTeams(string skillArea, string teams)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillAreaOnTeams(idsForTeams(teams), IdForSkillArea(skillArea));
		}

		[Given(@"I am viewing real time adherence for sites '(.*)' and teams '(.*)' with skill '(.*)'")]
		public void GivenIAmViewingRealTimeAdherenceForSitesAndTeamsWithSkills(string sites, string teams, string skill)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSkillOnSitesAndTeams(idsForSites(sites), idsForTeams(teams), IdForSkill(skill));
		}

		[Given(@"I am viewing real time adherence on sites '(.*)' only")]
		[When(@"I am viewing real time adherence on site '(.*)'")]
		public void GivenIAmViewingRealTimeAdherenceOnSites(string sites)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForSites(idsForSites(sites));
		}

		[Given(@"I am viewing real time adherence on teams '(.*)'")]
		public void GivenIAmViewingRealTimeAdherenceOnTeams(string teams)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceForTeams(idsForTeams(teams));
		}

		[Given(@"I am viewing the rta tool")]
		public void WhenIAmViewingTheRtaTool()
		{
			TestControllerMethods.Logon();
			Navigation.GoToRtaTool();
		}

		[Given(@"I view real time adherence trace for '(.*)'")]
		public void GivenIViewRealTimeAdherenceTraceFor(string userCode)
		{
			TestControllerMethods.Logon();
			Navigation.GoToRtaTracer(userCode);
			Browser.Interactions.AssertAnyContains(".process", userCode);
		}

		[When(@"I view agent details view for agent '(.*)'")]
		public void WhenIViewAgentDetailsViewForAgent(string name)
		{
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GoToAgentDetails(personId);
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
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
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
			DataMaker.Data().ApplyAfterSetup(new GroupingReadOnlyUpdate());
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

		[When(@"I wait half second and refresh the request page")]
		public void WhenIWaitHalfSecondAndRefreshTheRequestPage()
		{
			Thread.Sleep(500);
			TestControllerMethods.Logon();
			Navigation.GotoRequests();
		}

		[When(@"I navigate to the team schedule")]
		[Then(@"I navigate to the team schedule")]
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
			Navigation.GotoGlobalSignInPage();
		}

		[When(@"I navigate to the site home page")]
		[When(@"I navigate to the site's root")]
		public void WhenINavigateToTheSiteSRoot()
		{
			Navigation.GotoSiteHomePage();
		}

		[When(@"I navigate to MyTime")]
		public void WhenINavigateToMyTime()
		{
			Navigation.GotoMyTime();
		}

		[When(@"I navigate to CiscoWidget")]
		public void WhenINavigateToCiscoWidget()
		{
			Navigation.GotoASMWidget();
		}

		[Given(@"I navigate to Anywhere")]
		[When(@"I navigate to Anywhere")]
		public void WhenINavigateToAnywhere()
		{
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
		
		[Given(@"I am viewing real time adherence for agents on team '(.*)'")]
		[When(@"I view real time adherence for agents on team '(.*)'")]
		public void WhenIViewRealTimeAdherenceForAgentsOnTeam(string team)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceAgentsOnTeam(IdForTeam(team));
		}

		[Given(@"I am viewing real time adherence for all agents on team '(.*)'")]
		[When(@"I view real time adherence for all agents on team '(.*)'")]
		public void WhenIViewRealTimeAdherenceForAllAgentsOnTeam(string team)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceAllAgentsOnTeam(IdForTeam(team));
		}



		[Given(@"I view real time adherence for all agents on entire Bu")]
		[When(@"I view real time adherence for all agents on entire Bu")]
		public void WhenIViewRealTimeAdherenceForAllAgentsOnEntireBu()
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceAgents();
		}

		[When(@"I am viewing real time adherence for agents without state '(.*)' on team '(.*)'")]
		public void WhenIAmViewingRealTimeAdherenceWithoutStateForAgentsOnTeam(string state, string team)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceAgentsWithoutStateOnTeam(IdForTeam(team), IdForState(state));
		}
		
		[When(@"I view real time adherence for agents with skills '(.*)'")]
		public void WhenIViewRealTimeAdherenceForAgentsWithSkills(string skill)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRealTimeAdherenceAllAgentsWithSkill(IdForSkill(skill));
		}
		
		[When(@"I view people")]
		[Given(@"I view people")]
		public void WhenIViewPeople()
		{
			TestControllerMethods.Logon();
			Navigation.GoToPeople();
		}

		[When(@"I view wfm requests")]
		public void WhenIViewWfmRequests()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmRequests();
		}

		[When(@"I view wfm team schedules")]
		public void WhenIViewWfmTeamSchedules()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmTeams();
		}

        [When(@"I view wfm reports")]
        public void WhenIViewWfmReports()
        {
            TestControllerMethods.Logon();
            Navigation.GoToWfmReports();
        }

		[When(@"I view wfm leader board report")]
		public void WhenIViewWfmLeaderBoardReport()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmLeaderBoardReports();
		}

		[When(@"I view outbound")]
		public void WhenIViewOutbound()
		{
			TestControllerMethods.Logon();
			Navigation.GoToOutbound();
		}

		[When(@"I view the outbound campaign creation page")]
		public void WhenIViewTheOutboundCampaignCreationPage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToOutboundCampaignCreation();
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
		[Given(@"I am viewing forecast page")]
		[When(@"I am viewing forecast page")]
		public void GivenIViewForecasting()
		{
			TestControllerMethods.Logon();
			Navigation.GoToForecasting();
		}

		[Given(@"I am viewing intraday page")]
		[When(@"I am viewing intraday page")]
		public void GivenIAmViewingIntradayPage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToIntraday();
		}

		[Given(@"I am viewing staffing page")]
		[When(@"I am viewing staffing page")]
		public void GivenIAmViewingStaffingPage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToStaffing();
		}

		[Given(@"I am viewing BPO exhange page")]
		[When(@"I am viewing BPO exhange page")]
		public void GivenIAmViewingBpoExchangePage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToBpoExchange();
		}

		[When(@"I am viewing copy schedule page")]
		public void GivenIViewCopySchedule()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmCopySchedule();
		}

		[When(@"I am viewing import schedule page")]
		public void WhenIAmViewingImportSchedulePage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmImportSchedule();
		}

		[When(@"I am viewing create planning group page")]
		public void WhenIAmViewingCreatePlanningGroupPage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmCreatePlanningGroup();
		}

		[When(@"I am viewing planning group list page")]
		public void WhenIAmViewingPlanningGroupListPage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToWfmListPlanningGroups();
		}

		[When(@"I view planning periods for planning group '(.*)'")]
		public void WhenIViewPlanningPeriodsForPlanningGroup(string planningGroupName)
		{
			TestControllerMethods.Logon();
			var planningGroupConfigurable = DataMaker.Data().UserDatasOfType<PlanningGroupConfigurable>().Single();
			Navigation.GoToPlanningPeriodListForPlanningGroup(planningGroupConfigurable.PlanningGroup.Id.GetValueOrDefault());
		}

		[When(@"I am viewing scheduling setting page for planning group '(.*)'")]
		public void WhenIViewSchedulingSettingPageForPlanningGroup(string planningGroupName)
		{
			TestControllerMethods.Logon();
			var planningGroupConfigurable = DataMaker.Data().UserDatasOfType<PlanningGroupSchedulingSettingConfigurable>().Single();
			Navigation.GoToSchedulingSettingPageForPlanningGroup(planningGroupConfigurable.PlanningGroup.Id.GetValueOrDefault());
		}

		[When(@"I am viewing create scheduling setting page for planning group '(.*)'")]
		public void WhenIViewCreateSchedulingSettingPageForPlanningGroup(string planningGroupName)
		{
			TestControllerMethods.Logon();
			var planningGroupConfigurable = DataMaker.Data().UserDatasOfType<PlanningGroupConfigurable>().Single();
			Navigation.GoToCreateSchedulingSettingPageForPlanningGroup(planningGroupConfigurable.PlanningGroup.Id.GetValueOrDefault());
		}

		[When(@"I am viewing edit scheduling setting page for planning group '(.*)'")]
		public void WhenIViewEditSchedulingSettingPageForPlanningGroup(string planningGroupName)
		{
			TestControllerMethods.Logon();
			var planningGroupSchedulingSettingConfigurable = DataMaker.Data().UserDatasOfType<PlanningGroupSchedulingSettingConfigurable>().Single();
			Navigation.GoToEditSchedulingSettingPageForPlanningGroup(planningGroupSchedulingSettingConfigurable.PlanningGroup.Id.GetValueOrDefault(), planningGroupSchedulingSettingConfigurable.PlanningGroupSchedulingSetting.Id.GetValueOrDefault());
		}
	}
}