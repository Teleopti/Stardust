using System.Collections.Generic;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Requests
{
	[Binding]
	public class RequestsStepDefinitions
	{
		[Given(@"'(.*)' has an existing text request with")]
		public void GivenHasAnExistingTextRequestWith(string userName, Table table)
		{
			var textRequest = table.CreateInstance<TextRequestConfigurable>();
			DataMaker.Person(userName).Apply(textRequest);
		}
		
		[Given(@"'(.*)' has an existing absence request with")]
		public void GivenHasAnExistingAbsenceRequestWith(string userName, Table table)
		{
			var absenceRequest = table.CreateInstance<AbsenceRequestConfigurable>();
			DataMaker.Person(userName).Apply(absenceRequest);
		}
		

		[When(@"I select to load requests from '(.*)' to '(.*)'")]
		public void WhenISelectToLoadRequestsFromTo(string from, string to)
		{
			Browser.Interactions.SetScopeValues("date-range-picker", new Dictionary<string, string>
			{
				{ "requests.period.startDate", string.Format("new Date('{0}')", from)},
				{ "requests.period.endDate", string.Format("new Date('{0}')", to) },
			});
		}

		[Then(@"I should see a request from '(.*)' in the list")]
		public void ThenIShouldSeeARequestFromInTheList(string userName)
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.AssertAnyContains(".request-agent-name .ui-grid-cell-contents", userName);
		}

		[When(@"I sort the request list by descending agent name")]
		public void WhenISortTheRequestListByDescendingAgentName()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.ClickUsingJQuery(".request-agent-name-header .ui-grid-column-menu-button");
			Browser.Interactions.ClickUsingJQuery(".ui-grid-menu-items li:nth(1) button");
		}

		[Then(@"I should see the request from '(.*)' before the request from '(.*)' in the list")]
		public void ThenIShouldSeeTheRequestFromBeforeTheRequestFromInTheList(string agentName1, string agentName2)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".ui-grid-row:contains('{0}') + .ui-grid-row:contains('{1}')", agentName1, agentName2);
		}

		[When(@"I approve all requests that I see")]
		public void WhenIApproveAllRequestsThatISee()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.ClickUsingJQuery(".ui-grid-row .ui-grid-selection-row-header-buttons");
			Browser.Interactions.ClickUsingJQuery("requests-commands-pane .approve-requests");
		}

		[When(@"I deny all requests that I see")]
		public void WhenIDenyAllRequestsThatISee()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.ClickUsingJQuery(".ui-grid-row .ui-grid-selection-row-header-buttons");
			Browser.Interactions.ClickUsingJQuery("requests-commands-pane .deny-requests");
		}


		[Then(@"I should see request for '(.*)' approved")]
		public void ThenIShouldSeeRequestForApproved(string agentName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".ui-grid-row:contains('{0}') .request-status:contains('{1}')", agentName, "Approved"));
		}

		[Then(@"I should see request for '(.*)' denied")]
		public void ThenIShouldSeeRequestForDenied(string agentName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".ui-grid-row:contains('{0}') .request-status:contains('{1}')", agentName, "Denied"));
		}


		[Then(@"I should see all requests approved")]
		public void ThenIShouldSeeAllRequestsApproved()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".request-status:contains('{0}')", "Approved"));
		}

		[Then(@"I should see all requests denied")]
		public void ThenIShouldSeeAllRequestsDenied()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".request-status:contains('{0}')", "Denied"));
		}

	}
}
