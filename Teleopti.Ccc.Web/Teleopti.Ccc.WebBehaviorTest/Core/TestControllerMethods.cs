using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
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
			Navigation.GotoRaw("file://" + System.IO.Path.Combine(Environment.CurrentDirectory, "BeforeTestRun.html"));
		}

		public static void BeforeScenario()
		{
			Navigation.GoTo("Test/BeforeScenario", new ApplicationStartupTimeout());
		}

		/// <summary>
		/// Does an automatic logon with hardcoded username and password.
		/// </summary>
		/// <remarks>
		/// Creates, persist a auto user, gets the hardcoded password and logs on.
		/// </remarks>
		public static void Logon()
		{
			var userName = UserFactory.User().MakeUser();
			var password = TestData.CommonPassword;
			InnerLogon(userName, password);
		}

		public static void LogonForSpecificUser(string userName, string password)
		{
			UserFactory.User().MakeUser(userName, userName, password);
			InnerLogon(userName, password);
		}

		/// <summary>
		/// Imitates a logon process on UI with the given username and password.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
		private static void InnerLogon(string userName, string password)
		{
			Pages.Pages.CurrentSignInPage = Browser.Current.Page<SignInPage>();
			const string dataSourceName = "TestData";
			var businessUnitName = UserFactory.User().Person.PermissionInformation.ApplicationRoleCollection.Single().BusinessUnit.Name;
			var queryString = string.Format("?dataSourceName={0}&businessUnitName={1}&userName={2}&password={3}", dataSourceName, businessUnitName, userName, password);
			Navigation.GoTo("Test/Logon" + queryString);
		}

		public static void ExpireMyCookieInsidePortal()
		{
			// doing this twice because IE fails to grab the cookie after the first one sometimes..
			// probably depending on how quickly the next request takes place.
			// making a second request seems to enforce the cookie somehow..
			Retrying.Javascript("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			EventualAssert.That(() => Browser.Current.Eval("Teleopti.MyTimeWeb.Test.PopTestMessages();"), Is.StringContaining("Cookie is expired!"));

			Retrying.Javascript("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			EventualAssert.That(() => Browser.Current.Eval("Teleopti.MyTimeWeb.Test.PopTestMessages();"), Is.StringContaining("Cookie is expired!"));
		}

		public static void TestMessage(string message)
		{
			Retrying.Javascript("Teleopti.MyTimeWeb.Test.TestMessage('" + message + "');");
			EventualAssert.That(() => Browser.Current.Eval("Teleopti.MyTimeWeb.Test.PopTestMessages();"), Is.StringContaining(message));
		}

		public static void WaitUntilReadyForInteraction()
		{
			EventualAssert.That(() => Browser.Current.Eval("Teleopti.MyTimeWeb.Test.PopTestMessages();"), Is.StringContaining("Ready for interaction"));
		}

		public static void WaitUntilCompletelyLoaded()
		{
			EventualAssert.That(() => Browser.Current.Eval("Teleopti.MyTimeWeb.Test.PopTestMessages();"), Is.StringContaining("Completely loaded"));
		}

	}
}