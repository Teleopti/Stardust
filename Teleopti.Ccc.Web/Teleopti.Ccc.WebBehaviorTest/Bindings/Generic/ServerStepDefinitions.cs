using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.StartWeb;
using Teleopti.Ccc.WebBehaviorTest.Data;

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