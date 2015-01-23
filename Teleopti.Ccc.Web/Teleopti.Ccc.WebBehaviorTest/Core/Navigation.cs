using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Navigation
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Navigation));

		public static void GoToOtherPage(string pageUrl, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.URL, pageUrl), Browser.Interactions.GoTo, interceptors);
		}

		public static void GotoRaw(string url, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(url), Browser.Interactions.GoTo, interceptors);
		}

		private static void GoToApplicationPage(string pageUrl, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.URL, pageUrl), Browser.Interactions.GoTo, interceptors);
			CurrentTime.NavigatedToAnApplicationPage();
		}

		private static void InnerGoto(Uri url, Action<string> gotoAction, params IGoToInterceptor[] interceptors)
		{
			var args = new GotoArgs { Uri = url };

			interceptors.ToList().ForEach(i => i.Before(args));

			Browser.Interactions.DumpUrl(s => Log.Info("Am at: " + s));
			Log.Info("Browsing to: " + args.Uri);

			gotoAction.Invoke(args.Uri.ToString());

			interceptors.Reverse().ToList().ForEach(i => i.After(args));

			Browser.Interactions.DumpUrl(s => Log.Info("Ended up in: " + s));
		}

		public static void GotoAsm()
		{
			GoToApplicationPage("MyTime/Asm");
		}

		public static void GotoSiteHomePage()
		{
			GoToApplicationPage("", new ApplicationStartupTimeout(), new BustCache());
		}

		public static void GotoMyTime()
		{
			GoToApplicationPage("MyTime", new ApplicationStartupTimeout());
		}
		
		public static void GotoGlobalSignInPage()
		{
			GoToApplicationPage("Authentication", new ApplicationStartupTimeout());
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoToApplicationPage("MyTime/Schedule/Week", new ApplicationStartupTimeout());
		}

		public static void GotoAnApplicationPage()
		{
			GotoWeekSchedulePage();
		}

		public static void GotoWeekSchedulePage()
		{
			GoToApplicationPage("MyTime#Schedule/Week",
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoWeekSchedulePageNoWait()
		{
			GoToApplicationPage("MyTime#Schedule/Week",
								new ApplicationStartupTimeout());
		}

		public static void GotoWeekSchedulePage(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#Schedule/Week/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoAvailability()
		{
			GoToApplicationPage("MyTime#Availability/Index", new ApplicationStartupTimeout());
		}

		public static void GotoAvailability(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#Availability/Index/{0}/{1}/{2}",
												 date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				 new ApplicationStartupTimeout());
		}

		public static void GotoPreference()
		{
			GoToApplicationPage("MyTime#Preference/Index",
				new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoPreference(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#Preference/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoRegionalSettings()
		{
			GoToApplicationPage("MyTime#Settings/Index", new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoPasswordPage()
		{
			GotoWeekSchedulePage();
			GoToApplicationPage("MyTime#Settings/Password", new ApplicationStartupTimeout());
		}

		public static void GotoRequests()
		{
			GoToApplicationPage("MyTime#Requests/Index", new BustCache(), new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoTeamSchedule()
		{
			GoToApplicationPage("MyTime#TeamSchedule/Index", new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00"))
				, new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoTheInternet()
		{
			GotoRaw("about:blank");
			Browser.Interactions.AssertUrlContains("blank");
		}

		public static void GotoMessagePage()
		{
			GoToApplicationPage("MyTime#Message/Index", new ApplicationStartupTimeout());
		}

		public static void GoToPerformanceTool()
		{
			GoToApplicationPage("PerformanceTool", new ApplicationStartupTimeout());
		}

		public static void GoToPerformanceTool(string scenarioName)
		{
			GoToApplicationPage("PerformanceTool#" + HttpUtility.UrlEncode(scenarioName), new ApplicationStartupTimeout());
		}

		public static void GoToMessageBrokerTool()
		{
			GoToApplicationPage("MessageBrokerTool", new ApplicationStartupTimeout());
		}

		public static void GoToHealthCheck()
		{
			GoToApplicationPage("HealthCheck", new ApplicationStartupTimeout());
		}

		public static void GotoAnywhere()
		{
			GoToApplicationPage("Anywhere", new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid buId)
		{
			GoToApplicationPage(string.Format("Anywhere#teamschedule/{0}/{1}{2}{3}",
				buId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid teamId, Guid buId)
		{
			GoToApplicationPage(
				string.Format("Anywhere#teamschedule/{0}/{1}/{2}{3}{4}",
					buId,
					teamId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")),
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonSchedule(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToApplicationPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")
					),
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleFullDayAbsenceForm(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToApplicationPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addfulldayabsence",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")),
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleIntradayAbsenceForm(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToApplicationPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addintradayabsence",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")),
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleAddActivityForm(Guid buId, Guid personId, Guid groupId, DateTime date)
		{
			GoToApplicationPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addactivity",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")),
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleMoveActivityForm(Guid buId, Guid personId, Guid groupId, DateTime date, string minutes)
		{
			GoToApplicationPage(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/moveactivity/{6}",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00"),
					minutes),
				new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeAdherenceOverview(bool waitUntilSubscriptionIsCompleted)
		{
			if (waitUntilSubscriptionIsCompleted)
			{
				GoToApplicationPage(
					"Anywhere#realtimeadherencesites",
					new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
			}
			else
			{
				GoToApplicationPage(
				"Anywhere#realtimeadherencesites",
				new ApplicationStartupTimeout());
			}
			
		}

		public static void GotoAnywhereRealTimeAdherenceOverview(Guid buId, Guid siteId)
		{
			GoToApplicationPage(
				string.Format("Anywhere#realtimeadherenceteams/{0}/{1}",buId, siteId),
				new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeManageAdherenceOverview(Guid buId, Guid personId)
		{
			GoToApplicationPage(
				string.Format("Anywhere#manageadherence/{0}/{1}",buId, personId),
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereRealTimeAdherenceTeamOverview(Guid buId, Guid idForTeam)
		{
			GoToApplicationPage(
				string.Format("Anywhere#realtimeadherenceagents/{0}/{1}",buId, idForTeam),
				new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeAdherenceTeamOverviewNoWait(Guid buId, Guid idForTeam)
		{
			GoToApplicationPage(
				string.Format("Anywhere#realtimeadherenceagents/{0}/{1}", buId, idForTeam),
				new ApplicationStartupTimeout());
		}

		public static void GoToMyReport()
		{
			GoToApplicationPage("MyTime#MyReport/Index", new ApplicationStartupTimeout());
		}

		public static void GoToMyDetailedAdherence(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#MyReport/Adherence/{0}/{1}/{2}",
					date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
					new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoMonthSchedulePage(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#Schedule/Month/{0}/{1}/{2}",
					date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
					new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoMonthSchedulePage()
		{
			GoToApplicationPage("MyTime#Schedule/Month",
					new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GoToMyReport(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#MyReport/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout());
		}

		public static void GotoMobileWeekSchedulePageNoWait()
		{
			GoToApplicationPage("MyTime#Schedule/MobileWeek", new ApplicationStartupTimeout());
		}

		public static void GotoMobileWeekSchedulePage()
		{
			GoToApplicationPage("MyTime#Schedule/MobileWeek", new ApplicationStartupTimeout());
		}

		public static void GotoMobileWeekSchedulePage(DateTime date)
		{
			GoToApplicationPage(string.Format("MyTime#Schedule/MobileWeek/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

	    public static void GoToMyQueueMetrics(DateTime date)
	    {
            GoToApplicationPage(string.Format("MyTime#MyReport/QueueMetrics/{0}/{1}/{2}",
                    date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
                    new ApplicationStartupTimeout());
	    }

		public static void GotoQuickForecaster()
		{
			GoToApplicationPage("areas/wfm", new ApplicationStartupTimeout());
		}

		public static void GotoMessageTool(IEnumerable<Guid> ids)
		{
			GoToApplicationPage(string.Format("Messages?ids={0}",
				String.Join(",", ids.Select(x => x.ToString()).ToArray())),
				new ApplicationStartupTimeout());
		}

		public static void GoToLeaderboardReport()
		{
			GoToApplicationPage("MyTime#BadgeLeaderBoardReport/Index", new ApplicationStartupTimeout());
		}

		public static void GoToRtaTool()
		{
			GoToApplicationPage("RtaTool", new ApplicationStartupTimeout());
		}
	}

	public class WaitUntilSubscriptionIsCompleted : IGoToInterceptor
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			Browser.Interactions.AssertExists("[data-subscription-done]");
		}
	}

	public class GotoArgs
	{
		public Uri Uri { get; set; }
	}

	public interface IGoToInterceptor
	{
		void Before(GotoArgs args);
		void After(GotoArgs args);
	}

	public class BustCache : IGoToInterceptor
	{
		public void Before(GotoArgs args)
		{
			var url = args.Uri.ToString();
			if (url.Contains("?"))
				url = url.Replace("?", string.Format("?{0}&", Guid.NewGuid()));
			else if (url.Contains("#"))
				url = url.Replace("#", string.Format("?{0}#", Guid.NewGuid()));
			else
				url = string.Concat(url, string.Format("?{0}", Guid.NewGuid()));
			args.Uri = new Uri(url);
		}

		public void After(GotoArgs args)
		{
		}
	}

	public class ApplicationStartupTimeout : IGoToInterceptor
	{
		private IDisposable _timeoutScope;

		public void Before(GotoArgs args)
		{
			_timeoutScope = Browser.TimeoutScope(TimeSpan.FromSeconds(60));
		}

		public void After(GotoArgs args)
		{
			_timeoutScope.Dispose();
			_timeoutScope = null;
		}
	}

	public class WaitUntilReadyForInteraction : IGoToInterceptor
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			TestControllerMethods.WaitUntilReadyForInteraction();
		}
	}

	public class WaitUntilCompletelyLoaded : IGoToInterceptor
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			TestControllerMethods.WaitUntilCompletelyLoaded();
		}
	}

}