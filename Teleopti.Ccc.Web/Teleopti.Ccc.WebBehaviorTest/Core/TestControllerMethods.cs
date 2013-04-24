using System;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class TestControllerMethods
	{

		public static void CreateCorruptCookie()
		{
			Navigation.GoToWaitForCompleted("Test/CorruptMyCookie");
		}

		public static void CreateNonExistingDatabaseCookie()
		{
			Navigation.GoToWaitForCompleted("Test/NonExistingDatasourceCookie");
		}

		public static void SetCurrentTime(DateTime time)
		{
			Navigation.GoToWaitForCompleted("Test/SetCurrentTime?dateSet=" + time);
		}

		public static void BeforeTestRun()
		{
			Navigation.GotoRaw("file://" + System.IO.Path.Combine(Environment.CurrentDirectory, "BeforeTestRun.html"));
		}

		public static void BeforeScenario()
		{
			// use a scenario tag here for enableMyTimeMessageBroker if required
			Navigation.GoToWaitForCompleted("Test/BeforeScenario?enableMyTimeMessageBroker=false", new ApplicationStartupTimeout());
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

		public static void LogonWithReadModelsUpdated()
		{
			var userName = UserFactory.User().MakeUser(true);
			var password = TestData.CommonPassword;
			InnerLogon(userName, password);
		}

		public static void LogonForSpecificUser(string userName, string password)
		{
			UserFactory.User().MakeUser(userName, userName, password, false);
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
			Navigation.GoToWaitForCompleted("Test/Logon" + queryString);
		}

		public static void ExpireMyCookieInsidePortal()
		{
			// doing this twice because IE fails to grab the cookie after the first one sometimes..
			// probably depending on how quickly the next request takes place.
			// making a second request seems to enforce the cookie somehow..

			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			Browser.Interactions.AssertJavascriptResultContains("Teleopti.MyTimeWeb.Test.PopTestMessages();", "Cookie is expired!");

			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			Browser.Interactions.AssertJavascriptResultContains("Teleopti.MyTimeWeb.Test.PopTestMessages();", "Cookie is expired!");
		}

		public static void TestMessage(string message)
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Test.TestMessage('" + message + "');");
			Browser.Interactions.AssertJavascriptResultContains("Teleopti.MyTimeWeb.Test.PopTestMessages();", message);
		}

		public static void WaitUntilReadyForInteraction()
		{
			Browser.Interactions.AssertJavascriptResultContains("Teleopti.MyTimeWeb.Test.PopTestMessages();", "Ready for interaction");
		}

		public static void WaitUntilCompletelyLoaded()
		{
			Browser.Interactions.AssertJavascriptResultContains("Teleopti.MyTimeWeb.Test.PopTestMessages();", "Completely loaded");
		}

	}
}