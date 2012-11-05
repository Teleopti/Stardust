using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AsmStepDefinition
	{
		private const string attributeUsedForWidth = "padding-left";

		[When(@"I click ASM link")]
		public void WhenIClickASMLink()
		{
			TestControllerMethods.Logon();
			Navigation.GotoAsm();
		}

		[Then(@"I should see a schedule in popup")]
		public void ThenIShouldSeeAScheduleInPopup()
		{
			EventualAssert.That(() => Browser.Current.Spans.Filter(Find.ByClass("asm-layer", false)).Count, Is.GreaterThan(0));
		}

		[Then(@"I should see Phone as current activity")]
		public void ThenIShouldSeePhoneAsCurrentActivity()
		{
			EventualAssert.That(() =>
				Browser.Current.Div(Find.ByClass("asm-info-canvas-column-current", false)).Text,
				Is.StringContaining(TestData.ActivityPhone.Description.Name));
		}

		[Then(@"I should not see a current activity")]
		public void ThenIShouldNotSeeAsCurrentActivity()
		{
			Browser.Current.Element(Find.ByClass("asm-outer-canvas", false)).WaitUntilDisplayed();
			Browser.Current.Div(Find.ByClass("asm-info-canvas-column-current", false)).Text.Should().Be.Null();
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
											var allLayers = Browser.Current.Elements.Filter(Find.ByClass("asm-layer", false));
											var pxPerHour = pixelsPerHour();
											var theLayerToCheck = allLayers.Last();
											return pixelLength(theLayerToCheck) / pxPerHour;
										}, Is.EqualTo(hours));
		}

		[Then(@"I should see next activity time as '(.*)'")]
		public void ThenIShouldSeeLastActivityStarttimeAs(string startTime)
		{
			Browser.Current.Element(Find.ByClass("asm-outer-canvas", false)).WaitUntilDisplayed();
			EventualAssert.That(() =>
									  Browser.Current.Div(Find.ByClass("asm-info-canvas-column-next", false)).Text,
										Is.StringContaining(startTime));
		}

		[Then(@"I should see a popup with title AgentScheduleMessenger")]
		public void ThenIShouldSeeAPopupWithTitleAgentScheduleMessenger()
		{
			EventualAssert.That(() =>
								Browser.Current.Title.Contains(Resources.AgentScheduleMessenger), 
								Is.True,
								string.Format("{0} does not contain {1}", Browser.Current.Title, Resources.AgentScheduleMessenger));
		}

		[When(@"My schedule between '(.*)' to '(.*)' change")]
		public void WhenMyScheduleBetweenToChange(DateTime start, DateTime end)
		{
			var xmlStartDate = "D" + XmlConvert.ToString(start, XmlDateTimeSerializationMode.Unspecified);
			var xmlEndDate = "D" + XmlConvert.ToString(end, XmlDateTimeSerializationMode.Unspecified);

			const string js = @"var notification = {{StartDate : '{0}', EndDate : '{1}'}};Teleopti.MyTimeWeb.Asm.NotifyWhenScheduleChangedListener(notification);";

			var formattedJs = string.Format(js, xmlStartDate, xmlEndDate);
			Browser.Current.Eval(formattedJs);
		}

		[Then(@"Now indicator should be at hour '(.*)'")]
		public void ThenNowIndicatorShouldBeAtHour(int hour)
		{
			EventualAssert.That(() =>
			{
				var slidingSchedules = Browser.Current.Div(Find.ByClass("asm-sliding-schedules"));
				var pixelPos = -Convert.ToDouble(slidingSchedules.Style.GetAttributeValue("left").TrimEnd('p', 'x'));
				var holeHours = Math.Floor(pixelPos / pixelsPerHour());
				return holeHours;
			}, Is.EqualTo(hour));
		}

		[When(@"I have an unread message")]
		[Given(@"I have an unread message")]
		public void GivenIHaveAnUnreadMessage()
		{
			ScenarioContext.Current.Pending();
		}

		[When(@"I recieve a new message")]
		public void WhenIRecieveANewMessage()
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I shoud see an indication that I have '(.*)' unread messages")]
		public void ThenIShoudSeeAnIndicationThatIHaveUnreadMessages(int unreadMessagesCount)
		{
			ScenarioContext.Current.Pending();
		}

		[When(@"I click the unread message")]
		public void WhenIClickTheUnreadMessage()
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see a window showing messages")]
		public void ThenIShouldSeeAWindowShowingMessages()
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I shoud see an indication that I have an unread message")]
		public void ThenIShoudSeeAnIndicationThatIHaveAnUnreadMessage()
		{
			ScenarioContext.Current.Pending();
		}

		private static int pixelLength(Element oneHourLengthLayer)
		{
			return Convert.ToInt32(oneHourLengthLayer.Style.GetAttributeValue(attributeUsedForWidth).TrimEnd('p', 'x'));
		}

		private static int pixelsPerHour()
		{
			const int hackExtra = 1; //due to borde width of hours
			var allHours = Browser.Current.Elements.Filter(Find.ByClass("asm-timeline-line", false));
			var firstHour = allHours.First();
			if (!firstHour.Exists)
				throw new NotSupportedException("Missing hour to read from");
			return Convert.ToInt32(firstHour.Style.GetAttributeValue("width").TrimEnd('p', 'x')) + hackExtra;
		}
	}
}
