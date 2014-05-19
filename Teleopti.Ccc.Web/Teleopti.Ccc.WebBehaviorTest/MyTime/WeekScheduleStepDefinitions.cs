using System;
using System.Globalization;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class WeekScheduleStepDefinitions
    {

		[When(@"I click the request symbol for date '(.*)'")]
		public void WhenIClickTheRequestSymbolForDate(DateTime date)
		{
			Browser.Interactions.Click(string.Format(".weekview-day[data-mytime-date='{0}'] .glyphicon-comment", date.ToString("yyyy-MM-dd")));
		}

		[Then(@"I should not see any shifts on date '(.*)'")]
		public void ThenIShouldNotSeeAnyShiftsOnDate(DateTime date)
		{
			Browser.Interactions.AssertNotExists(".weekview-timeline",
			                                     string.Format(
				                                     "'.weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer'",
				                                     date.ToString("yyyy-MM-dd")));
		}

		[Then(@"I should see my week schedule for date '(.*)'")]
		[Then(@"I should see the start and end dates of current week for date '(.*)'")]
		public void ThenIShouldSeeTheStartAndEndDatesOfCurrentWeekForDate(DateTime date)
		{
			AssertShowingWeekForDay(DateHelper.GetFirstDateInWeek(date.Date, DataMaker.Data().MyCulture));
		}

		[Then(@"I should not see the end of the shift on date '(.*)'")]
		public void ThenIShouldNotSeeTheEndOfTheShiftOnDate(DateTime date)
		{
			AssertDayContainsGivenNumberOfLayers(date, 2);
		}

		[Then(@"I should see the the shift ending at '(.*)' on date '(.*)'")]
		public void ThenIShouldSeeTheTheShiftEndingAtOnDate(string time, DateTime date)
		{
			var activityNameScript =
				string.Format(
					".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer:last .weekview-day-schedule-layer-activity:contains('{1}')",
					date.ToString("yyyy-MM-dd"), 
					TestData.ActivityPhone.Description.Name);
			var endTimeScript =
				string.Format(
					".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer:last .weekview-day-schedule-layer-time:contains('{1}')",
					date.ToString("yyyy-MM-dd"), 
					time);
			Browser.Interactions.AssertExistsUsingJQuery(activityNameScript);
			Browser.Interactions.AssertExistsUsingJQuery(endTimeScript);
		}

		[Then(@"I should see a shift on date '(.*)'")]
		public void ThenIShouldSeeAShiftOnDate(DateTime date)
		{
			AssertDayContainsGivenNumberOfLayers(date, 1);
		}

		[Then(@"I should see the public note on date '(.*)'")]
		public void ThenIShouldSeeThePublicNoteOnDate(DateTime date)
		{
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .glyphicon-exclamation-sign", date.ToString("yyyy-MM-dd")));
		}
		
		[When(@"I open the weekschedule date-picker")]
		public void WhenIOpenTheWeekscheduleDate_Picker()
		{
			Browser.Interactions.Click(".glyphicon-calendar:enabled");
		}

		[Then(@"I should see '(.*)' as the first day in the calender")]
		public void ThenIShouldSeeAsTheFirstDayInTheCalender(string day)
		{
			Browser.Interactions.AssertFirstContains(".datepicker-days th.dow:nth-child(1)", day);
		}


		[Then(@"I should see start timeline and end timeline according to schedule with:")]
		public void ThenIShouldSeeStartTimelineAndEndTimelineAccordingToScheduleWith(Table table)
		{
			var startTime = table.Rows[0][1];
			var endTime = table.Rows[1][1];
			var timeLineCount = table.Rows[2][1];

			Browser.Interactions.AssertJavascriptResultContains("return $('.weekview-timeline-label:visible:first').text()", startTime);
			Browser.Interactions.AssertJavascriptResultContains("return $('.weekview-timeline-label:visible:last').text()", endTime);
			Browser.Interactions.AssertJavascriptResultContains("return $('.weekview-timeline-label:visible').length", timeLineCount);
		}

		[Then(@"I should see activities on date '(.*)' with:")]
		public void ThenIShouldSeeActivitiesOnDateWith(DateTime date, Table table)
		{
			const string layerTop1 = "16px";
			const string layerHeigth1 = "192px";
				
			const string layerTop2 = "207px";
			const string layerHeigth2 = "64px";

			const string layerTop3 = "270px";
			const string layerHeigth3 = "383px";
			var dateFixed = date.ToString("yyyy-MM-dd");

			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer[style*='top: {1}']", dateFixed, layerTop1));
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer[style*='height: {1}']", dateFixed, layerHeigth1));

			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer[style*='top: {1}']", dateFixed, layerTop2));
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer[style*='height: {1}']", dateFixed, layerHeigth2));

			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer[style*='top: {1}']", dateFixed, layerTop3));
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer[style*='height: {1}']", dateFixed, layerHeigth3));
		}

		[Then(@"I should see overtime availability bar with")]
		public void ThenIShouldSeeOvertimeAvailabilityBarWith(Table table)
		{
			var overtimeAvailability = table.CreateInstance<OvertimeAvailabilityTooltipAndBar>();
			var layerTop =
				string.Format("return $(\".weekview-day[data-mytime-date='{0}'] .overtime-availability-bar\").position().top",
				              overtimeAvailability.Date.ToString("yyyy-MM-dd"));
			var layerHeight =
				string.Format("return $(\".weekview-day[data-mytime-date='{0}'] .overtime-availability-bar\").height()",
							  overtimeAvailability.Date.ToString("yyyy-MM-dd"));
			var layerWidth =
				string.Format("return $(\".weekview-day[data-mytime-date='{0}'] .overtime-availability-bar\").width()",
							  overtimeAvailability.Date.ToString("yyyy-MM-dd"));
			if (overtimeAvailability.Date.Day == 20)
			{
				Browser.Interactions.AssertJavascriptResultContains(layerTop,"111");
				Browser.Interactions.AssertJavascriptResultContains(layerHeight,"445");
			}
			if (overtimeAvailability.Date.Day == 21)
			{
				Browser.Interactions.AssertJavascriptResultContains(layerTop, "459");
				Browser.Interactions.AssertJavascriptResultContains(layerHeight, "208");
			}
			if (overtimeAvailability.Date.Day == 22)
			{
				Browser.Interactions.AssertJavascriptResultContains(layerTop, "0");
				Browser.Interactions.AssertJavascriptResultContains(layerHeight, "89");
			}
			Browser.Interactions.AssertJavascriptResultContains(layerWidth, "20");
		}

		[Then(@"I should see activities on date '(.*)'")]
		public void ThenIShouldSeeActivitiesOnDate(DateTime date)
		{
			Browser.Interactions.AssertExistsUsingJQuery(
				string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer",date.ToString("yyyy-MM-dd")));
		}

		[Then(@"I should see request page")]
		public void ThenIShouldSeeRequestPage()
		{
			Browser.Interactions.AssertUrlContains("Request");
		}

		[When(@"I click the current week button")]
		public void WhenIClickTheCurrentWeekButton()
		{
			Browser.Interactions.Click(".hidden-sm #week-schedule-today");
		}
		
		[Scope(Feature = "View week schedule")]
		[Then(@"I should not see a shift on date '(.*)'")]
		public void ThenIShouldNotSeeAShiftOnDate(DateTime date)
		{
			Browser.Interactions.AssertNotExists(string.Format(".weekview-day[data-mytime-date='{0}']", date.ToString("yyyy-MM-dd")),
												 string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer",
			                                                   date.ToString("yyyy-MM-dd")));
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
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-summary[style*='color: {1}']", date.ToString("yyyy-MM-dd"), color));
		}

		[Then(@"I should see the text for date '(.*)' in '(.*)'")]
		public void ThenIShouldSeeTheTextForDateIn(DateTime date, string color)
		{
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-schedule-layer[style*='color: {1}']", date.ToString("yyyy-MM-dd"), color));
		}

		[Then(@"I should see an '(.*)' indication for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAnIndicationForChanceOfAbsenceRequestOn(string color, DateTime date)
		{
			Browser.Interactions.AssertNotExists(
				string.Format(".weekview-day[data-mytime-date='{0}'] .small-circle[style*='{1}']", date.ToString("yyyy-MM-dd"), color), 
				string.Format(".weekview-day[data-mytime-date='{0}'] .holiday-agents[style*='none']", date.ToString("yyyy-MM-dd")));
		}

		[Then(@"I should not see any indication of how many agents can go on holiday")]
		public void ThenIShouldNotSeeAnyIndicationOfHowManyAgentsCanGoOnHoliday()
		{
			AssertAbsenceIndicators(0);
		}

		private void AssertAbsenceIndicators(int visibleIndicatorCount)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.holiday-agents:visible').length",
																visibleIndicatorCount.ToString(
																	CultureInfo.InvariantCulture));
		}

		private void AssertShowingWeekForDay(DateTime anyDayOfWeek)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(anyDayOfWeek, DataMaker.Data().MyCulture).ToString("yyyy-MM-dd");
			var lastDayOfWeek = DateHelper.GetLastDateInWeek(anyDayOfWeek, DataMaker.Data().MyCulture).ToString("yyyy-MM-dd");

			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}']", firstDayOfWeek));
			Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}']", lastDayOfWeek));
		}

		private void AssertDayContainsGivenNumberOfLayers(DateTime date, int layerCount)
		{
			var script = string.Format("return $('.weekview-day[data-mytime-date=\"{0}\"] .weekview-day-schedule-layer').length", date.ToString("yyyy-MM-dd"));
			Browser.Interactions.AssertJavascriptResultContains(script, layerCount.ToString(CultureInfo.InvariantCulture));
		}

	}
}
