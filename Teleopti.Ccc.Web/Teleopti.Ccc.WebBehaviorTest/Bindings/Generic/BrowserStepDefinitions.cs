using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BrowserStepDefinitions
	{
		[When(@"I press back in the web browser")]
		public void WhenIPressBackInTheWebBrowser()
		{
			Browser.Interactions.Javascript("history.back();");
		}

	}
}