using System;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
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
			using (new WatiNWaitForCompleteTimeout(60)) // to handle possible application startup
			{
				Browser.Current.WaitUntilAjaxContentComplete();
				var uri = new Uri(TestSiteConfigurationSetup.Url, pageUrl);
				Log.Write("Browsing to: " + uri);
				Browser.Current.GoTo(uri);
				Log.Write("Ended up in: " + Browser.Current.Url);
				Browser.Current.WaitUntilAjaxContentComplete();
			}
		}

		public static void GotoGlobalSignInPage()
		{
			GoTo("Start/Authentication/SignIn");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoGlobalMobileSignInPage()
		{
			GoTo("Start/Authentication/MobileSignIn");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoGlobalMobileMenuPage()
		{
			GoTo("Start/Menu/MobileMenu");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoMyTimeSignInPage()
		{
			GoTo("MyTime/Authentication/SignIn");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReportsSignInPage(string hash)
		{
			GoTo("MobileReports/Authentication/SignIn" + hash);
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoMobileReportsSettings()
		{
			GoTo("MobileReports#report-settings-view");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoMyTime()
		{
			GoTo("MyTime");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReports()
		{
			GoTo("MobileReports");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoSiteHomePage()
		{
			GoTo("");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoTo("MyTime/Schedule/Week");
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoAnApplicationPage()
		{
			GotoWeekSchedulePage();
		}

		public static void GotoWeekSchedulePage()
		{
			GoTo("MyTime#Schedule/Week");
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoWeekSchedulePage(DateTime date)
		{
			GoTo(string.Format("MyTime#Schedule/Week/{0}/{1}/{2}", date.Year.ToString("0000"), date.Month.ToString("00"),
			                   date.Day.ToString("00")));
			Pages.Pages.NavigatingTo(Browser.Current.Page<WeekSchedulePage>());
		}

		public static void GotoStudentAvailability()
		{
			GoTo("MyTime#StudentAvailability/Index");
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoStudentAvailability(DateTime date)
		{
			GoTo(string.Format("MyTime#StudentAvailability/Index/{0}/{1}/{2}", date.Year.ToString("0000"),
			                   date.Month.ToString("00"), date.Day.ToString("00")));
			Pages.Pages.NavigatingTo(Browser.Current.Page<StudentAvailabilityPage>());
		}

		public static void GotoPreference()
		{
			GoTo("MyTime#Preference/Index");
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoRegionalSettings()
		{
			GoTo("MyTime#Settings/Index");
			Pages.Pages.NavigatingTo(Browser.Current.Page<RegionalSettingsPage>());
		}

		public static void GotoPasswordPage()
		{
			GoTo("MyTime#Settings/Password");
			Pages.Pages.NavigatingTo(Browser.Current.Page<PasswordPage>());
		}

		public static void GotoPreference(DateTime date)
		{
			GoTo(string.Format("MyTime#Preference/Index/{0}/{1}/{2}", date.Year.ToString("0000"), date.Month.ToString("00"),
			                   date.Day.ToString("00")));
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
		}

		public static void GotoRequests()
		{
			GoTo("MyTime#Requests/Index");
			Pages.Pages.NavigatingTo(Browser.Current.Page<RequestsPage>());
		}

		public static void GotoTeamSchedule()
		{
			GoTo("MyTime#TeamSchedule/Index");
			Pages.Pages.NavigatingTo(Browser.Current.Page<TeamSchedulePage>());
		}

		public static void GotoTeamSchedule(DateTime date)
		{
			GoTo(string.Format("MyTime#TeamSchedule/Index/{0}/{1}/{2}", date.Year.ToString("0000"), date.Month.ToString("00"),
			                   date.Day.ToString("00")));
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

	}
}