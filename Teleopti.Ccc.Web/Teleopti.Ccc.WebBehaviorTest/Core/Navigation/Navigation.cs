using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public static class Navigation
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Navigation));

		private static readonly Dictionary<Predicate<string>, INavigationInterceptor> interceptors = new Dictionary<Predicate<string>, INavigationInterceptor>();
		private static IEnumerable<INavigationInterceptor> _currentInterceptors = new INavigationInterceptor[] { };

		static Navigation()
		{
			interceptors.Add(u => !u.Contains("Test/"), new EndSetupPhase());
			interceptors.Add(u => true, new ApplicationStartupTimeout());
			interceptors.Add(u => u == "", new BustCache());
			interceptors.Add(u => u == "MyTime#Requests/Index", new BustCache());

			interceptors.Add(u => u.Contains("/MyTime/Asm"), new FakeTimeUsingMyTimeMethod());
			interceptors.Add(u => u.Contains("/MyTime#Schedule/Week"), new FakeTimeUsingMyTimeMethod());
//			interceptors.Add(u => u.Contains("/wfm/#/rta/agents"), new FakeClientTimeUsingSinonProvenWay());

			interceptors.Add(u => u.Contains("/wfm/#/rta"), new WaitUntilHangfireQueueIsProcessed());
		}

		public static void ReapplyFakeTime()
		{
			var fakeClientTimeMethods = from i in _currentInterceptors where i is IFakeClientTimeMethod select i as IFakeClientTimeMethod;
			fakeClientTimeMethods.ForEach(i => i.Apply());
		}

		private static void buildCurrentInterceptors(Uri url, IEnumerable<INavigationInterceptor> passed)
		{
			var matched = from i in interceptors where i.Key(url.ToString()) select i.Value;
			_currentInterceptors = matched.Concat(passed).ToList();
		}

		public static void GoToPage(string pageUrl, params INavigationInterceptor[] interceptors)
		{
			innerGoto(new Uri(TestSiteConfigurationSetup.URL, pageUrl), interceptors);
		}

		public static void GotoRaw(string url, params INavigationInterceptor[] interceptors)
		{
			innerGoto(new Uri(url), interceptors);
		}

		private static bool _nested;

		private static void innerGoto(Uri url, params INavigationInterceptor[] interceptors)
		{
			var args = new GotoArgs {Uri = url};

			if (_nested)
			{
				Browser.Interactions.GoTo(args.Uri.ToString());
				return;
			}

			_nested = true;
			try
			{
				buildCurrentInterceptors(url, interceptors);

				_currentInterceptors.ForEach(i => i.Before(args));

				Browser.Interactions.DumpUrl(s => Log.Info("Am at: " + s));
				Log.Info("Browsing to: " + args.Uri);

				Browser.Interactions.GoTo(args.Uri.ToString());

				_currentInterceptors.Reverse().ForEach(i => i.After(args));

				Browser.Interactions.DumpUrl(s => Log.Info("Ended up in: " + s));
			}
			finally
			{
				_nested = false;
			}
		}

		public static void GotoAsm()
		{
			GoToPage("MyTime/Asm");
		}

		public static void GotoSiteHomePage()
		{
			GoToPage("");
		}

		public static void GotoMyTime()
		{
			GoToPage("MyTime");
		}

		public static void GotoASMWidget()
		{
			GoToPage("MyTime/ASMWidget");
		}

		public static void GotoGlobalSignInPage()
		{
			GoToPage("Authentication");
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoToPage("MyTime/Schedule/Week");
		}

		public static void GotoAnApplicationPage()
		{
			GotoWeekSchedulePage();
			Browser.Interactions.AssertUrlContains("MyTime#Schedule/Week");
		}

		public static void GotoWeekSchedulePage()
		{
			GoToPage("MyTime#Schedule/Week", new WaitUntilCompletelyLoaded());
		}

		public static void GotoWeekSchedulePageNoWait()
		{
			GoToPage("MyTime#Schedule/Week");
		}

		public static void GotoWeekSchedulePage(DateTime date)
		{
			GoToPage($"MyTime#Schedule/Week/{date.Year:0000}/{date.Month:00}/{date.Day:00}",
				new WaitUntilCompletelyLoaded());
		}

		public static void GotoAvailability()
		{
			GoToPage("MyTime#Availability/Index");
		}

		public static void GotoAvailability(DateTime date)
		{
			GoToPage($"MyTime#Availability/Index/{date.Year:0000}/{date.Month:00}/{date.Day:00}");
		}

		public static void GotoPreference()
		{
			GoToPage("MyTime#Preference/Index", new WaitUntilReadyForInteraction());
		}

		public static void GotoPreference(DateTime date)
		{
			GoToPage($"MyTime#Preference/Index/{date.Year:0000}/{date.Month:00}/{date.Day:00}",
				new WaitUntilReadyForInteraction());
		}

		public static void GotoRegionalSettings()
		{
			GoToPage("MyTime#Settings/Index", new WaitUntilReadyForInteraction());
		}

		public static void GotoPasswordPage()
		{
			GoToPage("MyTime#Settings/Password", new WaitUntilReadyForInteraction());
		}

		public static void GotoRequests()
		{
			GoToPage("MyTime#Requests/Index", new WaitUntilReadyForInteraction());
		}

		public static void GotoTeamSchedule()
		{
			GoToPage("MyTime#TeamSchedule/Index", new WaitUntilReadyForInteraction());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoToPage(
				$"MyTime#TeamSchedule/Index/{date.Year:0000}/{date.Month:00}/{date.Day:00}"
				, new WaitUntilReadyForInteraction());
		}

		public static void GotoTheInternet()
		{
			GotoRaw("about:blank");
			Browser.Interactions.AssertUrlContains("blank");
		}

		public static void GotoMessagePage()
		{
			GoToPage("MyTime#Message/Index");
		}

		public static void GoToPerformanceTool()
		{
			GoToPage("PerformanceTool");
		}

		public static void GoToPerformanceTool(string scenarioName)
		{
			GoToPage("PerformanceTool#" + HttpUtility.UrlEncode(scenarioName));
		}

		public static void GoToMessageBrokerTool()
		{
			GoToPage("MessageBrokerTool");
		}

		public static void GoToHealthCheck()
		{
			GoToPage("HealthCheck");
		}

		public static void GotoAnywhere()
		{
			GoToPage("Anywhere");
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid buId)
		{
			GoToPage(string.Format("Anywhere#teamschedule/{0}/{1}{2}{3}",
				buId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")));
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid teamId, Guid buId)
		{
			GoToPage(
				string.Format("Anywhere#teamschedule/{0}/{1}/{2}{3}{4}",
					buId,
					teamId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")));
		}

		public static void GotoAnywherePersonSchedule(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")));
		}

		public static void GotoAnywherePersonScheduleFullDayAbsenceForm(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addfulldayabsence",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")));
		}

		public static void GotoAnywherePersonScheduleIntradayAbsenceForm(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addintradayabsence",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")));
		}

		public static void GotoAnywherePersonScheduleAddActivityForm(Guid buId, Guid personId, Guid groupId, DateTime date)
		{
			GoToPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addactivity",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")));
		}

		public static void GotoAnywherePersonScheduleMoveActivityForm(Guid buId, Guid personId, Guid groupId, DateTime date, string minutes)
		{
			GoToPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/moveactivity/{6}",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00"),
					minutes));
		}

		public static void GotoPermissions()
		{
			GoToPage("wfm/#/permissions", new WaitUntilLoaded());
		}

		public static void GotoPermissionsNew()
		{
			GoToPage("wfm/#/permissions?open", new WaitUntilLoaded());
		}

		public static void GotoSeatMap()
		{
			GoToPage("wfm/#/seatMap");
		}

		public static void GotoSeatPlan(DateTime? date)
		{
			if (date.HasValue)
			{
				GoToPage("wfm/#/seatPlan/" + date.Value.ToString("yyyy-MM-dd"));
			}
			else
			{
				GoToPage("wfm/#/seatPlan");
			}
		}

		public static void GoToMyReport()
		{
			GoToPage("MyTime#MyReport/Index");
		}

		public static void GoToMyDetailedAdherence(DateTime date)
		{
			GoToPage(string.Format("MyTime#MyReport/Adherence/{0}/{1}/{2}",
					date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new WaitUntilCompletelyLoaded());
		}

		public static void GotoMonthSchedulePage(DateTime date)
		{
			GoToPage(string.Format("MyTime#Schedule/Month/{0}/{1}/{2}",
					date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new WaitUntilCompletelyLoaded());
		}

		public static void GotoMobileDaySchedulePage(DateTime date)
		{
			GoToPage(string.Format("MyTime#Schedule/MobileDay/{0}/{1}/{2}",
					date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new WaitUntilCompletelyLoaded());
		}

		public static void GotoMonthSchedulePage()
		{
			GoToPage("MyTime#Schedule/Month",
				new WaitUntilCompletelyLoaded());
		}

		public static void GoToMyReport(DateTime date)
		{
			GoToPage(string.Format("MyTime#MyReport/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")));
		}

		public static void GoToMyQueueMetrics(DateTime date)
		{
			GoToPage(string.Format("MyTime#MyReport/QueueMetrics/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")));
		}

		public static void GotoMessageTool(IEnumerable<Guid> ids)
		{
			GoToPage(string.Format("Messages#{0}",
				String.Join(",", ids.Select(x => x.ToString()).ToArray())));
		}

		public static void GoToLeaderboardReport()
		{
			GoToPage("MyTime#BadgeLeaderBoardReport/Index");
		}

		public static void GoToRtaTool()
		{
			GoToPage("wfm/#/rtaTool");
		}

		public static void GoToRtaTracer(string userCode)
		{
			GoToPage($"wfm/#/rtaTracer?userCode={userCode}&trace");
		}

		public static void GoToWfmLandingPage()
		{
			GoToPage("wfm");
		}

		public static void GoToPeople()
		{
			GoToPage("wfm/#/people");
		}

		public static void GoToOutbound()
		{
			GoToPage("wfm/#/outbound");
		}

		public static void GoToOutboundCampaignCreation()
		{
			GoToPage("wfm/#/outbound/create");
		}

		public static void GoToOutboundCampaign(Guid id)
		{
			GoToPage("wfm/#/outbound/campaign/" + id);
		}

		public static void GoToForecasting()
		{
			GoToPage("wfm/#/forecast");
		}

		public static void GoToIntraday()
		{
			GoToPage("wfm/#/intraday");
		}

		public static void GotoRealTimeAdherenceOverview()
		{
			GoToPage("wfm/#/rta");
		}

		public static void GotoRealTimeAdherenceOverviewForTeamsOnSite(Guid siteId)
		{
			GoToPage("wfm/#/rta/teams/?siteIds=" + siteId + "&pollingInterval=100&open");
		}

		public static void GotoRealTimeAdherenceForSkillForTeamsOnSite(Guid skillId, Guid siteId)
		{
			GoToPage("wfm/#/rta/teams/?siteIds=" + siteId + "&skillIds=" + skillId + "&pollingInterval=100&open");
		}

		public static void GotoRealTimeAdherenceOverviewForSkillForSites(Guid skillId)
		{
			GoToPage("wfm/#/rta/?skillIds=" + skillId + "&pollingInterval=100&open");
		}

		public static void GotoRealTimeAdherenceOverviewForSkillAreaForSites(Guid skillAreaId)
		{
			GoToPage("wfm/#/rta/?skillAreaId=" + skillAreaId + "&pollingInterval=100&open");
		}

		public static void GotoRealTimeAdherenceOverviewForSkillAreaForTeamsOnSite(Guid skillAreaId, Guid siteId)
		{
			GoToPage("wfm/#/rta/teams/?siteIds=" + siteId + "&skillAreaId=" + skillAreaId + "&pollingInterval=100&open");
		}

		public static void GotoRealTimeAdherenceAgents()
		{
			GoToPage("wfm/#/rta/agents/?pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceAgentsForSites(IEnumerable<Guid> siteIds)
		{
			var sites = string.Join("&siteIds=", siteIds);
			GoToPage("wfm/#/rta/agents/?siteIds=" + sites + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceAgentsForTeams(IEnumerable<Guid> teamIds)
		{
			var teams = string.Join("&teamIds=", teamIds);
			GoToPage("wfm/#/rta/agents/?teamIds=" + teams + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceAgentsOnTeam(Guid teamId)
		{
			GoToPage("wfm/#/rta/agents/?teamIds=" + teamId + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceAllAgentsOnTeam(Guid teamId)
		{
			GoToPage("wfm/#/rta/agents/?teamIds=" + teamId + "&showAllAgents=true" + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceAgentsWithoutStateOnTeam(Guid teamId, Guid state)
		{
			GoToPage("wfm/#/rta/agents/?teamIds=" + teamId + "&es=" + state + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceAllAgentsWithSkill(Guid skillId)
		{
			GoToPage("wfm/#/rta/agents/?skillIds=" + skillId + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceForSkillOnTeams(IEnumerable<Guid> teamIds, Guid skillId)
		{
			var teamString = string.Join("&teamIds=", teamIds);
			GoToPage("wfm/#/rta/agents/?teamIds=" + teamString + "&skillIds=" + skillId + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceForSkillAreaOnTeams(IEnumerable<Guid> teamIds, Guid idForSkillArea)
		{
			var teamString = string.Join("&teamIds=", teamIds);
			GoToPage("wfm/#/rta/agents/?teamIds=" + teamString + "&idForSkillArea=" + idForSkillArea + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceForSkillOnSites(IEnumerable<Guid> siteIds, Guid skillId)
		{
			var sites = string.Join("&siteIds=", siteIds);
			GoToPage("wfm/#/rta/agents/?siteIds=" + sites + "&skillIds=" + skillId + "&pollingInterval=100");
		}

		public static void GotoRealTimeAdherenceForSkillAreaOnSites(IEnumerable<Guid> siteIds, Guid skillAreaId)
		{
			var sites = string.Join("&siteIds=", siteIds);
			GoToPage("wfm/#/rta/agents/?siteIds=" + sites + "&skillAreaId=" + skillAreaId + "&pollingInterval=100");
		}


		public static void GotoRealTimeAdherenceForSkillOnSitesAndTeams(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, Guid skillId)
		{
			var sites = string.Join("&siteIds=", siteIds);
			var teams = string.Join("&teamIds=", teamIds);
			GoToPage("wfm/#/rta/agents/?siteIds=" + sites + "&teamIds=" + teams + "&skillIds=" + skillId + "&pollingInterval=100");
		}

		public static void GoToAgentDetails(Guid personId)
		{
			GoToPage("wfm/#/rta/agent-details/" + personId);
		}

		public static void GoToAgentHistoricalAdherence(Guid personId)
		{
			GoToPage($"wfm/#/rta/agent-historical/{personId}?open=true");
		}

		public static void GoToAgentHistoricalAdherence(Guid personId, DateTime date)
		{
			GoToPage($"wfm/#/rta/agent-historical/{personId}/{date:yyyyMMdd}?open=true");
		}

		public static void GotoPageCiscoFinesse()
		{
			GoToPage("MyTime/ASMWidget");
		}

		public static void GoToWfmRequests()
		{
			GoToPage("wfm/#/requests");
		}

		public static void GoToWfmGamification()
		{
			GoToPage("wfm/#/gamification.setting");
		}

		public static void GoToWfmTeams()
		{
			GoToPage("wfm/#/teams");
		}

		public static void GoToWfmReports()
		{
			GoToPage("wfm/#/reports");
		}

		public static void GoToWfmLeaderBoardReports()
		{
			GoToPage("wfm/#/reports/leaderboard");
		}

		public static void GoToStaffing()
		{
			GoToPage("wfm/#/staffing");
		}

		public static void GoToBpoExchange()
		{
			GoToPage("wfm/#/bpo");
		}


		public static void GotoResourcePlanner()
		{
			GoToPage("wfm/#/resourceplanner/planningperiods");
		}

		public static void GoToWfmCopySchedule()
		{
			GoToPage("wfm/#/resourceplanner/copyschedule");
		}

		public static void GoToWfmImportSchedule()
		{
			GoToPage("wfm/#/resourceplanner/importschedule");
		}

		public static void GoToWfmCreatePlanningGroup()
		{
			GoToPage("wfm/#/resourceplanner/createplanninggroup");
		}

		public static void GoToWfmListPlanningGroups()
		{
			GoToPage("wfm/#/resourceplanner");
		}

		public static void GoToPlanningPeriodListForPlanningGroup(Guid planningGroupId)
		{
			GoToPage($"wfm/#/resourceplanner/planninggroup/{planningGroupId}/selectplanningperiod");
		}

		public static void GoToSchedulingSettingPageForPlanningGroup(Guid planningGroupId)
		{
			GoToPage($"wfm/#/resourceplanner/planninggroup/{planningGroupId}/settings");
		}

		public static void GoToCreateSchedulingSettingPageForPlanningGroup(Guid planningGroupId)
		{
			GoToPage($"wfm/#/resourceplanner/planninggroup/{planningGroupId}/setting/");
		}

		public static void GoToEditSchedulingSettingPageForPlanningGroup(Guid planningGroupId, Guid planningGroupSchedulingSettingId)
		{
			GoToPage($"wfm/#/resourceplanner/planninggroup/{planningGroupId}/setting/{planningGroupSchedulingSettingId}");
		}

		public static void GoToHistoricalOverview(Guid teamId)
		{
			GoToPage($"wfm/#/rta/historical-overview?teamIds={teamId}");
		}

		public static void GoToResetPassword(string token)
		{
			GoToPage($"wfm/reset_password.html?t={token}");
		}
	}
}