using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class NotifyStepDefinition
	{
		const string notifyTextSelector = "#noty_bottom_layout_container .noty_bar .noty_message .noty_text";

		[When(@"I wait until week schedule is fully loaded")]
		public void WhenIWaitUntilWeekScheduleIsFullyLoaded()
		{
			Browser.Interactions.AssertUrlContains("MyTime#Schedule/Week");
			Browser.Interactions.AssertNotVisibleUsingJQuery("#loading");
		}

		[When(@"An activity '(.*)' with time '(.*)' to '(.*)' is added to my schedule")]
		public void WhenAnActivityWithTimeToIsAddedToMySchedule(string activity, string startTime, string endTime)
		{
			var fetchActivityUrl = "/api/TeamScheduleData/FetchActivities";
			var addActivityUrl = "/api/TeamScheduleCommand/AddActivity";
			var getActivityId = "(function(activities){" +
									"var result;" +
									"activities.forEach(function(activity){" +
										"if(activity.Name == '" + activity + "')" +
											"result = activity.Id; " +
									"});" +
									"return result;" +
								"})(data)";

			var js = "Teleopti.MyTimeWeb.Common.GetUserData(function(user){" +
							"$.ajax({" +
								"url:'"+ fetchActivityUrl + "'," +
								"type:'GET'," +
								"contentType:'application/json'," +
								"success:function(data){" +
									"$.ajax({" +
										"url:'"+ addActivityUrl + "'," +
										"type:'POST'," +
										"contentType:'application/json'," +
										"data:JSON.stringify({" +
												"ActivityId: "+ getActivityId +"," +
												"Date:'"+ startTime.Split(' ')[0] +"', " +
												"StartTime:'"+ startTime + "'," +
												"EndTime:'"+ endTime + "'," +
												"PersonIds:[user.AgentId]" +
										"})" +
									"});" +
								"}" +
							"});" +
						"})";

			Browser.Interactions.Javascript(js);
			Browser.Interactions.AssertAnyContains(".schedule-table-container", activity);
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
	}
}