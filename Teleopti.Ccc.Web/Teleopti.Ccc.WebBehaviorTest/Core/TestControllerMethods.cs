using System;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class TestControllerMethods
	{
		public static void CreateCorruptCookie()
		{
			navigateOrRequest("Test/CorruptMyCookie");
		}

		public static void CreateNonExistingDatabaseCookie()
		{
			navigateOrRequest("Test/NonExistingDatasourceCookie");
		}

		public static void SetCurrentTime(DateTime time)
		{
			DataMaker.EndSetupPhase();
			navigateOrRequest("Test/SetCurrentTime?ticks=" + time.Ticks);
		}
		
		public static void ClearConnections()
		{
			navigateOrRequest("Test/ClearConnections");
		}
		
		private static void navigateOrRequest(string url)
		{
			if (Browser.Interactions.TryCheckIfUrlContains("/Test/"))
				Navigation.Navigation.GoToPage(url);
			else
			{
				using (var h = new Http())
					h.Get(url);
			}
		}

		public static void BeforeScenario()
		{
			var useBroker = "false";
			if (ScenarioContext.Current.ScenarioInfo.Tags.Contains("broker"))
				useBroker = "true";
			var defaultProvider = "Teleopti";
			if (ScenarioContext.Current.ScenarioInfo.Tags.Contains("WindowsAsDefaultIdentityProviderLogon"))
		        defaultProvider = "Windows";
			var usePolicy = FeatureContext.Current.FeatureInfo.Tags.Contains("PasswordPolicy");

			// use a scenario tag here for enableMyTimeMessageBroker if required
			Navigation.Navigation.GoToPage($"Test/BeforeScenario?name={ScenarioContext.Current.ScenarioInfo.Title}&enableMyTimeMessageBroker={useBroker}&defaultProvider={defaultProvider}&usePasswordPolicy={usePolicy}");
		}

		/// <summary>
		/// Does an automatic logon with hardcoded username and password.
		/// </summary>
		/// <remarks>
		/// Creates, persist a auto user, gets the hardcoded password and logs on.
		/// </remarks>
		public static void Logon()
		{
			DataMaker.EndSetupPhase();
			var userName = DataMaker.Me().LogOnName;
			var password = DefaultPassword.ThePassword;
			innerLogon(userName, password);
		}

		public static void LogonWithRememberMe()
		{
			DataMaker.EndSetupPhase();
			var userName = DataMaker.Me().LogOnName;
			var password = DefaultPassword.ThePassword;
			innerLogon(userName, password, true);
		}

		public static void LogonForSpecificUser(string userName, string password)
		{
			DataMaker.EndSetupPhase();
			innerLogon(userName, password);
		}

		private static void innerLogon(string userName, string password, bool isPersistent=false)
		{
			var businessUnitName = DataMaker.Me().Person.PermissionInformation.ApplicationRoleCollection.First().BusinessUnit.Name;
			var queryString = string.Format("?businessUnitName={0}&userName={1}&password={2}&isPersistent={3}", businessUnitName, userName, password, isPersistent);
			Navigation.Navigation.GoToPage("Test/Logon" + queryString);
		}

		public static void ExpireMyCookieInsidePortal()
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.Test.ExpireMyCookie('Cookie is expired!');");
			Browser.Interactions.AssertJavascriptResultContains("return Teleopti.MyTimeWeb.Test.GetTestMessages();", "Cookie is expired!");
		}

		public static void WaitUntilReadyForInteraction()
		{
			Browser.Interactions.AssertJavascriptResultContains("return (Teleopti != undefined) ? 'go' : 'x';", "go");
			Browser.Interactions.AssertJavascriptResultContains("return Teleopti.MyTimeWeb.Test.GetTestMessages();", "Ready for interaction");
		}

		public static void WaitUntilCompletelyLoaded()
		{
			Browser.Interactions.AssertJavascriptResultContains("return (Teleopti != undefined) ? 'go' : 'x';", "go");
			Browser.Interactions.AssertJavascriptResultContains("return Teleopti.MyTimeWeb.Test.GetTestMessages();", "Completely loaded");
		}

	}
}