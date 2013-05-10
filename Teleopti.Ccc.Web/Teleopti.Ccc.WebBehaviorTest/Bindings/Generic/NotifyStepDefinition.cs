using TechTalk.SpecFlow;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class NotifyStepDefinition
	{
		[Then(@"I should see only one alert containing '(.*)'")]
		public void ThenIShouldSeeOnlyOneAlertContaining(string activity)
		{
			Browser.Interactions.AssertExists(string.Format("#notifyLogger .notifyLoggerItem:contains('{0}')", activity));
			Browser.Interactions.AssertExists("#notifyLogger .notifyLoggerItem");
			Browser.Interactions.AssertNotExists("#notifyLogger", ".notifyLoggerItem:nth-child(2)");
		}

		[Then(@"I should see one notify message")]
		public void ThenIShouldSeeAnAlert()
		{
			Browser.Interactions.AssertExists("#notifyLogger .notifyLoggerItem");
			Browser.Interactions.AssertNotExists("#notifyLogger", ".notifyLoggerItem:nth-child(2)");
		}
	}
}