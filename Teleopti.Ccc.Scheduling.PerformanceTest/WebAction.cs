using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;

namespace Teleopti.Ccc.Scheduling.PerformanceTest
{
	public static class WebAction
	{
		public static void Logon(IBrowserInteractions browserInteractions, string businessUnitName, string userName, string password)
		{
			browserInteractions.GoTo(string.Concat(
				TestSiteConfigurationSetup.URL,
				"Test/Logon",
				string.Format("?businessUnitName={0}&userName={1}&password={2}", businessUnitName, userName, password)));
		}
	}
}