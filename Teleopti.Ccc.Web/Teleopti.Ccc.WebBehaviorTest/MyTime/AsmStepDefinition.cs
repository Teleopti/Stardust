using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AsmStepDefinition
	{
		private const string attributeUsedForWidth = "padding-left";

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
			Browser.Current.Element(QuicklyFind.ByClass("asm-outer-canvas")).WaitUntilDisplayed();
			Browser.Current.Div(QuicklyFind.ByClass("asm-info-canvas-column-current")).Text.Should().Be.Null();
		}

		[Then(@"ASM link should not be visible")]
		public void ThenASMLinkShouldNotBeVisible()
		{
			EventualAssert.That(() => Pages.Pages.CurrentPortalPage.AsmButton.Exists, Is.False);
		}

		[Then(@"The last layer should be '(.*)' hours long")]
		public void ThenTheLastLayerShouldBeHoursLong(int hours)
		{
			EventualAssert.That(() =>
										{
											var allLayers = Browser.Current.Elements.Filter(QuicklyFind.ByClass("asm-layer"));
											var pxPerHour = pixelsPerHour();
											var theLayerToCheck = allLayers.Last();
											return pixelLength(theLayerToCheck) / pxPerHour;
										}, Is.EqualTo(hours));
		}

		[Then(@"I should see next activity time as '(.*)'")]
		public void ThenIShouldSeeLastActivityStarttimeAs(string startTime)
		{
			Browser.Current.Element(QuicklyFind.ByClass("asm-outer-canvas")).WaitUntilDisplayed();
			EventualAssert.That(() =>
									  Browser.Current.Div(QuicklyFind.ByClass("asm-info-canvas-column-next")).Text,
										Is.StringContaining(startTime));
		}

		[Then(@"I should see a popup with title AgentScheduleMessenger")]
		public void ThenIShouldSeeAPopupWithTitleAgentScheduleMessenger()
		{
			EventualAssert.That(() =>
								Browser.Current.Title.Contains(Resources.AgentScheduleMessenger), 
								Is.True);
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
			EventualAssert.That(() =>
			{
				var slidingSchedules = Browser.Current.Div(QuicklyFind.ByClass("asm-sliding-schedules"));
				var pixelPos = -Convert.ToDouble(slidingSchedules.Style.GetAttributeValue("left").TrimEnd('p', 'x'));
				var holeHours = Math.Floor(pixelPos / pixelsPerHour());
				return holeHours;
			}, Is.EqualTo(hour));
		}

		[Then(@"I shoud see an indication that I have '(.*)' unread messages")]
		public void ThenIShoudSeeAnIndicationThatIHaveUnreadMessages(int unreadMessagesCount)
		{
			EventualAssert.That(() => (Browser.Current.Span(Find.ById("message-count")).Text), Is.EqualTo(unreadMessagesCount.ToString()));
		}

		[Then(@"I shoud not see an indication that I have an unread message")]
		public void ThenIShoudNotSeeAnIndicationThatIHaveAnUnreadMessage()
		{
			EventualAssert.That(() =>Browser.Current.Div(QuicklyFind.ByClass("asm-info-canvas-column-messages")).IsDisplayed(), Is.False);
		}


		private static int pixelLength(Element oneHourLengthLayer)
		{
			return Convert.ToInt32(oneHourLengthLayer.Style.GetAttributeValue(attributeUsedForWidth).TrimEnd('p', 'x'));
		}

		private static int pixelsPerHour()
		{
			const int hackExtra = 1; //due to borde width of hours
			var allHours = Browser.Current.Elements.Filter(QuicklyFind.ByClass("asm-timeline-line"));
			var firstHour = allHours.First();
			if (!firstHour.Exists)
				throw new NotSupportedException("Missing hour to read from");
			return Convert.ToInt32(firstHour.Style.GetAttributeValue("width").TrimEnd('p', 'x')) + hackExtra;
		}
	}
}
