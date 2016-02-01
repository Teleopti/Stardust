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

		private static readonly Dictionary<Predicate<string>, INavigationInterceptor> _interceptors = new Dictionary<Predicate<string>, INavigationInterceptor>();
		private static IEnumerable<INavigationInterceptor> _currentInterceptors = new INavigationInterceptor[] { };

		static Navigation()
		{
			_interceptors.Add(u => true, new ApplicationStartupTimeout());
			_interceptors.Add(u => u == "", new BustCache());
			_interceptors.Add(u => u == "MyTime#Requests/Index", new BustCache());

			_interceptors.Add(u => u.Contains("/Anywhere#realtimeadherenceagents"), new FakeClientTimeUsingSinonProvenWay());
			_interceptors.Add(u => u.Contains("/Anywhere#teamschedule"), new FakeClientTimeUsingSinonProvenWay());
			_interceptors.Add(u => u.Contains("/Anywhere#personschedule"), new FakeClientTimeUsingSinonProvenWay());
			_interceptors.Add(u => u.Contains("/Anywhere#realtimeadherence"), new FakeClientTimeUsingSinonProvenWay());
			_interceptors.Add(u => u.Contains("/MyTime/Asm"), new FakeTimeUsingMyTimeMethod());
			_interceptors.Add(u => u.Contains("/MyTime#Schedule/Week"), new FakeTimeUsingMyTimeMethod());
			_interceptors.Add(u => u.Contains("/wfm/#/rta/agents"), new FakeClientTimeUsingSinonProvenWay());

			_interceptors.Add(u => u.Contains("/Anywhere#realtimeadherence"), new WaitUntilHangfireQueueIsProcessed());
			_interceptors.Add(u => u.Contains("/Anywhere#manageadherence"), new WaitUntilHangfireQueueIsProcessed());
			_interceptors.Add(u => u.Contains("/wfm/#/rta"), new WaitUntilHangfireQueueIsProcessed());
		}

		public static void ReapplyFakeTime()
		{
			var fakeClientTimeMethods = from i in _currentInterceptors where i is IFakeClientTimeMethod select i as IFakeClientTimeMethod;
			fakeClientTimeMethods.ForEach(i => i.Apply());
		}

		private static void BuildCurrentInterceptors(Uri url, IEnumerable<INavigationInterceptor> passed)
		{
			var matched = from i in _interceptors where i.Key(url.ToString()) select i.Value;
			_currentInterceptors = matched.Concat(passed).ToList();
		}

		public static void GoToPage(string pageUrl, params INavigationInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.URL, pageUrl), interceptors);
		}

		public static void GotoRaw(string url, params INavigationInterceptor[] interceptors)
		{
			InnerGoto(new Uri(url), interceptors);
		}

		private static bool _nested = false;

		private static void InnerGoto(Uri url, params INavigationInterceptor[] interceptors)
		{
			var args = new GotoArgs { Uri = url };

			if (_nested)
			{
				Browser.Interactions.GoTo(args.Uri.ToString());
				return;
			}

			_nested = true;

			BuildCurrentInterceptors(url, interceptors);

			_currentInterceptors.ForEach(i => i.Before(args));

			Browser.Interactions.DumpUrl(s => Log.Info("Am at: " + s));
			Log.Info("Browsing to: " + args.Uri);

			Browser.Interactions.GoTo(args.Uri.ToString());

			_currentInterceptors.Reverse().ForEach(i => i.After(args));

			Browser.Interactions.DumpUrl(s => Log.Info("Ended up in: " + s));
			
			_nested = false;
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
			GoToPage(string.Format("MyTime#Schedule/Week/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				 new WaitUntilCompletelyLoaded());
		}

		public static void GotoAvailability()
		{
			GoToPage("MyTime#Availability/Index");
		}

		public static void GotoAvailability(DateTime date)
		{
			GoToPage(string.Format("MyTime#Availability/Index/{0}/{1}/{2}", date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")));
		}

		public static void GotoPreference()
		{
			GoToPage("MyTime#Preference/Index", new WaitUntilReadyForInteraction());
		}

		public static void GotoPreference(DateTime date)
		{
			GoToPage(string.Format("MyTime#Preference/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
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
			GoToPage(string.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00"))
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
					date.Day.ToString("00")
					), new WaitUntilSubscriptionIsCompleted());
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
					minutes),
				 new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeAdherenceOverview(bool waitUntilSubscriptionIsCompleted)
		{
			if (waitUntilSubscriptionIsCompleted)
			{
				GoToPage(
					"Anywhere#realtimeadherencesites",
					 new WaitUntilSubscriptionIsCompleted());
			}
			else
			{
				GoToPage("Anywhere#realtimeadherencesites");
			}
		}

		public static void GotoResourcePlanner()
		{
			GoToPage("wfm/#/resourceplanner");
		}

		public static void GotoPermissions()
		{
			GoToPage("wfm/#/permissions", new WaitUntilLoaded());
		}

		public static void GotoSeatMap()
		{
			GoToPage("wfm/#/seatMap");
		}

		public static void GotoSeatPlan(DateTime? date)
		{
			if (date.HasValue)
			{
				GoToPage("wfm/#/seatPlan/" + date.Value.ToString("yyyy-MM-dd"), new WaitUntilLoaded());
			}
			else
			{
				GoToPage("wfm/#/seatPlan", new WaitUntilLoaded());
			}
		}

		public static void GotoAnywhereRealTimeAdherenceOverview(Guid buId, Guid siteId)
		{
			GoToPage(
				string.Format("Anywhere#realtimeadherenceteams/{0}/{1}", buId, siteId),
				 new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeManageAdherenceOverview(Guid buId, Guid personId)
		{
			GoToPage(string.Format("Anywhere#manageadherence/{0}/{1}", buId, personId));
		}

		public static void GotoAnywhereRealTimeAdherenceTeamOverview(Guid buId, Guid idForTeam)
		{
			GoToPage(string.Format("Anywhere#realtimeadherenceagents/{0}/{1}", buId, idForTeam),
				 new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeAdherenceTeamOverviewNoWait(Guid buId, Guid idForTeam)
		{
			GoToPage(
				string.Format("Anywhere#realtimeadherenceagents/{0}/{1}", buId, idForTeam));
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

		public static void GotoMobileWeekSchedulePageNoWait()
		{
			GoToPage("MyTime#Schedule/MobileWeek");
		}

		public static void GotoMobileWeekSchedulePage()
		{
			GoToPage("MyTime#Schedule/MobileWeek");
		}

		public static void GotoMobileWeekSchedulePage(DateTime date)
		{
			GoToPage(string.Format("MyTime#Schedule/MobileWeek/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				 new WaitUntilCompletelyLoaded());
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
			GoToPage("RtaTool");
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
			GoToPage("wfm/#/outbound/campaign/" + id );
		}

		public static void GoToForecasting()
		{
			GoToPage("wfm/#/forecasting");
		}

		public static void GoToIntraday()
		{
			GoToPage("wfm/#/intraday");
		}

		public static void GotoRealTimeAdherenceSites()
		{
			GoToPage("wfm/#/rta");
		}

		public static void GotoRealTimeAdherenceForTeamsOnSite(Guid buId, Guid siteId)
		{
			GoToPage("wfm/#/rta/teams/" + siteId);
		}

		public static void GotoRealTimeAdherenceAgentsOnTeam(Guid businessUnitId, Guid siteId, Guid teamId)
		{
			GoToPage("wfm/#/rta/agents/" + siteId + "/" + teamId);
		}

		public static void GotoRealTimeAdherenceAllAgentsOnTeam(Guid businessUnitId, Guid siteId, Guid teamId)
		{
			GoToPage("wfm/#/rta/agents/" + siteId + "/" + teamId + "?showAllAgents=true");
		}
		
		public static void GoToAgentDetails(Guid personId)
		{
			GoToPage("wfm/#/rta/agent-details/" + personId);
		}

		public static void GotoPageCiscoFinesse()
		{
			GoToPage("MyTime/CiscoWidget");
		}

		public static void GoToWfmRequests()
		{
			GoToPage("wfm/#/requests");
		}

		public static void GoToWfmTeamSchedule()
		{
			GoToPage("wfm/#/teamSchedule");
		}
	}
}