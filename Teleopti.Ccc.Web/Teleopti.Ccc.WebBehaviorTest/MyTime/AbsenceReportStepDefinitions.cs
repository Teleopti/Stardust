using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	class AbsenceReportStepDefinitions
	{
		[When(@"I click to add a new absence report")]
		public void WhenIClickToAddANewAbsenceReport()
		{
			Browser.Interactions.Click("#addAbsenceReport");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[Then(@"I should see the add absence report form")]
		public void ThenIShouldSeeTheAddAbsenceReportForm()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-add-section .report-edit-absence");
		}

		[Then(@"I should not see the add absence report button")]
		public void ThenIShouldNotSeeTheAddAbsenceReportButton()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#addAbsenceReport");
		}

		[When(@"I cancel the current absence report draft")]
		public void WhenICancelTheCurrentAbsenceReportDraft()
		{
			Browser.Interactions.Click(".absence-report-cancel");
		}

		[Then(@"I should not see the add absence report form")]
		public void ThenIShouldNotSeeTheAddAbsenceReportForm()
		{
			Browser.Interactions.AssertNotExists(".body-weekview-inner", "#Request-add-section .report-edit-absence");
		}

		[When(@"I save the current absence report")]
		public void WhenISaveTheCurrentAbsenceReport()
		{
			Browser.Interactions.Click(".absence-report-send");
		}
	}
}