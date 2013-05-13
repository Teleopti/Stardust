using TechTalk.SpecFlow;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class NotifyStepDefinition
	{
		[Then(@"I should see one notify message")]
		public void ThenIShouldSeeAnAlert()
		{
			Browser.Interactions.AssertExists("#notifyLogger .notifyLoggerItem");
			Browser.Interactions.AssertNotExists("#notifyLogger", ".notifyLoggerItem:nth-child(2)");
		}
	}
}