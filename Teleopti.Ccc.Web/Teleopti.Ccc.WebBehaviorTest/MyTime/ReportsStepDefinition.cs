using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class ReportsSteps
	{
		[When(@"I click reports menu")]
		public void WhenIClickReportsMenu()
		{
			Browser.Interactions.Click("#reports");
		}

		[Then(@"I should see the dropdown report list")]
		public void ThenIShouldSeeTheDropdownReportList()
		{
			Browser.Interactions.AssertExists("#report-list");
			Browser.Interactions.AssertExistsUsingJQuery("#report-list li");
		}

		[Then(@"I should not see any report menu")]
		public void ThenIShouldNotSeeAnyReportMenu()
		{
			Browser.Interactions.AssertNotExists(".navbar-nav", "#reports");
			Browser.Interactions.AssertNotExists(".navbar-nav", "a[href$='#MyReportTab']");
		}

		[When(@"I click the report at position '(.*)' in the list")]
		public void WhenIClickTheReportAtPositionInTheList(int position)
		{
			Browser.Interactions.Click(string.Format("#report-list :nth-child({0})", position));
		}

		[Then(@"The report should not be opened in the same window")]
		public void ThenTheReportShouldNotBeOpenedInTheSameWindow()
		{
			Browser.Interactions.AssertUrlNotContains("MyTime", "Selection.aspx");
		    Browser.Interactions.CloseWindow("http://localhost");
		}

		[Then(@"The report should not be opened in the same tab")]
		public void ThenTheReportShouldNotBeOpenedInTheSameTab()
		{
			Browser.Interactions.AssertUrlNotContains("MyTime", "Reporting");
			Browser.Interactions.CloseWindow("Agent");
		}

		[When(@"I click one report in drop down list")]
		public void WhenIClickOneReportInDropDownList()
		{
			Browser.Interactions.Click("#report-list :nth-child(1)");
		}

		[Then(@"The report should be opened in a new window")]
		public void ThenTheReportShouldBeOpenedInANewWindow()
		{
			Browser.Interactions.AssertUrlNotContains("Anywhere", "Selection.aspx");
			Browser.Interactions.CloseWindow("http://localhost");
		}

		[Then(@"The report should be opened in another tab")]
		public void ThenTheReportShouldBeOpenedInAnotherTab()
		{
			Browser.Interactions.AssertUrlNotContains("Anywhere","Reporting" );
			Browser.Interactions.CloseWindow("Agent");
		}
	}
}
