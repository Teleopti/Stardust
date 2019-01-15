using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Adherence
{
	[Binding]
	public class ToolPageStepDefinitions
	{
		[When(@"I send '(.*)' for all agents")]
		public void WhenIClickFor(string state)
		{
			Browser.Interactions.AssertExists(".for-test-agent");
			Browser.Interactions.ClickContaining(".sendbatch", state);
			Browser.Interactions.AssertExists(".notice-info", "Done!");
		}
	}
}