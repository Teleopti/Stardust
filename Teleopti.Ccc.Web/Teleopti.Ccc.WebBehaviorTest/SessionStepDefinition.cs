using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class SessionStepDefinition
	{
		[When(@"the server restarts")]
		public void GivenTheServerRestarts()
		{
			TestSiteConfigurationSetup.RestartApplication();
		}

		[When(@"my cookie expires")]
		public void WhenMyCookieExpires()
		{
			Navigation.ExpireMyCookie();
		}

		[When(@"My cookie gets corrupt")]
		public void WhenMyCookieIsCorrupt()
		{
			Navigation.CreateCorruptCookie();
		}
	}
}
