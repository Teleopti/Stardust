﻿using TechTalk.SpecFlow;
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

		[Then(@"I should see a notify message contains text (.*)")]
		public void ThenIShouldSeeANotifyMessageContainsText(string content)
		{
			Browser.Interactions.AssertAnyContains("#noty_bottom_layout_container .noty_bar .noty_message .noty_text", content);
		}

		[Then(@"I should see a notify message contains all text")]
		public void ThenIShouldSeeANotifyMessageContainsAllText(Table contentTable)
		{
			const string selector = "#noty_bottom_layout_container .noty_bar .noty_message .noty_text";
			for(var i =0;i<contentTable.RowCount; i++)
			{
				var content = contentTable.Rows[i]["Content"].Trim();
				Browser.Interactions.AssertAnyContains(selector, content);
			}
		}

		[Then(@"I should not see any notify")]
		public void ThenIShouldNotSeeAnyNotify()
		{
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container");
		}

		[Then(@"I should not see pop up notify message")]
		public void ThenIShouldNotSeePopUpNotifyMessage()
		{
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container:visible");
		}

	}
}