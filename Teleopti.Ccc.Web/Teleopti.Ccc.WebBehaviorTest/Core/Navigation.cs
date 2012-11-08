using System;
using System.Linq;
using System.Runtime.InteropServices;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Navigation
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Navigation));

		public static void GoTo(string pageUrl)
		{
			GoTo(pageUrl, new IGoToInterceptor[] { });
		}

		public static void GoTo(string pageUrl, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(TestSiteConfigurationSetup.Url, pageUrl), interceptors);
		}

		public static void GotoRaw(string url, params IGoToInterceptor[] interceptors)
		{
			InnerGoto(new Uri(url), interceptors);
		}

		private static void InnerGoto(Uri uri, params IGoToInterceptor[] interceptors)
		{
			interceptors.ToList().ForEach(i => i.Before(uri));

			Log.Write("Browsing to: " + uri);
			Retrying.Action(() => Browser.Current.GoTo(uri));
			Log.Write("Ended up in: " + Browser.Current.Url);

			interceptors.Reverse().ToList().ForEach(i => i.After(uri));
		}

		public static void GotoAsm()
		{
			GoTo("MyTime/Asm", new OverrideNotifyBehavior());
		}

		public static void GotoGlobalSignInPage()
		{
			GoTo("Start/Authentication/SignIn", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoGlobalMobileSignInPage()
		{
			GoTo("Start/Authentication/MobileSignIn", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoGlobalMobileMenuPage()
		{
			GoTo("Start/Menu/MobileMenu", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoMyTimeSignInPage()
		{
			GoTo("MyTime/Authentication/SignIn", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReportsSignInPage(string hash)
		{
			GoTo("MobileReports/Authentication/SignIn" + hash, new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoMobileReportsSettings()
		{
			GoTo("MobileReports#report-settings-view");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoMyTime()
		{
			GoTo("MyTime", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReports()
		{
			GoTo("MobileReports", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoSiteHomePage()
		{
			GoTo("", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoTo("MyTime/Schedule/Week", new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoAnApplicationPage()
		{
			GotoWeekSchedulePage();
		}

		public static void GotoWeekSchedulePage()
		{
			GoTo("MyTime#Schedule/Week", new ApplicationStartupTimeout(), new LoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoWeekSchedulePage(DateTime date)
		{
			GoTo(string.Format("MyTime#Schedule/Week/{0}/{1}/{2}", 
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new LoadingOverlay(), new WaitUntilCompletelyLoaded());
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoStudentAvailability()
		{
			GoTo("MyTime#StudentAvailability/Index", new ApplicationStartupTimeout(), new LoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoStudentAvailability(DateTime date)
		{
			GoTo(string.Format("MyTime#StudentAvailability/Index/{0}/{1}/{2}",
			                   date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
			     new ApplicationStartupTimeout(), new LoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoPreference()
		{
			GoTo("MyTime#Preference/Index", new ApplicationStartupTimeout(), new LoadingOverlay(), new OverrideNotifyBehavior(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoPreference(DateTime date)
		{
			GoTo(string.Format("MyTime#Preference/Index/{0}/{1}/{2}",
				date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00")),
				new ApplicationStartupTimeout(), new LoadingOverlay(), new OverrideNotifyBehavior(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoRegionalSettings()
		{
			GoTo("MyTime#Settings/Index", new ApplicationStartupTimeout(), new LoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<RegionalSettingsPage>());
		}

		public static void GotoPasswordPage()
		{
			GoTo("MyTime#Settings/Password", new ApplicationStartupTimeout(), new LoadingOverlay());
			Pages.Pages.NavigatingTo(Browser.Current.Page<PasswordPage>());
		}

		public static void GotoRequests()
		{
			GoTo("MyTime#Requests/Index", new ApplicationStartupTimeout(), new LoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<RequestsPage>());
		}

		public static void GotoTeamSchedule()
		{
			GoTo("MyTime#TeamSchedule/Index", new ApplicationStartupTimeout(), new LoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<TeamSchedulePage>());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoTo(String.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}", 
				date.Year.ToString("0000"), date.Month.ToString("00"),date.Day.ToString("00"))
				, new ApplicationStartupTimeout(), new LoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<TeamSchedulePage>());
		}

		public static void GotoTheInternet()
		{
			Browser.Current.GoTo("about:blank");
		}

		public static void GotoBlank()
		{
			Browser.Current.GoTo("about:blank");
		}

	    public static void GotoMessagePage()
        {
            GoTo("MyTime#Message/Index", new ApplicationStartupTimeout(), new LoadingOverlay());
            Pages.Pages.NavigatingTo(Browser.Current.Page<MessagePage>());
	    }
	}



	public interface IGoToInterceptor
	{
		void Before(Uri url);
		void After(Uri url);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class ApplicationStartupTimeout : IGoToInterceptor
	{
		private WatiNWaitForCompleteTimeout _timeout;

		public void Before(Uri url)
		{
			_timeout = new WatiNWaitForCompleteTimeout(60);
		}

		public void After(Uri url)
		{
			_timeout.Dispose();
			_timeout = null;
		}
	}

	public class LoadingOverlay : IGoToInterceptor
	{
		public void Before(Uri url)
		{
			WaitUntilLoadingOverlayIsHidden();
		}

		public void After(Uri url)
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
		public void Before(Uri url)
		{
		}

		public void After(Uri url)
		{
			mockNotifyCall();
		}

		private static void mockNotifyCall()
		{
			const string jsCode = "Teleopti.MyTimeWeb.Notifier.Notify = function (value) {$('<span/>', {text: value, 'class': 'notifyLoggerItem'}).appendTo('#notifyLogger');};";
			Browser.Current.Eval(jsCode);
		}
	}

	public class WaitUntilReadyForInteraction : IGoToInterceptor
	{
		public void Before(Uri url)
		{
		}

		public void After(Uri url)
		{
			TestControllerMethods.WaitUntilReadyForInteraction();
		}
	}

	public class WaitUntilCompletelyLoaded : IGoToInterceptor
	{
		public void Before(Uri url)
		{
		}

		public void After(Uri url)
		{
			TestControllerMethods.WaitUntilCompletelyLoaded();
		}
	}
}