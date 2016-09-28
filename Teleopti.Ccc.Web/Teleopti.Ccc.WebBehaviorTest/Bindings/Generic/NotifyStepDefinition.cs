using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
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
			Browser.Interactions.AssertExists(notifyTextSelector);
		}

		[Then(@"I should see a notify message contains text (.*)")]
		public void ThenIShouldSeeANotifyMessageContainsText(string content)
		{
			var selector = string.Format(notifyTextSelector + ":contains('{0}')", content);
			Browser.Interactions.AssertExistsUsingJQuery(selector);
		}

		[When(@"I should not see any notify")]
		[Then(@"I should not see any notify")]
		public void ThenIShouldNotSeeAnyNotify()
		{
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container");
		}

		[Then(@"I should not see pop up notify message within one minute")]
		public void ThenIShouldNotSeePopUpNotifyMessageWithinOneMinute()
		{
			Browser.TimeoutScope(new TimeSpan(0, 1, 0));
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container");
		}

		[Then(@"I should not see pop up notify message")]
		public void ThenIShouldNotSeePopUpNotifyMessage()
		{
			Browser.TimeoutScope(new TimeSpan(0, 1, 0));
			Browser.Interactions.AssertNotExists("#notifyLogger", "#noty_bottom_layout_container");
		}

		private static Guid idForActivity(string activityName)
		{
			var activityId = (from a in DataMaker.Data().UserDatasOfType<ActivityConfigurable>()
						  let activity = a.Activity
						  where activity.Name.Equals(activityName)
						  select activity.Id.GetValueOrDefault()).First();
			return activityId;
		}

		private static Guid idForLogonUser()
		{
			return DataMaker.Me().Person.Id.GetValueOrDefault();
		}
	}
}