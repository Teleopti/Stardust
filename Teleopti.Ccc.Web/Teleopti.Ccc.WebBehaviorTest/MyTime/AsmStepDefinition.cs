using System;
using System.Globalization;
using System.Text;
using System.Xml;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AsmStepDefinition
	{
		[Then(@"I should see a schedule in popup")]
		public void ThenIShouldSeeAScheduleInPopup()
		{
			Browser.Interactions.AssertExists("span.asm-layer");
		}

		[Then(@"I should see (.*) as current activity")]
		public void ThenIShouldSeePhoneAsCurrentActivity(string activity)
		{
			Browser.Interactions.AssertFirstContains("div.asm-info-canvas-column-current", activity);
		}

		[Then(@"I should not see a current activity")]
		public void ThenIShouldNotSeeAsCurrentActivity()
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".asm-outer-canvas");
			Browser.Interactions.AssertJavascriptResultContains("return $('.asm-info-canvas-column-current').text().length", "0");
		}

		[Then(@"ASM link should not be visible")]
		public void ThenASMLinkShouldNotBeVisible()
		{
			Browser.Interactions.AssertNotExists("#regional-settings", "#asm-link");
		}

		[Then(@"The last layer should be '(.*)' hours long")]
		public void ThenTheLastLayerShouldBeHoursLong(int hours)
		{
			var script = new StringBuilder();
			script.AppendLine("var fixForBorderWidth=1;");
			script.AppendLine("var lastLayerWidth=$('.asm-layer:last').css('padding-left').replace('px', '');");
			script.AppendLine("var pixelPerHour=$('.asm-timeline-line:first').width()+fixForBorderWidth;");
			script.AppendLine("return lastLayerWidth/pixelPerHour;");

			Browser.Interactions.AssertJavascriptResultContains(script.ToString(), hours.ToString(CultureInfo.InvariantCulture));
		}

		[Then(@"I should see next activity time as '(.*)'")]
		public void ThenIShouldSeeLastActivityStarttimeAs(string startTime)
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".asm-outer-canvas");
			Browser.Interactions.AssertFirstContains(".asm-info-canvas-column-next", startTime);
		}

		[Then(@"I should see a popup with title AgentScheduleMessenger")]
		public void ThenIShouldSeeAPopupWithTitleAgentScheduleMessenger()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('title').text()", Resources.AgentScheduleMessenger);
		}

		[When(@"My schedule between '(.*)' to '(.*)' change")]
		public void WhenMyScheduleBetweenToChange(DateTime start, DateTime end)
		{
			var xmlStartDate = "D" + XmlConvert.ToString(start, XmlDateTimeSerializationMode.Unspecified);
			var xmlEndDate = "D" + XmlConvert.ToString(end, XmlDateTimeSerializationMode.Unspecified);

			const string js = @"var notification = {{StartDate : '{0}', EndDate : '{1}'}};Teleopti.MyTimeWeb.Asm.NotifyWhenScheduleChangedListener(notification);";

			var formattedJs = string.Format(js, xmlStartDate, xmlEndDate);
			Browser.Interactions.Javascript(formattedJs);
		}

		[Then(@"Now indicator should be at hour '(.*)'")]
		public void ThenNowIndicatorShouldBeAtHour(int hour)
		{
			var script = new StringBuilder();
			script.AppendLine("var fixForBorderWidth=1;");
			script.AppendLine("var pixelPos=$('.asm-sliding-schedules').css('left').replace('px', '').replace('-', '');");
			script.AppendLine("var pixelPerHour=$('.asm-timeline-line:first').width()+fixForBorderWidth;");
			script.AppendLine("return Math.floor(pixelPos/pixelPerHour);");

			Browser.Interactions.AssertJavascriptResultContains(script.ToString(), hour.ToString(CultureInfo.InvariantCulture));
		}

		[Then(@"I shoud see an indication that I have '(.*)' unread messages")]
		public void ThenIShoudSeeAnIndicationThatIHaveUnreadMessages(int unreadMessagesCount)
		{
			Browser.Interactions.AssertFirstContains("#message-count", unreadMessagesCount.ToString(CultureInfo.InvariantCulture));
		}

		[Then(@"I shoud not see an indication that I have an unread message")]
		public void ThenIShoudNotSeeAnIndicationThatIHaveAnUnreadMessage()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".asm-info-canvas-column-messages");
		}
	}
}