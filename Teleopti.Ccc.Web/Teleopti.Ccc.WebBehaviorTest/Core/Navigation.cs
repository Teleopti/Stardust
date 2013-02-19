using System;
using System.Globalization;
using System.Linq;
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
			var args = new GotoArgs {Uri = uri};

			interceptors.ToList().ForEach(i => i.Before(args));

			Log.Write("Browsing to: " + args.Uri);
			Retrying.Action(() => Browser.Current.GoTo(args.Uri));
			Log.Write("Ended up in: " + Browser.Current.Url);

			interceptors.Reverse().ToList().ForEach(i => i.After(args));
		}

		public static void GotoAsm()
		{
			GoTo("MyTime/Asm", new OverrideNotifyBehavior());
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
			GoTo("MobileReports", new ApplicationStartupTimeout());
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
			GoTo("MyTime#Requests/Index", new ForceRefresh(), new ApplicationStartupTimeout(), new LoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<RequestsPage>());
		}

		public static void GotoTeamSchedule()
		{
			GoTo("MyTime#TeamSchedule/Index", new ApplicationStartupTimeout(), new LoadingOverlay(), new WaitUntilReadyForInteraction());
			Pages.Pages.NavigatingTo(Browser.Current.Page<TeamSchedulePage>());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoTo(string.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}", 
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

		public static void GotoAnywhereSchedule(string date = null)
		{
			string hash = string.Empty;
			if (!string.IsNullOrEmpty(date))
			{
				hash = string.Format(CultureInfo.InvariantCulture, "#teamschedule/{0}", date.Replace("-",""));
			}
			GoTo("Anywhere" + hash, new ApplicationStartupTimeout());
			Pages.Pages.NavigatingTo(Browser.Current.Page<AnywherePage>());
		}

		public static void GotoAnywhereAgentSchedule(Guid personId, DateTime date)
		{
			GoTo(string.Format("Anywhere#agentschedule/{0}/{1}",
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

	public class LoadingOverlay : IGoToInterceptor
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
			Browser.Current.Eval(jsCode);
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