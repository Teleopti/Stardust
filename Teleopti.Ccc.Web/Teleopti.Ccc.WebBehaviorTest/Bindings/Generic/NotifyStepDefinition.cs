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

		[When(@"I wait until week schedule is fully loaded")]
		[Then(@"I wait until week schedule is fully loaded")]
		public void WhenIWaitUntilWeekScheduleIsFullyLoaded()
		{
			Browser.Interactions.AssertUrlContains("MyTime#Schedule/Week");
			Browser.Interactions.AssertNotVisibleUsingJQuery("#loading");
		}

		[When(@"An activity '(.*)' with time '(.*)' to '(.*)' is added to my schedule")]
		[Then(@"An activity '(.*)' with time '(.*)' to '(.*)' is added to my schedule")]
		public void ThenAnActivityWithTimeToIsAddedToMySchedule(string activity, string startTime, string endTime)
		{
			Browser.Interactions.Javascript("$('#loading').show()");
			var addActivityUrl = "/api/TeamScheduleCommand/AddActivity";
			var userId = idForLogonUser();
			var activityId = idForActivity(activity);

			var addActivityJs ="$.ajax({" +
									"url:'"+ addActivityUrl + "'," +
									"type:'POST'," +
									"contentType:'application/json'," +
									"data:JSON.stringify({" +
											"ActivityId: '"+ activityId + "'," +
											"ActivityType: 1," +
											"Date:'"+ startTime.Split(' ')[0] +"', " +
											"StartTime:'"+ startTime + "'," +
											"EndTime:'"+ endTime + "'," +
											"PersonIds:['" + userId + "']" +
										"})," +
									"success: function(){}" +
								"});";
								
			Browser.Interactions.Javascript(addActivityJs);
			Browser.Interactions.Javascript("$('#loading').hide()");
		}

		[Then(@"I should see activity '(.*)' on my schedule table")]
		public void ThenIShouldSeeActivityOnMyScheduleTable(string activity)
		{
			Browser.Interactions.AssertExists(".schedule-table-container", activity);
		}

		[Then(@"I should not see activity '(.*)' on my schedule table")]
		public void ThenIShouldNotSeeActivityOnMyScheduleTable(string activity)
		{
			Browser.Interactions.AssertNotExists(".schedule-table-container", activity);
		}

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