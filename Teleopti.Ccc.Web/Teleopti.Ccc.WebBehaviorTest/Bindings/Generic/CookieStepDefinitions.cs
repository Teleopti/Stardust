using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class CookieStepDefinitions
	{

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
