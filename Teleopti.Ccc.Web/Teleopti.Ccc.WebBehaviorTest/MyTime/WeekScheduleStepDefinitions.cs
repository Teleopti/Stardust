using System;
using System.Globalization;
using System.Xml;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;


namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class WeekScheduleStepDefinitions
	{
		[When(@"I click the request symbol for date '(.*)'")]
		public void WhenIClickTheRequestSymbolForDate(DateTime date)
		{
			Browser.Interactions.Click($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .glyphicon-comment");
		}

		[Then(@"I should not see any shifts on date '(.*)'")]
		public void ThenIShouldNotSeeAnyShiftsOnDate(DateTime date)
		{
			Browser.Interactions.AssertNotExists(".weekview-timeline",
				$".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-schedule-layer");
		}

		[Then(@"I should see my week schedule for date '(.*)'")]
		[Then(@"I should see the start and end dates of current week for date '(.*)'")]
		public void ThenIShouldSeeTheStartAndEndDatesOfCurrentWeekForDate(DateTime date)
		{
			AssertShowingWeekForDay(DateHelper.GetFirstDateInWeek(date.Date, DataMaker.Data().MyCulture));
		}

		[Then(@"I should see the start and end dates of current week in Bulgarian culture for date '(.*)'")]
		public void ThenIShouldSeeTheStartAndEndDatesOfCurrentWeekInBulgarianCultureForDate(DateTime date)
		{
			var day = DateHelper.GetFirstDateInWeek(date.Date, DataMaker.Data().MyCulture);
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(day, DataMaker.Data().MyCulture).ToString("d.M.yyyy");
			var lastDayOfWeek = DateHelper.GetLastDateInWeek(day, DataMaker.Data().MyCulture).ToString("d.M.yyyy");

			Browser.Interactions.AssertInputValue($".form-control.text-center.date-input-style", string.Format("{0} Г. - {1} Г.", firstDayOfWeek, lastDayOfWeek));
		}


		[Then(@"I should not see the end of the shift on date '(.*)'")]
		public void ThenIShouldNotSeeTheEndOfTheShiftOnDate(DateTime date)
		{
			AssertDayContainsGivenNumberOfLayers(date, 2);
		}

		[Then(@"I should see the the shift ending at '(.*)' on date '(.*)'")]
		public void ThenIShouldSeeTheTheShiftEndingAtOnDate(string time, DateTime date)
		{
			var endTimeScript =
				$".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-schedule-layer:last .weekview-day-schedule-layer-time:contains('{time}')";
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
			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .glyphicon-exclamation-sign");
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

			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{dateFixed}'] .weekview-day-schedule-layer[style*='top: {layerTop1}']");
			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{dateFixed}'] .weekview-day-schedule-layer[style*='height: {layerHeigth1}']");

			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{dateFixed}'] .weekview-day-schedule-layer[style*='top: {layerTop2}']");
			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{dateFixed}'] .weekview-day-schedule-layer[style*='height: {layerHeigth2}']");

			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{dateFixed}'] .weekview-day-schedule-layer[style*='top: {layerTop3}']");
			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{dateFixed}'] .weekview-day-schedule-layer[style*='height: {layerHeigth3}']");
		}

		[Then(@"I should see overtime availability bar with")]
		public void ThenIShouldSeeOvertimeAvailabilityBarWith(Table table)
		{
			var overtimeAvailability = table.CreateInstance<OvertimeAvailabilityTooltipAndBar>();
			var layerTop =
				$"return $(\".weekview-day[data-mytime-date='{overtimeAvailability.Date:yyyy-MM-dd}'] .overtime-availability-bar\").position().top";
			var layerHeight =
				$"return $(\".weekview-day[data-mytime-date='{overtimeAvailability.Date:yyyy-MM-dd}'] .overtime-availability-bar\").height()";
			var layerWidth =
				$".weekview-day[data-mytime-date='{overtimeAvailability.Date:yyyy-MM-dd}'] .overtime-availability-bar[style*='width: 20%']";
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
			Browser.Interactions.AssertExists(layerWidth); 
		}

		[Then(@"I should see activities on date '(.*)'")]
		public void ThenIShouldSeeActivitiesOnDate(DateTime date)
		{
			Browser.Interactions.AssertExistsUsingJQuery($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-schedule-layer");
		}

		[Then(@"I should see request page")]
		public void ThenIShouldSeeRequestPage()
		{
			Browser.Interactions.AssertUrlContains("Request");
		}

		[When(@"I click the month button")]
		public void WhenIClickTheMonthButton()
		{
			Browser.Interactions.Click(".submenu #week-schedule-month");
		}

		[When(@"I click the next week button")]
		public void WhenIClickTheNextWeekButton()
		{
			Browser.Interactions.Click("#btnNextWeek");
		}

		[When(@"I click the current week button")]
		public void WhenIClickTheCurrentWeekButton()
		{
			Browser.Interactions.Click(".submenu #week-schedule-today");
		}
		
		[Scope(Feature = "View week schedule")]
		[Then(@"I should not see a shift on date '(.*)'")]
		public void ThenIShouldNotSeeAShiftOnDate(DateTime date)
		{
			Browser.Interactions.AssertNotExists($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}']",
				$".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-schedule-layer");
		}
		
		[When(@"My schedule between '(.*)' to '(.*)' reloads")]
		public void WhenMyScheduleBetweenToReloads(DateTime start, DateTime end)
		{
			var xmlStartDate = "D" + XmlConvert.ToString(start, XmlDateTimeSerializationMode.Unspecified);
			var xmlEndDate = "D" + XmlConvert.ToString(end, XmlDateTimeSerializationMode.Unspecified);

			const string js = @"var notification = {{StartDate : '{0}', EndDate : '{1}'}};Teleopti.MyTimeWeb.Schedule.ReloadScheduleListener(notification);";

			var formattedJs = string.Format(js, xmlStartDate, xmlEndDate);
			Browser.Interactions.Javascript_IsFlaky(formattedJs);
		}

		[Then(@"I should see the day summary text for date '(.*)' in '(.*)'")]
		public void ThenIShouldSeeTheDaySummaryTextForDateIn(DateTime date, string color)
		{
			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-summary[style*='color: {color}']");
		}


		[Then(@"I should see the day header text for date '(.*)' is '(.*)'")]
		public void ThenIShouldSeeTheDayHeaderTextForDate(DateTime date, string text)
		{
			Browser.Interactions.AssertAnyContains(
				$".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-header", text);
		}


		[Then(@"I should see the text for date '(.*)' in '(.*)'")]
		public void ThenIShouldSeeTheTextForDateIn(DateTime date, string color)
		{
			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-schedule-layer[style*='color: {color}']");
		}

		[Then(@"I should see an '(.*)' indication for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAnIndicationForChanceOfAbsenceRequestOn(string color, DateTime date)
		{
			Browser.Interactions.AssertNotExists(
				$".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .small-circle[style*='{color}']",
				$".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .holiday-agents[style*='none']");
		}

		[Then(@"I should see an '(.*)' indication of new icon for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAnIndicationOfNewIconForChanceOfAbsenceRequestOn(string probability, DateTime date)
		{
			Browser.Interactions.AssertExists(
				$".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .traffic-light-progress-{probability}");
		}

		[Then(@"I should see an hint '(.*)' for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAnHintForChanceOfAbsenceRequestOn(string text, DateTime date)
		{
			Browser.Interactions.AssertKnockoutContextContains($".weekview-day[data-mytime-date={date:yyyy-MM-dd}]",
				"holidayChanceText", text);
		}

		[Then(@"I should see no indication for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAnEmptyIndicationForChanceOfAbsenceRequestOn(DateTime date)
		{
			Browser.Interactions.AssertNotExistingUsingJQuery($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .holiday-agents");
		}

		[Then(@"I should see no indication of new icon for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAnEmptyIndicationOfNewIconForChanceOfAbsenceRequestOn(DateTime date)
		{
			Browser.Interactions.AssertNotExistingUsingJQuery($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .traffic-light-progress");
		}


		[Then(@"I should not see any indication of how many agents can go on holiday")]
		public void ThenIShouldNotSeeAnyIndicationOfHowManyAgentsCanGoOnHoliday()
		{
			AssertAbsenceIndicators(0);
		}

		[When(@"I select '(.*)' as probability value")]
		public void WhenISelectAsProbabilityValue(string probability)
		{
			Browser.Interactions.ClickUsingJQuery("#probabilityDropdownMenu .dropdown-menu a:contains('{0}')", probability);
			Browser.Interactions.AssertExistsUsingJQuery("#dropdown-probability-type span:contains('{0}')", probability);
		}

		[Then(@"I should see the selected value for probability is '(.*)'")]
		public void ThenIShouldSeeTheSelectedValueForProbabilityIs(string probability)
		{
			Browser.Interactions.AssertExistsUsingJQuery(
				"#dropdown-probability-type span:contains('{0}')", probability);
		}

		[Then(@"I should not see option '(.*)'in probability value list")]
		public void ThenIShouldNotSeeOptionInProbabilityValueList(string probability)
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery($"#probabilityDropdownMenu .dropdown-menu a:contains('{probability}')");
		}

		private void AssertAbsenceIndicators(int visibleIndicatorCount)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.holiday-agents:visible').length",
				visibleIndicatorCount.ToString(CultureInfo.InvariantCulture));
		}

		private void AssertShowingWeekForDay(DateTime anyDayOfWeek)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(anyDayOfWeek, DataMaker.Data().MyCulture).ToString("yyyy-MM-dd");
			var lastDayOfWeek = DateHelper.GetLastDateInWeek(anyDayOfWeek, DataMaker.Data().MyCulture).ToString("yyyy-MM-dd");

			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{firstDayOfWeek}']");
			Browser.Interactions.AssertExists($".weekview-day[data-mytime-date='{lastDayOfWeek}']");
		}

		private void AssertDayContainsGivenNumberOfLayers(DateTime date, int layerCount)
		{
			var script = $"return $('.weekview-day[data-mytime-date=\"{date:yyyy-MM-dd}\"] .weekview-day-schedule-layer').length";
			Browser.Interactions.AssertJavascriptResultContains(script, layerCount.ToString(CultureInfo.InvariantCulture));
		}
	}
}
