using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.WebBehaviorTest.Data;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Navigation
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Navigation));

		public static void GoToWaitForUrlAssert(string pageUrlAndAssertUrl, params IGoToInterceptor[] interceptors)
		{
			GoToWaitForUrlAssert(pageUrlAndAssertUrl, pageUrlAndAssertUrl, interceptors);
		}

		public static void GoToWaitForUrlAssert(string pageUrl, string assertUrlContains, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.URL, pageUrl),
				s => Browser.Interactions.GoToWaitForUrlAssert(s, assertUrlContains),
				interceptors);
		}

		/// <summary>
		/// Dont use this method! 
		/// If you use good robustness practices this method is not required, and the result is more stable!
		/// </summary>
		/// <param name="uri"></param>
		public static void GoToWaitForCompleted(string pageUrl, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.URL, pageUrl), Browser.Interactions.GoToWaitForCompleted, interceptors);
		}

		public static void GotoRaw(string url, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(url), Browser.Interactions.GoToWaitForCompleted, interceptors);
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
			GoToWaitForCompleted("MyTime/Asm");
		}

		public static void GotoSiteHomePage()
		{
			GoToWaitForCompleted("", new ApplicationStartupTimeout(), new BustCache());
		}

		public static void GotoMyTime()
		{
			GoToWaitForCompleted("MyTime", new ApplicationStartupTimeout());
		}
		
		public static void GotoGlobalSignInPage()
		{
			GoToWaitForCompleted("Authentication", new ApplicationStartupTimeout());
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoToWaitForCompleted("MyTime/Schedule/Week", new ApplicationStartupTimeout());
		}

		public static void GotoAnApplicationPage()
		{
			GotoWeekSchedulePage();
		}

		public static void GotoWeekSchedulePage()
		{
			GoToWaitForCompleted("MyTime#Schedule/Week",
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoWeekSchedulePageNoWait()
		{
			GoToWaitForCompleted("MyTime#Schedule/Week",
								new ApplicationStartupTimeout());
		}

		public static void GotoWeekSchedulePage(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Schedule/Week/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoAvailability()
		{
			GoToWaitForCompleted("MyTime#Availability/Index", new ApplicationStartupTimeout());
		}

		public static void GotoAvailability(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Availability/Index/{0}/{1}/{2}",
												 date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				 new ApplicationStartupTimeout());
		}

		public static void GotoPreference()
		{
			GoToWaitForCompleted("MyTime#Preference/Index",
				new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoPreference(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Preference/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoRegionalSettings()
		{
			GoToWaitForCompleted("MyTime#Settings/Index", new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoPasswordPage()
		{
			GotoWeekSchedulePage();
			GoToWaitForUrlAssert("MyTime#Settings/Password", "Settings/Password", new ApplicationStartupTimeout());
		}

		public static void GotoRequests()
		{
			GoToWaitForCompleted("MyTime#Requests/Index", new BustCache(), new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoTeamSchedule()
		{
			GoToWaitForCompleted("MyTime#TeamSchedule/Index", new ApplicationStartupTimeout(), new WaitUntilReadyForInteraction());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}",
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
			GoToWaitForCompleted("MyTime#Message/Index", new ApplicationStartupTimeout());
		}

		public static void GoToPerformanceTool()
		{
			GoToWaitForUrlAssert("PerformanceTool", new ApplicationStartupTimeout());
		}

		public static void GoToPerformanceTool(string scenarioName)
		{
			GoToWaitForUrlAssert("PerformanceTool#" + HttpUtility.UrlEncode(scenarioName), new ApplicationStartupTimeout());
		}

		public static void GoToMessageBrokerTool()
		{
			GoToWaitForUrlAssert("MessageBrokerTool", new ApplicationStartupTimeout());
		}

		public static void GoToHealthCheck()
		{
			GoToWaitForUrlAssert("HealthCheck", new ApplicationStartupTimeout());
		}

		public static void GotoAnywhere()
		{
			GoToWaitForUrlAssert("Anywhere", new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid buId)
		{
			GoToWaitForUrlAssert(string.Format("Anywhere#teamschedule/{0}/{1}{2}{3}",
				buId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				"Anywhere#teamschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid teamId, Guid buId)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#teamschedule/{0}/{1}/{2}{3}{4}",
				buId,
				teamId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				"Anywhere#teamschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonSchedule(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}",
					buId,
					groupId,
					personId,
					date.Year.ToString("0000"),
					date.Month.ToString("00"),
					date.Day.ToString("00")
					),
				"Anywhere#personschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleFullDayAbsenceForm(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addfulldayabsence",
				buId,
				groupId,
				personId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				"Anywhere#personschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleIntradayAbsenceForm(Guid buId, Guid groupId, Guid personId, DateTime date)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addintradayabsence",
				buId,
				groupId,
				personId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				"Anywhere#personschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleAddActivityForm(Guid buId, Guid personId, Guid groupId, DateTime date)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/addactivity",
				buId,
				groupId,
				personId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				"Anywhere#personschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleMoveActivityForm(Guid buId, Guid personId, Guid groupId, DateTime date, string minutes)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#personschedule/{0}/{1}/{2}/{3}{4}{5}/moveactivity/{6}",
				buId,
				groupId,
				personId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00"),
				minutes),
				"Anywhere#personschedule",
				new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeAdherenceOverview(bool waitUntilSubscriptionIsCompleted)
		{
			if (waitUntilSubscriptionIsCompleted)
			{
				GoToWaitForUrlAssert(
					"Anywhere#realtimeadherencesites",
					"Anywhere#realtimeadherencesites",
					new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
			}
			else
			{
				GoToWaitForUrlAssert(
				"Anywhere#realtimeadherencesites",
				"Anywhere#realtimeadherencesites",
				new ApplicationStartupTimeout());
			}
			
		}

		public static void GotoAnywhereRealTimeAdherenceOverview(Guid buId, Guid siteId)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#realtimeadherenceteams/{0}/{1}",buId, siteId),
				"Anywhere#realtimeadherenceteams",
				new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeManageAdherenceOverview(Guid buId, Guid personId)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#manageadherence/{0}/{1}",buId, personId),
				"Anywhere#manageadherence",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereRealTimeAdherenceTeamOverview(Guid buId, Guid idForTeam)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#realtimeadherenceagents/{0}/{1}",buId, idForTeam),
				"Anywhere#realtimeadherenceagents",
				new ApplicationStartupTimeout(), new WaitUntilSubscriptionIsCompleted());
		}

		public static void GotoAnywhereRealTimeAdherenceTeamOverviewNoWait(Guid buId, Guid idForTeam)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#realtimeadherenceagents/{0}/{1}", buId, idForTeam),
				"Anywhere#realtimeadherenceagents",
				new ApplicationStartupTimeout());
		}

		public static void GoToMyReport()
		{
			GoToWaitForCompleted("MyTime#MyReport/Index", new ApplicationStartupTimeout());
		}

		public static void GoToMyDetailedAdherence(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#MyReport/Adherence/{0}/{1}/{2}",
					date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
					new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoMonthSchedulePage(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Schedule/Month/{0}/{1}/{2}",
					date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
					new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GotoMonthSchedulePage()
		{
			GoToWaitForCompleted("MyTime#Schedule/Month",
					new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

		public static void GoToMyReport(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#MyReport/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout());
		}

		public static void GotoMobileWeekSchedulePageNoWait()
		{
			GoToWaitForCompleted("MyTime#Schedule/MobileWeek", new ApplicationStartupTimeout());
		}

		public static void GotoMobileWeekSchedulePage()
		{
			GoToWaitForCompleted("MyTime#Schedule/MobileWeek", new ApplicationStartupTimeout());
		}

		public static void GotoMobileWeekSchedulePage(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Schedule/MobileWeek/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded());
		}

	    public static void GoToMyQueueMetrics(DateTime date)
	    {
            GoToWaitForCompleted(string.Format("MyTime#MyReport/QueueMetrics/{0}/{1}/{2}",
                    date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
                    new ApplicationStartupTimeout());
	    }

		public static void GotoQuickForecaster()
		{
			GoToWaitForCompleted("areas/wfm", new ApplicationStartupTimeout());
		}

		public static void GotoMessageTool(IEnumerable<Guid> ids)
		{
			GoToWaitForCompleted(string.Format("Messages?ids={0}",
				String.Join(",", ids.Select(x => x.ToString()).ToArray())),
				new ApplicationStartupTimeout());
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