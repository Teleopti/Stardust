using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ServerStepDefinitions
	{
		[When(@"the server restarts")]
		public void GivenTheServerRestarts()
		{
			TestSiteConfigurationSetup.RecycleApplication();
		}
	}
}