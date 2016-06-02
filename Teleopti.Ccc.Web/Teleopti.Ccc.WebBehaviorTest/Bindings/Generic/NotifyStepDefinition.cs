﻿using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class NotifyStepDefinition
	{
		const string notifyTextSelector = "#noty_bottom_layout_container .noty_bar .noty_message .noty_text";

		[When(@"An activity with time '(.*)' to '(.*)' is added to my schedule")]
		public void WhenAnActivityWithTimeToIsAddedToMySchedule(string startTime, string endTime)
		{
			var fetchActivityUrl = "/api/TeamScheduleData/FetchActivities";
			var createActivityUrl = "/api/TeamScheduleCommand/AddActivity";
			var requestData = $"ActivityId:d[0].Id, Date:'{startTime.Split(' ')[0]}',StartTime:'{startTime}',EndTime:'{endTime}',PersonIds:[user.AgentId]";

			var javascript = "Teleopti.MyTimeWeb.Common.GetUserData(function(user){$.ajax({url:'" + fetchActivityUrl +
				"',type:'GET',contentType:'application/json',success:function(d){$.ajax({url:'" + createActivityUrl +
				"',type:'POST',contentType:'application/json',data:JSON.stringify({" + requestData + "})});}});});";

			Browser.Interactions.Javascript(javascript);
		}

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