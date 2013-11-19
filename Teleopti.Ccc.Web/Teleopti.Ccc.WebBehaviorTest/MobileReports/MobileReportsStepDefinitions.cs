using System;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.MobileReports
{

	[Binding]
	public class MobileReportsStepDefinitions
	{
		private DateTime _clickedDateInDatePicker;

		[Then(@"I should see Mobile Reports")]
		public void ThenIShouldSeeMobileReports()
		{
			Browser.Interactions.AssertExists("#report-settings-view");
		}

		[Given(@"I have skill analytics data")]
		public void GivenIHaveSkillAnalyticsData()
		{
			var timeZones = DataMaker.Data().UserData<ITimeZoneData>();
			var dataSource = DataMaker.Data().UserData<IDatasourceData>();
			var businessUnits = new BusinessUnit(TestData.BusinessUnit, dataSource);
			DataMaker.Analytics().Setup(businessUnits);
			DataMaker.Analytics().Setup(new ThreeSkills(timeZones, businessUnits, dataSource));
		}

		[Given(@"I have analytics fact queue data")]
		public void GivenIHaveFactQueueDataForAWeek()
		{
			var timeZones = DataMaker.Data().UserData<ITimeZoneData>();
			var dataSource = DataMaker.Data().UserData<IDatasourceData>();
			var queues = new AQueue(dataSource);
			var businessUnits = new BusinessUnit(TestData.BusinessUnit, dataSource);
			DataMaker.Analytics().Setup(queues);
			DataMaker.Analytics().Setup(businessUnits);
			var skills = new ThreeSkills(timeZones, businessUnits, dataSource);
			DataMaker.Analytics().Setup(skills);
			var workloads = new AWorkload(skills, timeZones, businessUnits, dataSource);
			DataMaker.Analytics().Setup(workloads);
			DataMaker.Analytics().Setup(new FillBridgeQueueWorkloadFromData(workloads, queues, businessUnits, dataSource));
			var intervals = DataMaker.Data().UserData<IIntervalData>();
			var bridgeTimeZones = DataMaker.Data().UserData<IBridgeTimeZone>();
			var dates = DataMaker.Data().UserData<IDateData>();
			DataMaker.Analytics().Setup(new FactQueue(dates, intervals, queues, dataSource, bridgeTimeZones));
			DataMaker.Analytics().Setup(new WeekdayTranslations());
		}

		[When(@"I click the signout button")]
		public void GivenIClickSignoutButton()
		{
			Browser.Interactions.Click("#signout-button");
		}

		[Then(@"I should see a report")]
		public void ThenIShouldSeAReport()
		{
			Browser.Interactions.AssertExists("body:not(.ui-mobile-viewport-transitioning) #report-view.ui-page-active");
		}

		[Then(@"I should see a graph")]
		public void ThenIShouldSeeAGraph()
		{
			Browser.Interactions.AssertExists("body:not(.ui-mobile-viewport-transitioning) #report-view.ui-page-active #report-graph-holder");
			Browser.Interactions.AssertExists("body:not(.ui-mobile-viewport-transitioning) #report-view.ui-page-active #report-graph-canvas");
		}

		[Then(@"I should see a report for next date")]
		public void ThenIShouldSeeAReportForNextDate()
		{
			var expected = DateOnlyForBehaviorTests.TestToday.AddDays(1).ToShortDateString(DataMaker.Data().MyCulture);
			Browser.Interactions.AssertFirstContains("#report-view-date-nav-current", expected);
		}

		[Then(@"I should see a report for previous date")]
		public void ThenIShouldSeeAReportForPreviousDate()
		{
			var expected = DateOnlyForBehaviorTests.TestToday.AddDays(-1).ToShortDateString(DataMaker.Data().MyCulture);
			Browser.Interactions.AssertFirstContains("#report-view-date-nav-current", expected);
		}

		[Then(@"I should see a table")]
		public void ThenIShouldSeeATable()
		{
			Browser.Interactions.AssertExists("#report-table-holder tr");
		}

		[Then(@"I should see friendly error message")]
		public void ThenIShouldSeeFriendlyErrorMessage()
		{
			Browser.Interactions.AssertFirstContains("#error-view", "No access");
		}

		[Then(@"I should see ReportSettings")]
		public void ThenIShouldSeeReportSettings()
		{
			Browser.Interactions.AssertExists("body:not(.ui-mobile-viewport-transitioning) #report-settings-view.ui-page-active");
		}

		[Then(@"I should see ReportSettings with default value")]
		public void ThenIShouldSeeReportSettingsWithDefaultValue()
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#sel-date", DateOnlyForBehaviorTests.TestToday.AddDays(-1).ToShortDateString(DataMaker.Data().MyCulture));
			Browser.Interactions.AssertFirstContains("#sel-skill-menu", Resources.All);
			Browser.Interactions.AssertExists("#report-settings-type-graph:checked");
			Browser.Interactions.AssertExists("#report-settings-type-table:not(:checked)");
			Browser.Interactions.AssertExists("#report-settings-interval-week:not(:checked)");
		}


		[Then(@"I should see the selected date")]
		public void ThenIShouldSeeTheSelectedDate()
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#sel-date", _clickedDateInDatePicker.ToShortDateString(DataMaker.Data().MyCulture));
		}

		[Then(@"I should see the selected skill")]
		public void ThenIShouldSeeTheSelectedSkill()
		{
			var skillName = DataMaker.Data().UserData<ThreeSkills>().Skill2Name;
			Browser.Interactions.AssertFirstContains("#sel-skill-button", skillName);
		}

		[Then(@"I should only see reports i have access to")]
		public void ThenIShouldonlySeeReportsIHaveAccessTo()
		{
			Browser.Interactions.AssertNotExists(".ui-radio:nth-child(2) input[name='sel-report']", ".ui-radio:nth-child(3) input[name='sel-report']");
		}

		[Then(@"the date-picker should close")]
		public void ThenTheDatePickerShouldClose()
		{
			Browser.Interactions.AssertNotExists("#report-settings-view", ".ui-datebox-container");
		}

		[Given(@"I am viewing a report")]
		public void WhenIAmViewAReport()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileReportsSettings();

			ThenIShouldSeeReportSettings();
			WhenISelectDateToday();
			SelectGetAnsweredAndAbondonedReport();
			WhenICheckTypeTable();
			WhenIClickViewReportButton();
		}

		private static void SelectGetAnsweredAndAbondonedReport()
		{
			Browser.Interactions.Click("#sel-report-GetAnsweredAndAbandoned + label");
		}

		[When(@"I view a report with week data")]
		public void WhenIViewAReportWithWeekData()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileReportsSettings();

			ThenIShouldSeeReportSettings();
			WhenISelectDateToday();
			SelectGetAnsweredAndAbondonedReport();
			Browser.Interactions.Click("#report-settings-interval-week + label");
			WhenICheckTypeTable();
			WhenIClickViewReportButton();
		}

		[When(@"I check type Graph")]
		public void WhenICheckTypeGraph()
		{
			Browser.Interactions.Javascript("$('#report-settings-type-graph').prop('checked', true).checkboxradio('refresh');");
		}

		[When(@"I check type Table")]
		public void WhenICheckTypeTable()
		{
			Browser.Interactions.Javascript("$('#report-settings-type-table').prop('checked', true).checkboxradio('refresh');");
		}

		[When(@"I click next date")]
		public void WhenIClickNextDate()
		{
			// before click next date, make sure current is today
			var expected = DateOnlyForBehaviorTests.TestToday.ToShortDateString(DataMaker.Data().MyCulture);
			Browser.Interactions.AssertFirstContains("#report-view-date-nav-current", expected);
			Browser.Interactions.Click("#report-view-date-nav-next");
		}

		[When(@"I click on any date")]
		public void WhenIClickOnAnyDate()
		{
            _clickedDateInDatePicker = DateOnlyForBehaviorTests.TestToday.AddDays(2);
            if (_clickedDateInDatePicker.Month != DateOnlyForBehaviorTests.TestToday.AddDays(-1).Month)
                _clickedDateInDatePicker = DateOnlyForBehaviorTests.TestToday.AddDays(-2);
			Browser.Interactions.ClickContaining(".ui-datebox-griddate.ui-btn-up-d", _clickedDateInDatePicker.Day.ToString());
		}

		[When(@"I click previous date")]
		public void WhenIClickPreviousDate()
		{
			// before click previous date, make sure current is today
            var expected = DateOnlyForBehaviorTests.TestToday.ToShortDateString(DataMaker.Data().MyCulture);
			Browser.Interactions.AssertFirstContains("#report-view-date-nav-current", expected);
			Browser.Interactions.Click("#report-view-date-nav-prev");
		}

		[When(@"I click View Report Button")]
		public void WhenIClickViewReportButton()
		{
			Browser.Interactions.Click("body:not(.ui-mobile-viewport-transitioning) #report-view-show-button");
		}

		[Given(@"I view MobileReports")]
		[When(@"I view MobileReports")]
		public void WhenIEnterMobileReports()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileReportsPage();
		}

		[When(@"I open the date-picker")]
		public void WhenIOpenTheDatePicker()
		{
			Browser.Interactions.Click("#sel-date + a");
			Browser.Interactions.AssertExists(".ui-datebox-container");
			Browser.Interactions.AssertNotExists(".ui-datebox-container", "ui-datebox-screen ui-datebox-hidden");
		}

		[When(@"I open the skill-picker")]
		public void WhenIOpenTheSkillPicker()
		{
			Browser.Interactions.Click("body:not(.ui-mobile-viewport-transitioning) #report-settings-view.ui-page-active #sel-skill-button");
		}

		[When(@"I close the skill-picker")]
		public void WhenICloseTheSkillPicker()
		{
			Browser.Interactions.ClickContaining(".ui-popup-container a", "Close");
		}

		[When(@"I select a report")]
		public void WhenISelectAReport()
		{
			SelectGetAnsweredAndAbondonedReport();
		}

		[When(@"I select date today")]
		public void WhenISelectDateToday()
		{
			var date = DateOnlyForBehaviorTests.TestToday;
			var dateString = date.ToShortDateString(DataMaker.Data().MyCulture);
			// this cant be correct. trigger an avent named datebox?!
			new JQueryExpression()
				.SelectById("sel-date")
				.Trigger("datebox",
						 string.Format(
							"{{'method':'set', 'value': '{0}', 'date': new Date({1}, {2} , {3})}}",
							dateString, date.Year, date.Month - 1, date.Day))
				.Eval();

			Browser.Interactions.AssertInputValueUsingJQuery("#sel-date", dateString);
		}

		[When(@"I select a skill")]
		public void WhenISelectASkill()
		{
			var skillId = DataMaker.Data().UserData<ThreeSkills>().Skill2Id;
			var script = "var el = $('#sel-skill');" +
						 "el.val('" + skillId + "').attr('selected', true).siblings('option').removeAttr('selected');" +
						 "el.selectmenu('refresh', true);";
			Browser.Interactions.Javascript(script, skillId);
		}

		[When(@"I select the all skills item")]
		public void WhenISelectTheAllSkillsItem()
		{
			// Default operation is select all.
		}

		[When(@"I view ReportSettings")]
		public void WhenIViewReportSettings()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileReportsSettings();
		}

		[Then(@"I should see sunday as the first day of week in tabledata")]
		public void ThenIShouldSeeSundayAsTheFirstDayOfWeekInTabledata()
		{
			Browser.Interactions.AssertFirstContains(".report-table>tbody>tr>td", Resources.DayOfWeekSunday);
		}

		[Then(@"I should see the all skill item selected")]
		public void ThenIShouldSeeTheAllSkillItemSelected()
		{
			Browser.Interactions.AssertExistsUsingJQuery("body:not(.ui-mobile-viewport-transitioning) #report-settings-view.ui-page-active #sel-skill-button:contains('{0}')", Resources.All);
			Browser.Interactions.AssertExistsUsingJQuery("body:not(.ui-mobile-viewport-transitioning) #report-settings-view.ui-page-active #sel-skill-menu li[aria-selected='true']:contains('{0}')", Resources.All);
		}
	}
}