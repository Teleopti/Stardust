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
			Browser.Interactions.AssertExists("#noty_bottom_layout_container .noty_bar .noty_message .noty_text");
		}

		[Then(@"I should see a notify message contains (.*)")]
		public void ThenIShouldSeeAnAlertContains(string content)
		{
			Browser.Interactions.AssertAnyContains("#noty_bottom_layout_container .noty_bar .noty_message .noty_text", content);
		}

		[Then(@"I should not see any alert")]
		public void ThenIShouldNotSeeAnyAlert()
		{
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container");
		}
	}
}