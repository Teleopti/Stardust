using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics;

namespace Teleopti.Ccc.WebBehaviorTest
{
	using NUnit.Framework;

	using TechTalk.SpecFlow;

	using Teleopti.Ccc.WebBehaviorTest.Core;
	using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
	using Teleopti.Ccc.WebBehaviorTest.Data;
	using Teleopti.Ccc.WebBehaviorTest.Pages;
	using Teleopti.Interfaces.Domain;

	using WatiN.Core;

	using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
	using Table = WatiN.Core.Table;

	[Binding]
	public class MobileReportsStepDefinitions
	{
		private static readonly DateOnly StartDateForNextPrevNavigation = new DateOnly(2001, 1, 1);

		private MobileReportsPage _page;

		[Given(@"I browse with a mobile")]
		public void GivenIBrowseWithAMobile()
		{
			// Inject mobile UserAgent String /Replace CurrentBrowser?
		}

		[Given(@"I have skill analytics data")]
		public void GivenIHaveSkillAnalyticsData()
		{
			var timeZones = UserFactory.User().UserData<ITimeZoneData>();
			var dataSource = UserFactory.User().UserData<IDatasourceData>();
			var businessUnits = new BusinessUnit(TestData.BusinessUnit, dataSource);
			UserFactory.User().Setup(businessUnits);
			UserFactory.User().Setup(new ThreeSkills(timeZones, businessUnits, dataSource));
		}

		[Given(@"I have fact queue data")]
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
			UserFactory.User().Setup(new FactQueue(dates, intervals, queues, dataSource, timeZones, bridgeTimeZones));
		}

		[When(@"I click the signout button")]
		public void GivenIClickSignoutButton()
		{
			_page.SignoutButton.Click();
		}

		[Then(@"I should be signed out")]
		public void ThenIShouldBeSignedOut()
		{
			EventualAssert.That(() => Browser.Current.Url.EndsWith("/SignIn"), Is.True);
		}

		[Then(@"I should se a report")]
		public void ThenIShouldSeAReport()
		{
			Assert.That(() => _page.ReportsViewPageContainer.DisplayVisible(), Is.True.After(5000, 100));
		}

		[Then(@"I should see a graph")]
		public void ThenIShouldSeeAGraph()
		{
			Assert.That(() => _page.ReportGraphContainer.DisplayVisible(), Is.True.After(5000, 100));
			Assert.That(() => _page.ReportGraph.DisplayVisible(), Is.True.After(5000, 100));
		}

		[Then(@"I should see a report for next date")]
		public void ThenIShouldSeeAReportForNextDate()
		{
			var date = StartDateForNextPrevNavigation;
			var expexted = date.AddDays(1).ToShortDateString(UserFactory.User().Culture);
			Assert.That(
				() => expexted.Equals(_page.ReportCurrentDate),
				Is.True.After(1000, 100),
				string.Format("ReportCurrentDate should be \"{0}\" but was \"{1}\"!", expexted, _page.ReportCurrentDate));
		}

		[Then(@"I should see a report for previous date")]
		public void ThenIShouldSeeAReportForPreviousDate()
		{
			var date = StartDateForNextPrevNavigation;
			var expexted = date.AddDays(-1).ToShortDateString(UserFactory.User().Culture);
			Assert.That(
				() => expexted.Equals(_page.ReportCurrentDate),
				Is.True.After(1000, 100),
				string.Format("ReportCurrentDate should be \"{0}\" but was \"{1}\"!", expexted, _page.ReportCurrentDate));
		}

		[Then(@"I should see a table")]
		public void ThenIShouldSeeATable()
		{
			var table = _page.ReportTableContainer.ElementOfType<Table>(table1 => true);
			var count = table.TableCells.Count;

			/*  3 + (96 * 3) */
			Assert.That(
				() => count.Equals(291), Is.True.After(1000, 100), string.Format("Table should have 291 rows not: {0}!", count));
		}

		[Then(@"I should see friendly error message")]
		public void ThenIShouldSeeFriendlyErrorMessage()
		{
			_page.Document.Divs.Filter(Find.ById("error-view")).First().DomContainer.ContainsText("No access");
		}

		[Then(@"I should see ReportSettings")]
		public void ThenIShouldSeeReportSettings()
		{
			EventualAssert.That(() => _page.ReportsSettingsViewPageContainer.Exists, Is.True);
			EventualAssert.That(() => _page.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the selected date")]
		public void ThenIShouldSeeTheSelectedDate()
		{
			EventualAssert.That(() => _page.ReportSelectionDateValue, Is.Not.Empty);
		}

		[Then(@"I should see the selected skill")]
		public void ThenIShouldSeeTheSelectedSkill()
		{
			string text = _page.ReportSkillSelectionOpener.Text;
			EventualAssert.That(() => text.Contains(_page.ThirdSkillName), Is.True);
		}

		[Then(@"I should only see reports i have access to")]
		public void ThenIShouldonlySeeReportsIHaveAccessTo()
		{
			var count = _page.Reports.Count;
			EventualAssert.That(
				() => 2 == count,
				Is.True.After(1000, 100),
				string.Format("Restricted access user should only see two reports by saw: {0}", count));
		}

		[Then(@"the date-picker should close")]
		public void ThenTheDatePickerShouldClose()
		{
			EventualAssert.That(() => _page.ReportSelectionDatePickerContainer.DisplayHidden(), Is.True);
		}

		[Given(@"I am viewing a report")]
		public void WhenIAmViewAReport()
		{
			createAndSignIn();
			Navigation.GotoMobileReportsSettings();
			_page = Browser.Current.Page<MobileReportsPage>();
			EventualAssert.That(() => _page.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True);

			_page.SetReportSettingsDate(StartDateForNextPrevNavigation);
			EventualAssert.That(() => _page.ReportSelectionDateValue, Is.Not.Empty);
			_page.ReportGetAnsweredAndAbandoned.Click();

			if (!_page.ReportTypeTableInput.Checked)
			{
				_page.ReportTypeTableInput.Click();
			}

			_page.ReportViewShowButton.Click();

			EventualAssert.That(() => _page.ReportsViewPageContainer.DisplayVisible(), Is.True);
		}

		[When(@"I view a report with week data")]
		public void WhenIViewAReportWithWeekData()
		{
			createAndSignIn();
			Navigation.GotoMobileReportsSettings();
			_page = Browser.Current.Page<MobileReportsPage>();
			EventualAssert.That(() => _page.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True);

			_page.SetReportSettingsDate(StartDateForNextPrevNavigation);
			EventualAssert.That(() => _page.ReportSelectionDateValue, Is.Not.Empty);
			_page.ReportGetAnsweredAndAbandoned.Click();

			_page.ReportIntervalWeekInput.Click();
			if (!_page.ReportTypeTableInput.Checked)
			{
				_page.ReportTypeTableInput.Click();
			}

			_page.ReportViewShowButton.Click();

			EventualAssert.That(() => _page.ReportsViewPageContainer.DisplayVisible(), Is.True);
		}

		[When(@"I check type Graph")]
		public void WhenICheckTypeGraph()
		{
			if (!_page.ReportTypeGraphInput.Checked)
			{
				_page.ReportTypeGraphInput.Click();
			}
			Assert.That(() => _page.ReportTypeGraphInput.Checked, Is.True);
		}

		[When(@"I check type Table")]
		public void WhenICheckTypeTable()
		{
			if (!_page.ReportTypeTableInput.Checked)
			{
				_page.ReportTypeTableInput.Click();
			}
			Assert.That(() => _page.ReportTypeTableInput.Checked, Is.True);
		}

		[When(@"I click next date")]
		public void WhenIClickNextDate()
		{
			_page.ReportViewNextDateNavigation.Click();
		}

		[When(@"I click on any date")]
		public void WhenIClickOnAnyDate()
		{
			var pickerDayDiv = _page.AnyDatePickerDay;
			pickerDayDiv.Click();
		}

		[When(@"I click previous date")]
		public void WhenIClickPreviousDate()
		{
			_page.ReportViewPrevDateNavigation.Click();
		}

		[When(@"I click View Report Button")]
		public void WhenIClickViewReportButton()
		{
			_page.ReportViewShowButton.Click();
		}

		[When(@"I close the skill-picker")]
		public void WhenICloseTheSkillPicker()
		{
			_page.ReportSkillSelectionCloseButton.Click();
			EventualAssert.That(() => _page.ReportSkillSelectionContainer.PositionedOnScreen(), Is.False);
		}

		[Given(@"I view MobileReports")]
		[When(@"I view MobileReports")]
		public void WhenIEnterMobileReports()
		{
			createAndSignIn();
			Navigation.GotoMobileReports();
			_page = Browser.Current.Page<MobileReportsPage>();
		}

		[When(@"I open the date-picker")]
		public void WhenIOpenTheDatePicker()
		{
			_page.ReportSelectionDateOpener.Click();
			EventualAssert.That(() => _page.ReportSelectionDatePickerContainer.DisplayVisible(), Is.True);
		}

		[When(@"I open the skill-picker")]
		public void WhenIOpenTheSkillPicker()
		{
			_page.ReportSkillSelectionOpener.Click();
			EventualAssert.That(() => _page.ReportSkillSelectionContainer.PositionedOnScreen(), Is.True);
			EventualAssert.That(() => _page.ReportSkillSelectionContainer.DisplayVisible(), Is.True);
		}

		[When(@"I select a report")]
		public void WhenISelectAReport()
		{
			_page.ReportGetAnsweredAndAbandoned.Click();
			Assert.That(() => _page.ReportGetAnsweredAndAbandonedInput.Checked, Is.True);
		}

		[When(@"I select a skill")]
		public void WhenISelectASkill()
		{
			foreach (var skillListItem in _page.ReportSkillSelectionList.ListItems)
			{
				if (skillListItem.GetAttributeValue("aria-selected").Equals("true"))
				{
					skillListItem.Click();
				}
			}

			var third = _page.ThirdSkillInSkillList;
			third.Click();

			_page.ThirdSkillName = third.Text.Trim();
		}

		[When(@"I view ReportSettings")]
		public void WhenIViewReportSettings()
		{
			createAndSignIn();
			Navigation.GotoMobileReportsSettings();
			_page = Browser.Current.Page<MobileReportsPage>();
		}

		[Then(@"I should see sunday as the first day of week in tabledata")]
		public void ThenIShouldSeeSundayAsTheFirstDayOfWeekInTabledata()
		{
			EventualAssert.That(() => _page.ReportTableFirstDataCell.Text.Trim(), Is.StringContaining("Sunday"));
		}

		private static void createAndSignIn()
		{
			var userName = UserFactory.User().MakeUser();
			Navigation.GotoMobileReportsSignInPage(string.Empty);
			var page = Browser.Current.Page<MobileSignInPage>();
			page.SignInApplication(userName, TestData.CommonPassword);
		}

	}
}