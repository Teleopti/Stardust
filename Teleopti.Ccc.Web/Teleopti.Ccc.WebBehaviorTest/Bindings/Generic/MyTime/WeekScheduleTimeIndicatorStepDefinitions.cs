using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class WeekScheduleTimeIndicatorStepDefinitions
	{
		[Given(@"I should see the time indicator at time '(.*)'")]
		[Then(@"I should see the time indicator at time '(.*)'")]
		public void ThenIShouldSeeTheTimeIndicatorAtTime(DateTime date)
		{
			TimeSpan minTimelineTime;
			TimeSpan maxTimelineTime;

			Browser.Interactions.AssertExists(".weekview-timeline-label");

			var result = (string)Browser.Interactions.Javascript_IsFlaky("return $('.weekview-timeline-label').first().html();");
			var startText = result.Split('<')[0];
			var returnCharAt = startText.IndexOf('\n');
			if (returnCharAt>=0)
			{
				startText = startText.Substring(0, returnCharAt+1);
			}
			if (!TimeHelper.TryParse(startText, out minTimelineTime))
			{
				throw new ValidationException("Could not find timeline start label time.");
			}

			result = (string)Browser.Interactions.Javascript_IsFlaky("return $('.weekview-timeline-label').last().html();");
			var endText = result.Split('<')[0];
			returnCharAt = endText.IndexOf('\n');
			if (returnCharAt >= 0)
			{
				endText = endText.Substring(0, returnCharAt+1);
			}
			if (!TimeHelper.TryParse(endText, out maxTimelineTime))
			{
				throw new ValidationException("Could not find timeline end label time.");
			}
			
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-time-indicator", date.ToString("yyyy-MM-dd")));
			Browser.Interactions.AssertExists(".weekview-day-time-indicator-small");
		}

		[Then(@"I should not see the time indicator for date '(.*)'")]
		public void ThenIShouldNotSeeTheTimeIndicatorForDate(DateTime date)
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-time-indicator", date.ToString("yyyy-MM-dd")));
			Browser.Interactions.AssertNotVisibleUsingJQuery(".weekview-day-time-indicator-small");
		}
		
		[Then(@"I should not see the time indicator")]
		public void ThenIShouldNotSeeTheTimeIndicator()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".weekview-day .weekview-day-time-indicator");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".weekview-day-time-indicator-small");
		}
	}
}