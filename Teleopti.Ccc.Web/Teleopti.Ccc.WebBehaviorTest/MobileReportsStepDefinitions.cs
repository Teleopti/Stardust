using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using Table = WatiN.Core.Table;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class MobileReportsStepDefinitions
	{
		private static readonly DateOnly StartDateForNextPrevNavigation = new DateOnly(2001, 1, 1);
		private MobileReportsPage _page;

		[Given(@"I view MobileReports")]
		[When(@"I view MobileReports")]
		public void WhenIEnterMobileReports()
		{
			createAndSignIn();
			Navigation.GotoMobileReports();
			_page = Browser.Current.Page<MobileReportsPage>();
		}

		[When(@"I view ReportSettings")]
		public void WhenIViewReportSettings()
		{
			createAndSignIn();
			Navigation.GotoMobileReportsSettings();
			_page = Browser.Current.Page<MobileReportsPage>();
		}

		private static void createAndSignIn()
		{
			var userName = UserFactory.User().MakeUser();
			Navigation.GotoMobileReportsSignInPage();
			var page = Browser.Current.Page<MobileSignInPage>();
			page.SignInApplication(userName, TestData.CommonPassword);
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

		[When(@"I select a skill")]
		public void WhenISelectASkill()
		{
			foreach (var skillListItem in _page.ReportSkillSelectionList.ListItems)
			{
				if (skillListItem.GetAttributeValue("aria-selected").Equals("true"))
					skillListItem.Click();
			}

			var third = _page.ThirdSkillInSkillList;
			third.Click();

			_page.ThirdSkillName = third.Text.Trim();
		}

		[When(@"I close the skill-picker")]
		public void WhenICloseTheSkillPicker()
		{
			_page.ReportSkillSelectionCloseButton.Click();
			EventualAssert.That(() => _page.ReportSkillSelectionContainer.PositionedOnScreen(), Is.False);
		}

		[Then(@"I should see the selected skill")]
		public void ThenIShouldSeeTheSelectedSkill()
		{
			string text = _page.ReportSkillSelectionOpener.Text;
			EventualAssert.That(() => text.Contains(_page.ThirdSkillName), Is.True);
		}

		[When(@"I click on any date")]
		public void WhenIClickOnAnyDate()
		{
			var pickerDayDiv = _page.AnyDatePickerDay;
			pickerDayDiv.Click();
		}

		[Then(@"the date-picker should close")]
		public void ThenTheDatePickerShouldClose()
		{
			EventualAssert.That(() => _page.ReportSelectionDatePickerContainer.DisplayHidden(), Is.True);
		}

		[Then(@"I should see friendly error message")]
		public void ThenIShouldSeeFriendlyErrorMessage()
		{
			_page.Document.ContainsText("No access");
		}


		[Then(@"I should see the selected date")]
		public void ThenIShouldSeeTheSelectedDate()
		{
			EventualAssert.That(() => _page.ReportSelectionDateValue, Is.Not.Empty);
		}

		[Given(@"I browse with a mobile")]
		public void GivenIBrowseWithAMobile()
		{
			// Inject mobile UserAgent String /Replace CurrentBrowser?
		}

		[Then(@"I should see the Home")]
		public void ThenIShouldSeeTheHome()
		{
			EventualAssert.That(() => _page.HomeViewContainer.Exists, Is.True);
			EventualAssert.That(() => _page.HomeViewContainer.DisplayVisible(), Is.True);
		}

		[Given(@"I click Signout button")]
		public void GivenIClickSignoutButton()
		{
			_page.SignoutButton.Click();
		}

		[Then(@"I should be signed out")]
		public void ThenIShouldBeSignedOut()
		{
			Assert.That(() => Browser.Current.Url.EndsWith("/SignIn"), Is.True.After(5000, 50));
		}

		[When(@"I select a report")]
		public void WhenISelectAReport()
		{
			_page.ReportGetAnsweredAndAbandoned.Click();
			Assert.That(() => _page.ReportGetAnsweredAndAbandonedInput.Checked, Is.True);
		}

		[When(@"I check type Graph")]
		public void WhenICheckTypeGraph()
		{
			if (!_page.ReportTypeGraphInput.Checked)
				_page.ReportTypeGraphInput.Click();
			Assert.That(() => _page.ReportTypeGraphInput.Checked, Is.True);
		}

		[When(@"I check type Table")]
		public void WhenICheckTypeTable()
		{
			if (!_page.ReportTypeTableInput.Checked)
				_page.ReportTypeTableInput.Click();
			Assert.That(() => _page.ReportTypeTableInput.Checked, Is.True);
		}

		[When(@"I click View Report Button")]
		public void WhenIClickViewReportButton()
		{
			_page.ReportViewShowButton.Click();
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

		[Then(@"I should see a table")]
		public void ThenIShouldSeeATable()
		{
			var table = _page.ReportTableContainer.ElementOfType<Table>(table1 => true);
			var count = table.TableCells.Count;

			/*  3 + (96 * 3) */
			Assert.That(() => count.Equals(291), Is.True.After(1000, 100),
			            string.Format("Table should have 291 rows not: {0}!", count));
		}

		[When(@"I am view a Report")]
		public void WhenIAmViewAReport()
		{
			createAndSignIn();
			Navigation.GotoMobileReportsSettings();
			_page = Browser.Current.Page<MobileReportsPage>();
			Assert.That(() => _page.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True.After(10000, 100));

			_page.SetReportSettingsDate(StartDateForNextPrevNavigation);
			EventualAssert.That(() => _page.ReportSelectionDateValue, Is.Not.Empty, "Date should be set");
			_page.ReportGetAnsweredAndAbandoned.Click();
			if (!_page.ReportTypeTableInput.Checked)
				_page.ReportTypeTableInput.Click();
			_page.ReportViewShowButton.Click();

			Assert.That(() => _page.ReportsViewPageContainer.DisplayVisible(), Is.True.After(10000, 100));
		}

		[When(@"I click next date")]
		public void WhenIClickNextDate()
		{
			_page.ReportViewNextDateNavigation.Click();
		}

		[When(@"I click previous date")]
		public void WhenIClickPreviousDate()
		{
			_page.ReportViewPrevDateNavigation.Click();
		}

		[Then(@"I should see a report for next date")]
		public void ThenIShouldSeeAReportForNextDate()
		{
			var date = StartDateForNextPrevNavigation;
			var expexted = date.AddDays(1).ToShortDateString(UserFactory.User().Culture);
			Assert.That(() => expexted.Equals(_page.ReportCurrentDate), Is.True.After(1000, 100),
			            string.Format("ReportCurrentDate should be \"{0}\" but was \"{1}\"!", expexted, _page.ReportCurrentDate));
		}

		[Then(@"I should see a report for previous date")]
		public void ThenIShouldSeeAReportForPreviousDate()
		{
			var date = StartDateForNextPrevNavigation;
			var expexted = date.AddDays(-1).ToShortDateString(UserFactory.User().Culture);
			Assert.That(() => expexted.Equals(_page.ReportCurrentDate), Is.True.After(1000, 100),
			            string.Format("ReportCurrentDate should be \"{0}\" but was \"{1}\"!", expexted, _page.ReportCurrentDate));
		}
	}
}