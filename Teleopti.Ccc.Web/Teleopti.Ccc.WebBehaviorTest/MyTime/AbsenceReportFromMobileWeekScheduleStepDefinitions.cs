using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Scope(Feature = "Absence Report On Mobile Week Schedule")]
	[Binding]
	class AbsenceReportFromMobileWeekScheduleStepDefinitions
	{
		[Then(@"I should not see any add absence report button")]
		public void ThenIShouldNotSeeTheAddAbsenceReportButton()
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(".day", ".absence-report-add");
		}

		[Then(@"I should see the add absence report form")]
		public void ThenIShouldSeeTheAddAbsenceReportForm()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-add-section .report-edit-absence");
		}

		[Then(@"I should not see the add absence report form")]
		public void ThenIShouldNotSeeTheAddAbsenceReportForm()
		{
			Browser.Interactions.AssertNotExists(".day", "#Request-add-section .report-edit-absence");
		}

		[Then(@"I should see add absence report button for '(.*)'")]
		public void ThenIShouldSeeAddAbsenceReportButtonFor(string date)
		{
			Browser.Interactions.AssertExists(getAddAbsenceReportButtonSelector(date));
		}

		[Then(@"I should not see add absence report button for '(.*)'")]
		public void ThenIShouldNotSeeAddAbsenceReportButtonFor(string date)
		{
			Browser.Interactions.AssertNotExists(".day", getAddAbsenceReportButtonSelector(date));
		}

		[When(@"I click on add absence report button for '(.*)'")]
		public void WhenIClickOnAddAbsenceReportButtonFor(string date)
		{
			Browser.Interactions.Click(getAddAbsenceReportButtonSelector(date));
		}

		[When(@"I cancel the current absence report draft")]
		public void WhenICancelTheCurrentAbsenceReportDraft()
		{
			Browser.Interactions.ClickUsingJQuery(".absence-report-cancel:visible");
		}

		[When(@"I save the current absence report draft")]
		public void WhenISaveTheCurrentAbsenceReportDraft()
		{
			Browser.Interactions.ClickUsingJQuery(".absence-report-send:visible");
		}

		[Then(@"I should see the add absence report form for '(.*)'")]
		public void ThenIShouldSeeTheAddAbsenceReportFormFor(string date)
		{
			Browser.Interactions.AssertExists(getAddAbsenceReportFormSelector(date));
		}

		[Then(@"I should not see the add absence report form for '(.*)'")]
		public void ThenIShouldNotSeeTheAddAbsenceReportFormFor(string date)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(".weekview-mobile", getAddAbsenceReportFormSelector(date));
		}

		private string getAddAbsenceReportButtonSelector(string date)
		{
			return string.Format(".day[data-mytime-date='{0}'] .absence-report-add", date);
		}

		private string getAddAbsenceReportFormSelector(string date)
		{
			return string.Format(".day[data-mytime-date='{0}'] #Request-add-section", date);
		}
	}
}