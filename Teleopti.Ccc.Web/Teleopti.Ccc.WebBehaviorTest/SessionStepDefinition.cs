﻿using TechTalk.SpecFlow;
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
			TestSiteConfigurationSetup.RecycleApplication();
		}

		[When(@"my cookie expires")]
		public void WhenMyCookieExpires()
		{
			// we need to wait for everything to be completely loaded
			// and for all ajax call to complete
			// before we expire the cookie from inside the application.
			// otherwise we run the risk of being logged out the moment we expire it!
			TestControllerMethods.WaitUntilCompletelyLoaded();
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
