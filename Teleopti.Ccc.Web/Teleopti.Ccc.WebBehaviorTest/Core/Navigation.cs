﻿using System;
using System.Threading;
using NUnit.Framework;
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
			Browser.Current.WaitUntilAjaxContentComplete();
			var uri = new Uri(TestSiteConfigurationSetup.Url, pageUrl);
			Log.Write("Browsing to: " + uri);
			Browser.Current.GoTo(uri);
			Log.Write("Ended up in: " + Browser.Current.Url);
			Browser.Current.WaitUntilAjaxContentComplete();
		}

		public static void GoToWithPossibleLongApplicationStartTime(string pageUrl)
		{
			using (new WatiNWaitForCompleteTimeout(60*5))
			{
				GoTo(pageUrl);
			}
		}

		public static void GotoGlobalSignInPage()
		{
			GoToWithPossibleLongApplicationStartTime("Start/Authentication/SignIn");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoGlobalMobileSignInPage()
		{
			GoToWithPossibleLongApplicationStartTime("Start/Authentication/MobileSignIn");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoGlobalMobileMenuPage()
		{
			GoToWithPossibleLongApplicationStartTime("Start/Menu/MobileMenu");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoMyTimeSignInPage()
		{
			GoToWithPossibleLongApplicationStartTime("MyTime/Authentication/SignIn");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReportsSignInPage(string hash)
		{
			GoToWithPossibleLongApplicationStartTime("MobileReports/Authentication/SignIn" + hash);
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileSignInPage>());
		}

		public static void GotoMobileReportsSettings()
		{
			GoToWithPossibleLongApplicationStartTime("MobileReports#report-settings-view");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoMyTime()
		{
			GoToWithPossibleLongApplicationStartTime("MyTime");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoMobileReports()
		{
			GoToWithPossibleLongApplicationStartTime("MobileReports");
			Pages.Pages.NavigatingTo(Browser.Current.Page<MobileReportsPage>());
		}

		public static void GotoSiteHomePage()
		{
			GoToWithPossibleLongApplicationStartTime("");
			Pages.Pages.NavigatingTo(Browser.Current.Page<SignInPage>());
		}

		public static void GotoAnApplicationPageOutsidePortal()
		{
			GoToWithPossibleLongApplicationStartTime("MyTime/Schedule/Week");
			Pages.Pages.NavigatingTo(Browser.Current.Page<PreferencePage>());
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

		public static void CreateCorruptCookie()
		{
			GoTo("Test/CorruptMyCookie");
		}

		public static void CreateNonExistingDatabaseCookie()
		{
			GoTo("Test/NonExistingDatasourceCookie");
		}

		public static void ExpireMyCookie()
		{
			// doing this twice because IE fails to grab the cookie after the first one sometimes..
			// probably depending on how quickly the next request takes places.
			// making a second request seems to enforce the cookie somehow..
			Browser.Current.Eval("Teleopti.MyTimeWeb.Common.ExpireMyCookie();");
			EventualAssert.That(() => Browser.Current.Text, Is.StringContaining("Cookie is expired!"));
			Browser.Current.Eval("Teleopti.MyTimeWeb.Common.ExpireMyCookie();");
			EventualAssert.That(() => Browser.Current.Text, Is.StringContaining("Cookie is expired!Cookie is expired!"));
		}
	}
}