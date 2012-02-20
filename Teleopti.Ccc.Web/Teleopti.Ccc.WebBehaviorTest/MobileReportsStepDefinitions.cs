namespace Teleopti.Ccc.WebBehaviorTest
{
	using NUnit.Framework;

	using TechTalk.SpecFlow;

	using Teleopti.Ccc.WebBehaviorTest.Core;
	using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
	using Teleopti.Ccc.WebBehaviorTest.Data;
	using Teleopti.Ccc.WebBehaviorTest.Pages;
	using Teleopti.Interfaces.Domain;

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

		[Given(@"I click Signout button")]
		public void GivenIClickSignoutButton()
		{
			this._page.SignoutButton.Click();
		}

		[Then(@"I should be signed out")]
		public void ThenIShouldBeSignedOut()
		{
			Assert.That(() => Browser.Current.Url.EndsWith("/SignIn"), Is.True.After(5000, 50));
		}

		[Then(@"I should se a report")]
		public void ThenIShouldSeAReport()
		{
			Assert.That(() => this._page.ReportsViewPageContainer.DisplayVisible(), Is.True.After(5000, 100));
		}

		[Then(@"I should see a graph")]
		public void ThenIShouldSeeAGraph()
		{
			Assert.That(() => this._page.ReportGraphContainer.DisplayVisible(), Is.True.After(5000, 100));
			Assert.That(() => this._page.ReportGraph.DisplayVisible(), Is.True.After(5000, 100));
		}

		[Then(@"I should see a report for next date")]
		public void ThenIShouldSeeAReportForNextDate()
		{
			var date = StartDateForNextPrevNavigation;
			var expexted = date.AddDays(1).ToShortDateString(UserFactory.User().Culture);
			Assert.That(
				() => expexted.Equals(this._page.ReportCurrentDate),
				Is.True.After(1000, 100),
				string.Format("ReportCurrentDate should be \"{0}\" but was \"{1}\"!", expexted, this._page.ReportCurrentDate));
		}

		[Then(@"I should see a report for previous date")]
		public void ThenIShouldSeeAReportForPreviousDate()
		{
			var date = StartDateForNextPrevNavigation;
			var expexted = date.AddDays(-1).ToShortDateString(UserFactory.User().Culture);
			Assert.That(
				() => expexted.Equals(this._page.ReportCurrentDate),
				Is.True.After(1000, 100),
				string.Format("ReportCurrentDate should be \"{0}\" but was \"{1}\"!", expexted, this._page.ReportCurrentDate));
		}

		[Then(@"I should see a table")]
		public void ThenIShouldSeeATable()
		{
			var table = this._page.ReportTableContainer.ElementOfType<Table>(table1 => true);
			var count = table.TableCells.Count;

			/*  3 + (96 * 3) */
			Assert.That(
				() => count.Equals(291), Is.True.After(1000, 100), string.Format("Table should have 291 rows not: {0}!", count));
		}

		[Then(@"I should see friendly error message")]
		public void ThenIShouldSeeFriendlyErrorMessage()
		{
			this._page.Document.ContainsText("No access");
		}

		[Then(@"I should see the Home")]
		public void ThenIShouldSeeTheHome()
		{
			EventualAssert.That(() => this._page.HomeViewContainer.Exists, Is.True);
			EventualAssert.That(() => this._page.HomeViewContainer.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the selected date")]
		public void ThenIShouldSeeTheSelectedDate()
		{
			EventualAssert.That(() => this._page.ReportSelectionDateValue, Is.Not.Empty);
		}

		[Then(@"I should see the selected skill")]
		public void ThenIShouldSeeTheSelectedSkill()
		{
			string text = this._page.ReportSkillSelectionOpener.Text;
			EventualAssert.That(() => text.Contains(this._page.ThirdSkillName), Is.True);
		}

		[Then(@"I should only see reports i have access to")]
		public void ThenIShouldonlySeeReportsIHaveAccessTo()
		{
			var count = this._page.Reports.Count;
			EventualAssert.That(
				() => 2.Equals(count),
				Is.True.After(1000, 100),
				string.Format("Restricted access user should only see two reports by saw: {0}", count));
		}

		[Then(@"the date-picker should close")]
		public void ThenTheDatePickerShouldClose()
		{
			EventualAssert.That(() => this._page.ReportSelectionDatePickerContainer.DisplayHidden(), Is.True);
		}

		[When(@"I am view a Report")]
		public void WhenIAmViewAReport()
		{
			createAndSignIn();
			Navigation.GotoMobileReportsSettings();
			this._page = Browser.Current.Page<MobileReportsPage>();
			Assert.That(() => this._page.ReportsSettingsViewPageContainer.DisplayVisible(), Is.True.After(10000, 100));

			this._page.SetReportSettingsDate(StartDateForNextPrevNavigation);
			EventualAssert.That(() => this._page.ReportSelectionDateValue, Is.Not.Empty, "Date should be set");
			this._page.ReportGetAnsweredAndAbandoned.Click();
			if (!this._page.ReportTypeTableInput.Checked)
			{
				this._page.ReportTypeTableInput.Click();
			}
			this._page.ReportViewShowButton.Click();

			Assert.That(() => this._page.ReportsViewPageContainer.DisplayVisible(), Is.True.After(10000, 100));
		}

		[When(@"I check type Graph")]
		public void WhenICheckTypeGraph()
		{
			if (!this._page.ReportTypeGraphInput.Checked)
			{
				this._page.ReportTypeGraphInput.Click();
			}
			Assert.That(() => this._page.ReportTypeGraphInput.Checked, Is.True);
		}

		[When(@"I check type Table")]
		public void WhenICheckTypeTable()
		{
			if (!this._page.ReportTypeTableInput.Checked)
			{
				this._page.ReportTypeTableInput.Click();
			}
			Assert.That(() => this._page.ReportTypeTableInput.Checked, Is.True);
		}

		[When(@"I click next date")]
		public void WhenIClickNextDate()
		{
			this._page.ReportViewNextDateNavigation.Click();
		}

		[When(@"I click on any date")]
		public void WhenIClickOnAnyDate()
		{
			var pickerDayDiv = this._page.AnyDatePickerDay;
			pickerDayDiv.Click();
		}

		[When(@"I click previous date")]
		public void WhenIClickPreviousDate()
		{
			this._page.ReportViewPrevDateNavigation.Click();
		}

		[When(@"I click View Report Button")]
		public void WhenIClickViewReportButton()
		{
			this._page.ReportViewShowButton.Click();
		}

		[When(@"I close the skill-picker")]
		public void WhenICloseTheSkillPicker()
		{
			this._page.ReportSkillSelectionCloseButton.Click();
			EventualAssert.That(() => this._page.ReportSkillSelectionContainer.PositionedOnScreen(), Is.False);
		}

		[Given(@"I view MobileReports")]
		[When(@"I view MobileReports")]
		public void WhenIEnterMobileReports()
		{
			createAndSignIn();
			Navigation.GotoMobileReports();
			this._page = Browser.Current.Page<MobileReportsPage>();
		}

		[When(@"I open the date-picker")]
		public void WhenIOpenTheDatePicker()
		{
			this._page.ReportSelectionDateOpener.Click();
			EventualAssert.That(() => this._page.ReportSelectionDatePickerContainer.DisplayVisible(), Is.True);
		}

		[When(@"I open the skill-picker")]
		public void WhenIOpenTheSkillPicker()
		{
			this._page.ReportSkillSelectionOpener.Click();
			EventualAssert.That(() => this._page.ReportSkillSelectionContainer.PositionedOnScreen(), Is.True);
			EventualAssert.That(() => this._page.ReportSkillSelectionContainer.DisplayVisible(), Is.True);
		}

		[When(@"I select a report")]
		public void WhenISelectAReport()
		{
			this._page.ReportGetAnsweredAndAbandoned.Click();
			Assert.That(() => this._page.ReportGetAnsweredAndAbandonedInput.Checked, Is.True);
		}

		[When(@"I select a skill")]
		public void WhenISelectASkill()
		{
			foreach (var skillListItem in this._page.ReportSkillSelectionList.ListItems)
			{
				if (skillListItem.GetAttributeValue("aria-selected").Equals("true"))
				{
					skillListItem.Click();
				}
			}

			var third = this._page.ThirdSkillInSkillList;
			third.Click();

			this._page.ThirdSkillName = third.Text.Trim();
		}

		[When(@"I view ReportSettings")]
		public void WhenIViewReportSettings()
		{
			createAndSignIn();
			Navigation.GotoMobileReportsSettings();
			this._page = Browser.Current.Page<MobileReportsPage>();
		}

		private static void createAndSignIn()
		{
			var userName = UserFactory.User().MakeUser();
			Navigation.GotoMobileReportsSignInPage();
			var page = Browser.Current.Page<MobileSignInPage>();
			page.SignInApplication(userName, TestData.CommonPassword);
		}
	}
}