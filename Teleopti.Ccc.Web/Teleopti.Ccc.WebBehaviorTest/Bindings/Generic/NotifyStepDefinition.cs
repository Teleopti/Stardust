using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class NotifyStepDefinition
	{
		const string notifyTextSelector = "#noty_bottom_layout_container .noty_bar .noty_message .noty_text";

		[Then(@"I should see one notify message")]
		public void ThenIShouldSeeAnAlert()
		{
			var jsCode = string.Format("return $('{0}').length === 1", notifyTextSelector);
			Browser.Interactions.AssertJavascriptResultContains(jsCode, "True");
		}
		[Then(@"I should see a notify message contains text (.*)")]
		public void ThenIShouldSeeANotifyMessageContainsText(string content)
		{
			var selector = string.Format(notifyTextSelector + ":contains('{0}')", content);
			Browser.Interactions.AssertExistsUsingJQuery(selector);
		}

		[Then(@"I should not see any notify")]
		public void ThenIShouldNotSeeAnyNotify()
		{
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container");
		}

		/*
		[Then(@"I should not see pop up notify message")]
		public void ThenIShouldNotSeePopUpNotifyMessage()
		{
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container:visible");
		}
		*/
	}
}