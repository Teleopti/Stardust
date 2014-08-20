using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using log4net;

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

		public static void BeforeScenario()
		{
		    var defaultProvider = "Teleopti";
			var useBroker = "false";
			if (ScenarioContext.Current.ScenarioInfo.Tags.Contains("broker"))
				useBroker = "true";
		    if (ScenarioContext.Current.ScenarioInfo.Tags.Contains("WindowsAsDefaultIdentityProviderLogon"))
		        defaultProvider = "Windows";

			// use a scenario tag here for enableMyTimeMessageBroker if required
		    Navigation.GoToWaitForCompleted(string.Format(CultureInfo.InvariantCulture,
		        "Test/BeforeScenario?enableMyTimeMessageBroker={0}&defaultProvider={1}", useBroker, defaultProvider));
		}

		/// <summary>
		/// Does an automatic logon with hardcoded username and password.
		/// </summary>
		/// <remarks>
		/// Creates, persist a auto user, gets the hardcoded password and logs on.
		/// </remarks>
		public static void Logon()
		{
			var userName = DataMaker.Data().ApplyDelayed();
			var password = TestData.CommonPassword;
			InnerLogon(userName, password);
		}

		public static void LogonForSpecificUser(string userName, string password)
		{
			DataMaker.Data().Apply(
				new UserConfigurable
					{
						UserName = userName, 
						Password = password
					});
			DataMaker.Data().ApplyDelayed();
			InnerLogon(userName, password);
		}

		/// <summary>
		/// Imitates a logon process on UI with the given username and password.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
		private static void InnerLogon(string userName, string password)
		{
			const string dataSourceName = "TestData";
			var businessUnitName = DataMaker.Data().MePerson.PermissionInformation.ApplicationRoleCollection.Single().BusinessUnit.Name;
			var queryString = string.Format("?dataSourceName={0}&businessUnitName={1}&userName={2}&password={3}", dataSourceName, businessUnitName, userName, password);
			Navigation.GoToWaitForCompleted("Test/Logon" + queryString, new ApplicationStartupTimeout());
		}

		public static void ExpireMyCookieInsidePortal()
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			Browser.Interactions.AssertJavascriptResultContains("return Teleopti.MyTimeWeb.Test.GetTestMessages();", "Cookie is expired!");
		}

		public static void TestMessage(string message)
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Test.TestMessage('" + message + "');");
			Browser.Interactions.AssertJavascriptResultContains("return Teleopti.MyTimeWeb.Test.GetTestMessages();", message);
		}

		public static void WaitUntilReadyForInteraction()
		{
			Browser.Interactions.AssertJavascriptResultContains("return (Teleopti != undefined) ? 'go' : 'no go';", "go");
			Browser.Interactions.AssertJavascriptResultContains("return Teleopti.MyTimeWeb.Test.GetTestMessages();", "Ready for interaction");
		}

		public static void WaitUntilCompletelyLoaded()
		{
			Browser.Interactions.AssertJavascriptResultContains("return (Teleopti != undefined) ? 'go' : 'no go';", "go");
			Browser.Interactions.AssertJavascriptResultContains("return Teleopti.MyTimeWeb.Test.GetTestMessages();", "Completely loaded");
		}

	}
}