using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class ApplicationPageStepDefinition
	{
		[Then(@"I should see Anywhere")]
		public void ThenIShouldSeeAnywhere()
		{
			Browser.Interactions.AssertExists("#content-placeholder");
		}
	}
}