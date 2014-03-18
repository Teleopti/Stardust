using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
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
			Browser.Interactions.AssertJavascriptResultContains(string.Format("return $('#report-list li').length > 0;"), "True");
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
		}


	}
}
