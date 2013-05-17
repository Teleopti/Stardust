using System;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest
{

	[Binding]
	public class MobileReportsStepDefinitions
	{
		private MobileReportsPage _page;

		[Given(@"I have skill analytics data")]
		public void GivenIHaveSkillAnalyticsData()
		{
			var timeZones = UserFactory.User().UserData<ITimeZoneData>();
			var dataSource = UserFactory.User().UserData<IDatasourceData>();
			var businessUnits = new BusinessUnit(TestData.BusinessUnit, dataSource);
			UserFactory.User().Setup(businessUnits);
			UserFactory.User().Setup(new ThreeSkills(timeZones, businessUnits, dataSource));
		}

		[Given(@"I have analytics fact queue data")]
		public void GivenIHaveFactQueueDataForAWeek()
		{
			var timeZones = UserFactory.User().UserData<ITimeZoneData>();
			var dataSource = UserFactory.User().UserData<IDatasourceData>();
			var queues = new AQueue(dataSource);
			var businessUnits = new BusinessUnit(TestData.BusinessUnit, dataSource);
			UserFactory.User().Setup(queues);
			UserFactory.User().Setup(businessUnits);
			var skills = new ThreeSkills(timeZones, businessUnits, dataSource);
			UserFactory.User().Setup(skills);
			var workloads = new AWorkload(skills, timeZones, businessUnits, dataSource);
			UserFactory.User().Setup(workloads);
			UserFactory.User().Setup(new FillBridgeQueueWorkloadFromData(workloads, queues, businessUnits, dataSource));
			var intervals = UserFactory.User().UserData<IIntervalData>();
			var bridgeTimeZones = UserFactory.User().UserData<IBridgeTimeZone>();
			var dates = UserFactory.User().UserData<IDateData>();
			UserFactory.User().Setup(new FactQueue(dates, intervals, queues, dataSource, bridgeTimeZones));
			UserFactory.User().Setup(new WeekdayTranslations());
		}

		[When(@"I click the signout button")]
		public void GivenIClickSignoutButton()
		{
			Browser.Interactions.Click("#signout-button");
		}

		[Then(@"I should see a report")]
		public void ThenIShouldSeAReport()
		{
			Browser.Interactions.AssertExists("#report-view.ui-page-active");
			Browser.Interactions.AssertNotExists("#report-view.ui-page-active", ".ui-mobile-viewport-transitioning");
		}

		[Then(@"I should see a graph")]
		public void ThenIShouldSeeAGraph()
		{
			Browser.Interactions.AssertExists("#report-view.ui-page-active #report-graph-holder");
			Browser.Interactions.AssertExists("#report-view.ui-page-active #report-graph-canvas");
		}

		[Then(@"I should see a report for next date")]
		public void ThenIShouldSeeAReportForNextDate()
		{
			var expected = DateOnly.Today.AddDays(1).ToShortDateString(UserFactory.User().Culture);
			Browser.Interactions.AssertContains("#report-view-date-nav-current", expected);
		}

		[Then(@"I should see a report for previous date")]
		public void ThenIShouldSeeAReportForPreviousDate()
		{
			var expected = DateOnly.Today.AddDays(-1).ToShortDateString(UserFactory.User().Culture);
			Browser.Interactions.AssertContains("#report-view-date-nav-current", expected);
		}

		[Then(@"I should see a table")]
		public void ThenIShouldSeeATable()
		{
			Browser.Interactions.AssertExists("#report-table-holder tr");
		}

		[Then(@"I should see friendly error message")]
		public void ThenIShouldSeeFriendlyErrorMessage()
		{
			Browser.Interactions.AssertContains("#error-view", "No access");
		}

		[Then(@"I should see ReportSettings")]
		public void ThenIShouldSeeReportSettings()
		{
			Browser.Interactions.AssertExists("#report-settings-view.ui-page-active");
			Browser.Interactions.AssertNotExists("#report-settings-view.ui-page-active", ".ui-mobile-viewport-transitioning");
		}

		[Then(@"I should see ReportSettings with default value")]
		public void ThenIShouldSeeReportSettingsWithDefaultValue()
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#sel-date", DateOnly.Today.AddDays(-1).ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertContains("#sel-skill-menu", Resources.All);
			Browser.Interactions.AssertExists("#report-settings-type-graph:checked");
			Browser.Interactions.AssertExists("#report-settings-type-table:not(:checked)");
			Browser.Interactions.AssertExists("#report-settings-interval-week:not(:checked)");
		}


		[Then(@"I should see the selected date")]
		public void ThenIShouldSeeTheSelectedDate()
		{
			EventualAssert.That(() => _page.ReportSelectionDateValue, Is.Not.Empty);
		}

		[Then(@"I should see the selected skill")]
		public void ThenIShouldSeeTheSelectedSkill()
		{
			var skillName = UserFactory.User().UserData<ThreeSkills>().Skill2Name;
			EventualAssert.That(() => _page.ReportSkillSelectionOpener.Text, Is.StringContaining(skillName));
		}

		[Then(@"I should only see reports i have access to")]
		public void ThenIShouldonlySeeReportsIHaveAccessTo()
		{
			EventualAssert.That(() => _page.Reports.Count, Is.EqualTo(2));
		}

		[Then(@"the date-picker should close")]
		public void ThenTheDatePickerShouldClose()
		{
			EventualAssert.That(() => Browser.Current.Element(QuicklyFind.ByClass("ui-datebox-container")).Exists, Is.False);
		}

		[Given(@"I am viewing a report")]
		public void WhenIAmViewAReport()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileReportsSettings();
			_page = Browser.Current.Page<MobileReportsPage>();
			EventualAssert.That(() => _page.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True);

			WhenISelectDateToday();
			_page.ReportGetAnsweredAndAbandoned.EventualClick();
			WhenICheckTypeTable();
			WhenIClickViewReportButton();
		}

		[When(@"I view a report with week data")]
		public void WhenIViewAReportWithWeekData()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileReportsSettings();
			_page = Browser.Current.Page<MobileReportsPage>();
			EventualAssert.That(() => _page.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True);

			WhenISelectDateToday();
			_page.ReportGetAnsweredAndAbandoned.EventualClick();
			_page.ReportIntervalWeekInput.EventualClick();
			WhenICheckTypeTable();
			WhenIClickViewReportButton();
		}

		[When(@"I check type Graph")]
		public void WhenICheckTypeGraph()
		{
			if (!_page.ReportTypeGraphInput.Checked)
				_page.ReportTypeGraphInput.EventualClick();
			EventualAssert.That(() => _page.ReportTypeGraphInput.Checked, Is.True);
		}

		[When(@"I check type Table")]
		public void WhenICheckTypeTable()
		{
			if (!_page.ReportTypeTableInput.Checked)
				_page.ReportTypeTableInput.EventualClick();
			EventualAssert.That(() => _page.ReportTypeTableInput.Checked, Is.True);
		}

		[When(@"I click next date")]
		public void WhenIClickNextDate()
		{
			// before click next date, make sure current is today
			var expexted = DateOnly.Today.ToShortDateString(UserFactory.User().Culture);
			EventualAssert.That(() => _page.ReportViewNavDate.Text, Is.EqualTo(expexted));
			_page.ReportViewNextDateNavigation.EventualClick();
		}

		[When(@"I click on any date")]
		public void WhenIClickOnAnyDate()
		{
			var pickerDayDiv = _page.AnyDatePickerDay;
			pickerDayDiv.EventualClick();
		}

		[When(@"I click previous date")]
		public void WhenIClickPreviousDate()
		{
			// before click previous date, make sure current is today
			var expexted = DateOnly.Today.ToShortDateString(UserFactory.User().Culture);
			EventualAssert.That(() => _page.ReportViewNavDate.Text, Is.EqualTo(expexted));
			_page.ReportViewPrevDateNavigation.EventualClick();
		}

		[When(@"I click View Report Button")]
		public void WhenIClickViewReportButton()
		{
			_page.ReportViewShowButton.EventualClick();
			EventualAssert.That(() => _page.ReportsViewPageContainer.DisplayVisible(), Is.True);
			EventualAssert.That(() => _page.ReportsViewPageContainer.JQueryVisible(), Is.True);
			EventualAssert.That(() => _page.ReportsViewPageContainer.ClassName, Is.StringContaining("ui-page-active"));
		}

		[When(@"I close the skill-picker")]
		public void WhenICloseTheSkillPicker()
		{
			_page.ReportSkillSelectionCloseButton.EventualClick();
		}

		[Given(@"I view MobileReports")]
		[When(@"I view MobileReports")]
		public void WhenIEnterMobileReports()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileReportsPage();
			_page = Browser.Current.Page<MobileReportsPage>();
		}

		[When(@"I open the date-picker")]
		public void WhenIOpenTheDatePicker()
		{
			_page.ReportSelectionDateOpener.EventualClick();
			EventualAssert.That(() => _page.ReportSelectionDatePickerContainer.DisplayVisible(), Is.True);
		}

		[When(@"I open the skill-picker")]
		public void WhenIOpenTheSkillPicker()
		{
			_page.ReportSkillSelectionOpener.EventualClick();
		}

		[When(@"I select a report")]
		public void WhenISelectAReport()
		{
			_page.ReportGetAnsweredAndAbandoned.EventualClick();
			Assert.That(() => _page.ReportGetAnsweredAndAbandonedInput.Checked, Is.True);
		}

		[When(@"I select date today")]
		public void WhenISelectDateToday()
		{
			var date = DateOnly.Today.Date.ToShortDateString(UserFactory.User().Culture);
			_page.SetReportSettingsDate(DateOnly.Today, UserFactory.User().Culture);
			EventualAssert.That(() => _page.ReportSelectionDateValue, Is.EqualTo(date));
		}

		[When(@"I select a skill")]
		public void WhenISelectASkill()
		{
			foreach (var skillListItem in _page.ReportSkillSelectionList.ListItems)
			{
				if (skillListItem.GetAttributeValue("aria-selected").Equals("true"))
				{
					skillListItem.EventualClick();
				}
			}

			_page.SelectSkillByName(UserFactory.User().UserData<ThreeSkills>().Skill2Name);
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
			_page = Browser.Current.Page<MobileReportsPage>();
		}

		[Then(@"I should see sunday as the first day of week in tabledata")]
		public void ThenIShouldSeeSundayAsTheFirstDayOfWeekInTabledata()
		{
			EventualAssert.That(() => _page.ReportTableFirstDataCell.Text.Trim(), Is.EqualTo(Resources.DayOfWeekSunday));
		}

		[Then(@"I should see the all skill item selected")]
		public void ThenIShouldSeeTheAllSkillItemSelected()
		{
			EventualAssert.That(() => _page.ReportSkillSelectionOpener.Text.Trim(), Is.StringContaining(Resources.All));
		}
	}
}