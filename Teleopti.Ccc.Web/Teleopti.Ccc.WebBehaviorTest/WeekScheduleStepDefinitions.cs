﻿using System;
using System.Globalization;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
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

		[When(@"I click on any day of a week")]
		public void WhenIClickOnAnyDayOfAWeek()
		{
			_page.ClickThirdDayOfOtherWeekInWeekPicker(UserFactory.User().Culture);
		}

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

		[Then(@"I should see the meeting details with subject '(.*)' on date '(.*)'")]
		public void ThenIShouldSeeTheMeetingDetailsOnDate(string subject, DateTime date)
		{
			EventualAssert.That(() => _page.DayLayerTooltipElement(date, subject).Exists, Is.True);
		}

		[Then(@"I should see the public note on date '(.*)'")]
		public void ThenIShouldSeeThePublicNoteOnDate(DateTime date)
		{
			EventualAssert.That(() => _page.DayComment(date).Exists, Is.True);
		}


		[Then(@"I should see the selected week")]
		public void ThenIShouldSeeTheSelectedWeek()
		{
			AssertShowingWeekForDay(TestDataSetup.ThirdDayOfOtherThanCurrentWeekInCurrentMonth(UserFactory.User().Culture));
		}

		[Then(@"I should see monday as the first day of week")]
		public void ThenIShouldSeeMondayAsTheFirstDayOfWeek()
		{
			_page.DatePicker.CalendarFirstDayNumbers.Should().Contain(TestDataSetup.FirstDayOfAnyWeekInCurrentMonth(UserFactory.User().Culture).Day);
		}

		[Then(@"I should see sunday as the first day of week")]
		public void ThenIShouldSeeSundayAsTheFirstDayOfWeek()
		{
			_page.DatePicker.CalendarFirstDayNumbers.Should().Contain(TestDataSetup.FirstDayOfAnyWeekInCurrentMonth(UserFactory.User().Culture).Day);
		}

		[Then(@"I should see number '(.*)' with the request count for date '(.*)'")]
		public void ThenIShouldSeeNumberWithTheRequestCountForDate(int requestCount, DateTime date)
		{
			var request = _page.RequestForDate(date);
			EventualAssert.That(() => request.InnerHtml, Is.StringContaining(requestCount.ToString()));
		}

		[Then(@"I should see start timeline and end timeline according to schedule with:")]
		public void ThenIShouldSeeStartTimelineAndEndTimelineAccordingToScheduleWith(Table table)
		{
			var divs = _page.TimelineLabels;
			EventualAssert.That(() => divs[0].InnerHtml, Is.StringContaining(table.Rows[0][1]));
			EventualAssert.That(() => divs[divs.Count - 1].InnerHtml, Is.StringContaining(table.Rows[1][1]));
			EventualAssert.That(() => string.Format("{0}", divs.Count), Is.EqualTo(table.Rows[2][1]));
		}

		[Then(@"I should see activities on date '(.*)' with:")]
		public void ThenIShouldSeeActivitiesOnDateWith(DateTime date, Table table)
		{
			DivCollection layers = _page.DayLayers(date);
			
			EventualAssert.That(() => layers[0].GetAttributeValue("tooltip-text"), Is.EqualTo(table.Rows[0][1]));
			EventualAssert.That(() => layers[0].Style.GetAttributeValue("Top"), Is.EqualTo("0px"));
			EventualAssert.That(() => layers[0].Style.GetAttributeValue("Height"), Is.EqualTo("199px"));

			EventualAssert.That(() => layers[1].GetAttributeValue("tooltip-text"), Is.EqualTo(table.Rows[1][1]));
			EventualAssert.That(() => layers[1].Style.GetAttributeValue("Top"), Is.EqualTo("200px"));
			EventualAssert.That(() => layers[1].Style.GetAttributeValue("Height"), Is.EqualTo("66px"));

			EventualAssert.That(() => layers[2].GetAttributeValue("tooltip-text"), Is.EqualTo(table.Rows[2][1]));
			EventualAssert.That(() => layers[2].Style.GetAttributeValue("Top"), Is.EqualTo("267px"));
			EventualAssert.That(() => layers[2].Style.GetAttributeValue("Height"), Is.EqualTo("400px"));
		}

		[Then(@"I should see activities on date '(.*)'")]
		public void ThenIShouldSeeActivitiesOnDate(DateTime date)
		{
			EventualAssert.That(() => _page.DayLayers(date), Is.Not.Empty);
		}

		[Then(@"I should see request page")]
		public void ThenIShouldSeeRequestPage()
		{
			EventualAssert.That(() => Pages.Pages.Current.Document.Uri.AbsoluteUri, Is.StringContaining("Request"));
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
			Browser.Current.Eval(formattedJs);
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
