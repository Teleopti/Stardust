using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class WeekScheduleStepDefinitions
	{

		private WeekSchedulePage _page { get { return Pages.Pages.WeekSchedulePage; } }

		[When(@"I click the request symbol for date '(.*)'")]
		public void WhenIClickTheRequestSymbolForDate(DateTime date)
		{
            Pages.Pages.WeekSchedulePage.DayElementForDate(date).ListItems.First(Find.ById("day-symbol")).Div(
				QuicklyFind.ByClass("text-request")).EventualClick();
		}

		[Then(@"I should not see any shifts on date '(.*)'")]
		public void ThenIShouldNotSeeAnyShiftsOnDate(DateTime date)
		{
			_page.DayLayers(date).Count.Should().Be.EqualTo(0);
		}

		[Then(@"I should see the start and end dates of current week for date '(.*)'")]
		public void ThenIShouldSeeTheStartAndEndDatesOfCurrentWeekForDate(DateTime date)
		{
			AssertShowingWeekForDay(DateHelper.GetFirstDateInWeek(date.Date, UserFactory.User().Culture));
		}

		[Then(@"I should not see the end of the shift on date '(.*)'")]
		public void ThenIShouldNotSeeTheEndOfTheShiftOnDate(DateTime date)
		{
			_page.DayLayers(date).Count.Should().Be.EqualTo(2);
		}

		[Then(@"I should see the end of the shift on date '(.*)'")]
		public void ThenIShouldSeeTheEndOfTheShiftOnDate(DateTime date)
		{
			var contents = _page.DayElementForDate(date).InnerHtml;

			var indexForShiftEnd = contents.IndexOf(TestData.ActivityPhone.Description.Name);

			indexForShiftEnd.Should().Be.GreaterThan(-1);
		}

		[Then(@"I should see the start of the shift on date '(.*)'")]
		public void ThenIShouldSeeTheStartOfTheShiftOnDate(DateTime date)
		{
			_page.DayLayers(date).Count.Should().Be.EqualTo(2);
		}

		[When(@"I hover layer '(.*)' on '(.*)'")]
		public void WhenIHoverLayerOn(int layer, DateTime date)
		{
			DivCollection layers = _page.DayLayers(date);
			layers[layer-1].FireEvent("mouseenter");
			layers[layer-1].FireEvent("mouseover");
		}

		[Then(@"I should see the meeting details with description '(.*)' on date '(.*)'")]
		public void ThenIShouldSeeTheMeetingDetailsWithDescriptionOnDate(string description, DateTime date)
		{
			EventualAssert.That(() => _page.DayLayerTooltipElement(date, description).Exists, Is.True);
		}

		[Then(@"I should see the public note on date '(.*)'")]
		public void ThenIShouldSeeThePublicNoteOnDate(DateTime date)
		{
			EventualAssert.That(() => _page.DayComment(date).Exists, Is.True);
		}

		[Then(@"I should see number '(.*)' with the request count for date '(.*)'")]
		public void ThenIShouldSeeNumberWithTheRequestCountForDate(int requestCount, DateTime date)
		{
			var request = _page.RequestForDate(date);
			EventualAssert.That(() => request.InnerHtml, Is.StringContaining(requestCount.ToString()));
		}

		[When(@"I open the weekschedule date-picker")]
		public void WhenIOpenTheWeekscheduleDate_Picker()
		{
			Browser.Interactions.Click(".icon-th:enabled");
		}

		[Then(@"I should see '(.*)' as the first day in the calender")]
		public void ThenIShouldSeeAsTheFirstDayInTheCalender(string day)
		{
			Browser.Interactions.AssertContains(".datepicker-days th.dow:nth-child(1)", day);
		}


		[Then(@"I should see start timeline and end timeline according to schedule with:")]
		public void ThenIShouldSeeStartTimelineAndEndTimelineAccordingToScheduleWith(Table table)
		{
			_page.AnyTimelineLabel.WaitUntilExists();
			var divs = _page.TimelineLabels;
			EventualAssert.That(() => divs[0].InnerHtml.Split('<')[0] + divs[1].InnerHtml.Split('<')[0], Is.StringContaining(table.Rows[0][1]));
			EventualAssert.That(() => divs[divs.Count - 1].InnerHtml.Split('<')[0] + divs[divs.Count - 2].InnerHtml.Split('<')[0], Is.StringContaining(table.Rows[1][1]));
			var count = int.Parse(table.Rows[2][1]);
			EventualAssert.That(() => divs.Count, Is.InRange(count, count + 2));
		}

		[Then(@"I should see activities on date '(.*)' with:")]
		public void ThenIShouldSeeActivitiesOnDateWith(DateTime date, Table table)
		{
			DivCollection layers = _page.DayLayers(date);

			EventualAssert.That(() => layers[0].Style.GetAttributeValue("Top"), Is.EqualTo("16px"));
			EventualAssert.That(() => layers[0].Style.GetAttributeValue("Height"), Is.EqualTo("190px"));

			EventualAssert.That(() => layers[1].Style.GetAttributeValue("Top"), Is.EqualTo("207px"));
			EventualAssert.That(() => layers[1].Style.GetAttributeValue("Height"), Is.EqualTo("62px"));

			EventualAssert.That(() => layers[2].Style.GetAttributeValue("Top"), Is.EqualTo("270px"));
			EventualAssert.That(() => layers[2].Style.GetAttributeValue("Height"), Is.EqualTo("381px"));
		}

		[Then(@"I should see activities on date '(.*)'")]
		public void ThenIShouldSeeActivitiesOnDate(DateTime date)
		{
			EventualAssert.That(() => _page.DayLayers(date), Is.Not.Empty);
		}

		[Then(@"I should see request page")]
		public void ThenIShouldSeeRequestPage()
		{
			EventualAssert.That(() => Browser.Current.Url, Is.StringContaining("Request"));
		}

		[When(@"I click the current week button")]
		public void WhenIClickTheCurrentWeekButton()
		{
			Pages.Pages.WeekSchedulePage.TodayButton.EventualClick();
		}

		[Then(@"I should see a shift on date '(.*)'")]
		public void ThenIShouldSeeAShiftOnDate(DateTime date)
		{
			_page.DayLayers(date).Count.Should().Be.EqualTo(1);
		}

		[Then(@"I should not see a shift on date '(.*)'")]
		public void ThenIShouldNotSeeAShiftOnDate(DateTime date)
		{
			_page.DayLayers(date).Count.Should().Be.EqualTo(0);
		}

		[When(@"My schedule between '(.*)' to '(.*)' reloads")]
		public void WhenMyScheduleBetweenToReloads(DateTime start, DateTime end)
		{
			var xmlStartDate = "D" + XmlConvert.ToString(start, XmlDateTimeSerializationMode.Unspecified);
			var xmlEndDate = "D" + XmlConvert.ToString(end, XmlDateTimeSerializationMode.Unspecified);

			const string js = @"var notification = {{StartDate : '{0}', EndDate : '{1}'}};Teleopti.MyTimeWeb.Schedule.ReloadScheduleListener(notification);";

			var formattedJs = string.Format(js, xmlStartDate, xmlEndDate);
			Browser.Interactions.Javascript(formattedJs);
		}

		[Then(@"I should see the day summary text for date '(.*)' in '(.*)'")]
		public void ThenIShouldSeeTheDaySummaryTextForDateIn(DateTime date, string color)
		{
			_page.DayElementForDate(date).ListItems.First(Find.ById("day-summary")).Style.Color.ToString().ToLower().Should().Contain(
				color);
		}

		[Then(@"I should see the text for date '(.*)' in '(.*)'")]
		public void ThenIShouldSeeTheTextForDateIn(DateTime date, string color)
		{
			DivCollection layers = _page.DayLayers(date);

			EventualAssert.That(() => layers[0].Style.GetAttributeValue("color").ToLower(), Is.StringContaining(color));
		}

		[Then(@"I should not see any indication of how many agents can go on holiday")]
		public void ThenIShouldNotSeeAnyIndicationOfHowManyAgentsCanGoOnHoliday()
		{
			var indicators = Pages.Pages.WeekSchedulePage.AbsenceIndiciators();
			foreach (var indicator in indicators)
			{
				EventualAssert.That(()=>indicator.IsDisplayed(), Is.False);
			}
		}

		[Then(@"I should see an indication of the amount of agents that can go on holiday on each day of the week")]
		public void ThenIShouldSeeAnIndicationOfTheAmountOfAgentsThatCanGoOnHolidayOnEachDayOfTheWeek()
		{
			var indicators = Pages.Pages.WeekSchedulePage.AbsenceIndiciators();
			foreach (var indicator in indicators)
			{
				EventualAssert.That(()=>indicator.IsDisplayed(), Is.True);
			}
		}
		[Then(@"I should see an '(.*)' indication for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAnIndicationForChanceOfAbsenceRequestOn(string color, DateTime date)
		{
			var layers = _page.DayLayers(date);
			var background = _page.DayElementForDate(date).Divs.Filter(Find.ByClass("small-circle"))[0].Style.BackgroundColor;
			
			EventualAssert.That(() => background.ToHexString, Is.EqualTo(new HtmlColor(color).ToHexString));
		}
		private void AssertShowingWeekForDay(DateTime anyDayOfWeek)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(anyDayOfWeek, UserFactory.User().Culture);
			var lastDayOfWeek = DateHelper.GetLastDateInWeek(anyDayOfWeek, UserFactory.User().Culture);
			EventualAssert.That(() => _page.FirstDay.GetAttributeValue("data-mytime-date"), Is.EqualTo(firstDayOfWeek.ToString("yyyy-MM-dd")));
			EventualAssert.That(() => _page.SeventhDay.GetAttributeValue("data-mytime-date"), Is.EqualTo(lastDayOfWeek.ToString("yyyy-MM-dd")));
		}
	}
}
