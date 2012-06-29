using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;

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
			if (Pages.Pages.Current is PreferencePage)
			{
				TestControllerMethods.WaitForPreferenceFeedbackToLoad();
			}
			TestControllerMethods.ExpireMyCookieInsidePortal();
		}

		[When(@"My cookie gets corrupt")]
		public void WhenMyCookieIsCorrupt()
		{
			TestControllerMethods.CreateCorruptCookie();
		}

		[When(@"My cookie gets pointed to non existing database")]
		public void WhenMyCookieGetsPointedToNonExistingDatabase()
		{
			TestControllerMethods.CreateNonExistingDatabaseCookie();
		}		
	}
}
