using System;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Navigation
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Navigation));

		public static void GoToWaitForUrlAssert(string pageUrl, string assertUrlContains, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.Url, pageUrl), 
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
			InnerGoto(new Uri(TestSiteConfigurationSetup.Url, pageUrl), Browser.Interactions.GoToWaitForCompleted, interceptors);
		}

		public static void GotoRaw(string url, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(url), Browser.Interactions.GoToWaitForCompleted, interceptors);
		}

		private static void InnerGoto(Uri url, Action<string> gotoAction, params IGoToInterceptor[] interceptors)
		{
			var args = new GotoArgs {Uri = url};

			interceptors.ToList().ForEach(i => i.Before(args));

			Log.Info("Am at: " + Browser.Current.Url);
			Log.Info("Browsing to: " + args.Uri);

			gotoAction.Invoke(args.Uri.ToString());

			interceptors.Reverse().ToList().ForEach(i => i.After(args));

			Log.Info("Ended up in: " + Browser.Current.Url);
		}

		public static void GotoAsm()
		{
			GoToWaitForCompleted("MyTime/Asm", new OverrideNotifyBehavior());
		}

		public static void GotoSiteHomePage()
		{
			GoToWaitForCompleted("", new ApplicationStartupTimeout(), new BustCache());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMyTime()
		{
			GoToWaitForCompleted("MyTime", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReports()
		{
			GoToWaitForCompleted("MobileReports", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoGlobalSignInPage()
		{
			GoToWaitForCompleted("Authentication", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());

		}

		public static void GotoMobileReportsSignInPage(string hash)
		{
			GoToWaitForCompleted("MobileReports/Authentication" + hash, new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReportsPage()
		{
			GoToWaitForCompleted("MobileReports#", new ApplicationStartupTimeout());
		}

		public static void GotoMobileReportsSettings()
		{
			GoToWaitForCompleted("MobileReports#report-settings-view");
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoToWaitForCompleted("MyTime/Schedule/Week", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoAnApplicationPage()
		{
			GotoWeekSchedulePage();
		}

		public static void GotoWeekSchedulePage()
		{
			GoToWaitForCompleted("MyTime#Schedule/Week",
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded(), new WaitForLoadingOverlay(),new OverrideNotifyBehavior());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoWeekSchedulePageNoWait()
		{
			GoToWaitForCompleted("MyTime#Schedule/Week",
                new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoWeekSchedulePage(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Schedule/Week/{0}/{1}/{2}", 
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoAvailability()
		{
			GoToWaitForCompleted("MyTime#Availability/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoAvailability(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Availability/Index/{0}/{1}/{2}",
			                   date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				 new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoPreference()
		{
			GoToWaitForCompleted("MyTime#Preference/Index", 
				new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new OverrideNotifyBehavior(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoPreference(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#Preference/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new OverrideNotifyBehavior(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoRegionalSettings()
		{
			GoToWaitForCompleted("MyTime#Settings/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<RegionalSettingsPage>());
		}

		public static void GotoPasswordPage()
		{
			GoToWaitForCompleted("MyTime#Settings/Password", new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PasswordPage>());
		}

		public static void GotoRequests()
		{
			GoToWaitForCompleted("MyTime#Requests/Index", new BustCache(), new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<RequestsPage>());
		}

		public static void GotoTeamSchedule()
		{
			GoToWaitForCompleted("MyTime#TeamSchedule/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<TeamSchedulePage>());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoToWaitForCompleted(string.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}", 
				date.Year.ToString("0000"), date.Month.ToString("00"),date.Day.ToString("00"))
				, new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<TeamSchedulePage>());
		}

		public static void GotoTheInternet()
		{
			GotoRaw("about:blank");
			Browser.Interactions.AssertUrlContains("blank");
		}

		public static void GotoBlank()
		{
			GotoRaw("about:blank");
			Browser.Interactions.AssertUrlContains("blank");
		}

	    public static void GotoMessagePage()
        {
			GoToWaitForCompleted("MyTime#Message/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
            Pages.Pages.NavigatingTo(Browser.Current.Page<MessagePage>());
	    }

		public static void GoToPerformanceTool()
		{
			GoToWaitForUrlAssert("PerformanceTool", "PerformanceTool", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoAnywhere()
		{
			GoToWaitForUrlAssert("Anywhere", "Anywhere", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoAnywhereTeamSchedule()
		{
			GoToWaitForUrlAssert("Anywhere", "Anywhere", new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date)
		{
			GoToWaitForUrlAssert(string.Format("Anywhere#teamschedule/{0}{1}{2}",
				date.Year.ToString("0000"), 
				date.Month.ToString("00"), 
				date.Day.ToString("00")),
				"Anywhere#teamschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid teamId)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#teamschedule/{0}/{1}{2}{3}",
				teamId,
				date.Year.ToString("0000"), 
				date.Month.ToString("00"), 
				date.Day.ToString("00")),
				"Anywhere#teamschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonSchedule(Guid personId, DateTime date)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#personschedule/{0}/{1}{2}{3}",
				personId,
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				"Anywhere#personschedule",
				new ApplicationStartupTimeout());
		}

		public static void GotoAnywherePersonScheduleFullDayAbsenceForm(Guid personId, DateTime date)
		{
			GoToWaitForUrlAssert(
				string.Format("Anywhere#personschedule/{0}/{1}{2}{3}/addfulldayabsence",
				personId, 
				date.Year.ToString("0000"),
				date.Month.ToString("00"),
				date.Day.ToString("00")),
				"Anywhere#personschedule",
				new ApplicationStartupTimeout());
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

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class ApplicationStartupTimeout : IGoToInterceptor
	{
		private WatiNWaitForCompleteTimeout _timeout;

		public void Before(GotoArgs args)
		{
			_timeout = new WatiNWaitForCompleteTimeout(60);
		}

		public void After(GotoArgs args)
		{
			_timeout.Dispose();
			_timeout = null;
		}
	}

	public class WaitForLoadingOverlay : IGoToInterceptor
	{
		public void Before(GotoArgs args)
		{
			WaitUntilLoadingOverlayIsHidden();
		}

		public void After(GotoArgs args)
		{
			WaitUntilLoadingOverlayIsHidden();
		}

		private static void WaitUntilLoadingOverlayIsHidden()
		{
			var loading = Browser.Current.Div("loading");
			if (!loading.Exists) return;
			loading.WaitUntilHidden();
		}

	}

	public class OverrideNotifyBehavior : IGoToInterceptor
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			mockNotifyCall();
		}

		private static void mockNotifyCall()
		{
			const string jsCode = "Teleopti.MyTimeWeb.Notifier.Notify = function (value) {$('<span/>', {text: value, 'class': 'notifyLoggerItem'}).appendTo('#notifyLogger');};";
			Browser.Interactions.Javascript(jsCode);
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