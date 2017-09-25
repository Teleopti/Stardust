using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class RealTimeAdherenceToolPageStepDefinitions
	{
		[When(@"I send '(.*)' for all agents")]
		public void WhenIClickFor(string state)
		{
			Browser.Interactions.AssertExists(".for-test-agent");
			Browser.Interactions.ClickContaining(".sendbatch", state.ToUpper());
			Browser.Interactions.AssertExists(".notice-info", "Done!");
		}
	}
}