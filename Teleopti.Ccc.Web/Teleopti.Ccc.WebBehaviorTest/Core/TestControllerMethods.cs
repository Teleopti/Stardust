using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class TestControllerMethods
	{

		public static void CreateCorruptCookie()
		{
			Navigation.GoTo("Test/CorruptMyCookie");
		}

		public static void CreateNonExistingDatabaseCookie()
		{
			Navigation.GoTo("Test/NonExistingDatasourceCookie");
		}

		public static void ExpireMyCookie()
		{
			Navigation.GoTo("Test/ExpireMyCookie");
		}

		public static void BeforeTestRun()
		{
			Browser.Current.GoTo("file://" + System.IO.Path.Combine(Environment.CurrentDirectory, "BeforeTestRun.html"));
		}

		public static void BeforeScenario()
		{
			Navigation.GoTo("Test/BeforeScenario", new ApplicationStartupTimeout());
		}

		public static void LogonMobile()
		{
			Pages.Pages.CurrentSignInPage = Browser.Current.Page<MobileSignInPage>();
			InnerLogon();
		}

		public static void Logon()
		{
			Pages.Pages.CurrentSignInPage = Browser.Current.Page<SignInPage>();
			InnerLogon();
		}

		private static void InnerLogon()
		{
			const string dataSourceName = "TestData";
			var userName = UserFactory.User().MakeUser();
			var businessUnitName = UserFactory.User().Person.PermissionInformation.ApplicationRoleCollection.Single().BusinessUnit.Name;
			var password = TestData.CommonPassword;
			var queryString = string.Format("?dataSourceName={0}&businessUnitName={1}&userName={2}&password={3}", dataSourceName, businessUnitName, userName, password);
			Navigation.GoTo("Test/Logon" + queryString);
		}

		public static void ExpireMyCookieInsidePortal()
		{
			// doing this twice because IE fails to grab the cookie after the first one sometimes..
			// probably depending on how quickly the next request takes place.
			// making a second request seems to enforce the cookie somehow..
			Browser.Current.Eval("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			EventualAssert.That(() =>
			                    	{
										Browser.Current.Eval("Teleopti.MyTimeWeb.Test.FlushPageLog();");
										return Browser.Current.Text;
			                    	}, Is.StringContaining("Cookie is expired!"));

			Browser.Current.Eval("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			EventualAssert.That(() =>
			                    	{
										Browser.Current.Eval("Teleopti.MyTimeWeb.Test.FlushPageLog();");
										var text = Browser.Current.Text;
			                    		var regex = new Regex("Cookie is expired!");
			                    		return regex.Matches(text).Count;
			                    	}, Is.EqualTo(2));
		}

		public static void PageLog(string message)
		{
			Browser.Current.Eval("Teleopti.MyTimeWeb.Test.PageLog('" + message + "');");
			Browser.Current.Eval("Teleopti.MyTimeWeb.Test.FlushPageLog();");
			EventualAssert.That(() => Browser.Current.Text, Is.StringContaining(message));
		}

		public static void WaitUntilReadyForInteraction()
		{
			EventualAssert.That(() =>
			                    	{
			                    		Browser.Current.Eval("Teleopti.MyTimeWeb.Test.FlushPageLog();");
			                    		return Browser.Current.Text;
			                    	}, Is.StringContaining("Ready for interaction"));
		}

		public static void WaitUntilCompletelyLoaded()
		{
			EventualAssert.That(() =>
			                    	{
			                    		Browser.Current.Eval("Teleopti.MyTimeWeb.Test.FlushPageLog();");
			                    		return Browser.Current.Text;
			                    	}, Is.StringContaining("Completely loaded"));
		}

	}
}