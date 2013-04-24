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

		public static void GoTo(string pageUrl, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.Url, pageUrl), null, interceptors);
		}

		/// <summary>
		/// Dont use this method! 
		/// If you use good robustness practices this method is not required, and the result is more stable!
		/// </summary>
		/// <param name="uri"></param>
		public static void GoTo_ForUnstableStepDefinitions(string pageUrl, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.Url, pageUrl), Browser.Current.GoTo, interceptors);
		}

		public static void GotoRaw(string url, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(url), null, interceptors);
		}

		private static void InnerGoto(Uri url, Action<string> gotoMethod, params IGoToInterceptor[] interceptors)
		{
			var args = new GotoArgs {Uri = url};

			interceptors.ToList().ForEach(i => i.Before(args));

			Log.Info("Am at: " + Browser.Current.Url);
			Log.Info("Browsing to: " + args.Uri);

			if (gotoMethod == null)
				gotoMethod = Browser.Interactions.GoTo;
			gotoMethod(args.Uri.ToString());

			interceptors.Reverse().ToList().ForEach(i => i.After(args));

			Log.Info("Ended up in: " + Browser.Current.Url);
		}

		public static void GotoAsm()
		{
			GoTo_ForUnstableStepDefinitions("MyTime/Asm", new OverrideNotifyBehavior());
		}

		public static void GotoSiteHomePage()
		{
			GoTo("", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMyTime()
		{
			GoTo("MyTime", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReports()
		{
			GoTo_ForUnstableStepDefinitions("MobileReports", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoAnywhere()
		{
			GoTo("Anywhere", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoGlobalSignInPage()
		{
			GoTo("Authentication", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());

		}

		public static void GotoMobileReportsSignInPage(string hash)
		{
			GoTo("MobileReports/Authentication" + hash, new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReportsPage()
		{
			GoTo("MobileReports#", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoMobileReportsSettings()
		{
			GoTo("MobileReports#report-settings-view");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoTo_ForUnstableStepDefinitions("MyTime/Schedule/Week", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoAnApplicationPage()
		{
			GotoWeekSchedulePage();
		}

		public static void GotoWeekSchedulePage()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#Schedule/Week",
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoWeekSchedulePageNoWait()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#Schedule/Week", 
				new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoWeekSchedulePage(DateTime date)
		{
			GoTo_ForUnstableStepDefinitions(string.Format("MyTime#Schedule/Week/{0}/{1}/{2}", 
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitUntilCompletelyLoaded(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoStudentAvailability()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#StudentAvailability/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoStudentAvailability(DateTime date)
		{
			GoTo_ForUnstableStepDefinitions(string.Format("MyTime#StudentAvailability/Index/{0}/{1}/{2}",
			                   date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				 new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoPreference()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#Preference/Index", 
				new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new OverrideNotifyBehavior(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoPreference(DateTime date)
		{
			GoTo_ForUnstableStepDefinitions(string.Format("MyTime#Preference/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new OverrideNotifyBehavior(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoRegionalSettings()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#Settings/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<RegionalSettingsPage>());
		}

		public static void GotoPasswordPage()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#Settings/Password", new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PasswordPage>());
		}

		public static void GotoRequests()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#Requests/Index", new ForceRefresh(), new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<RequestsPage>());
		}

		public static void GotoTeamSchedule()
		{
			GoTo_ForUnstableStepDefinitions("MyTime#TeamSchedule/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<TeamSchedulePage>());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoTo_ForUnstableStepDefinitions(string.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}", 
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
			GoTo_ForUnstableStepDefinitions("MyTime#Message/Index", new ApplicationStartupTimeout(), new WaitForLoadingOverlay());
            Pages.Pages.NavigatingTo(Browser.Current.Page<MessagePage>());
	    }




		public static void GotoAnywhereTeamSchedule()
		{
			GoTo("Anywhere", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<AnywherePage>());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date)
		{
			GoTo(string.Format("Anywhere#teamschedule/{0}{1}{2}",
				date.Year.ToString("0000"), 
				date.Month.ToString("00"), 
				date.Day.ToString("00")),
				new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<AnywherePage>());
		}

		public static void GotoAnywhereTeamSchedule(DateTime date, Guid teamId)
		{
			GoTo(string.Format("Anywhere#teamschedule/{0}/{1}{2}{3}",
				teamId,
				date.Year.ToString("0000"), 
				date.Month.ToString("00"), 
				date.Day.ToString("00")),
				new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<AnywherePage>());
		}

		public static void GotoAnywherePersonSchedule(Guid personId, DateTime date)
		{
			GoTo(string.Format("Anywhere#personschedule/{0}/{1}",
				personId, date.Year.ToString("0000") + date.Month.ToString("00") + date.Day.ToString("00"))
				, new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<AnywherePage>());
		}

		public static void GotoAnywherePersonScheduleFullDayAbsenceForm(Guid personId, DateTime date)
		{
			GoTo(string.Format("Anywhere#personschedule/{0}/{1}/addfulldayabsence",
				personId, date.Year.ToString("0000") + date.Month.ToString("00") + date.Day.ToString("00"))
				, new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<AnywherePage>());
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

	public class ForceRefresh : IGoToInterceptor
	{
		public void Before(GotoArgs args)
		{
			var url = args.Uri.ToString();
			url = url.Replace("#", string.Format("?{0}#", Guid.NewGuid()));
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