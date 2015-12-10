using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
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
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", Is.EqualTo("true"));
			Browser.Interactions.AssertAnyContains(".request-agent-name .ui-grid-cell-contents", userName);
		}

		[When(@"I sort the request list by descending agent name")]
		public void WhenISortTheRequestListByDescendingAgentName()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", Is.EqualTo("true"));
			Browser.Interactions.ClickUsingJQuery(".request-agent-name-header .ui-grid-column-menu-button");
			Browser.Interactions.ClickUsingJQuery(".ui-grid-menu-items li:nth(1) button");
		}

		[Then(@"I should see the request from '(.*)' before the request from '(.*)' in the list")]
		public void ThenIShouldSeeTheRequestFromBeforeTheRequestFromInTheList(string agentName1, string agentName2)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".ui-grid-row:contains('{0}') + .ui-grid-row:contains('{1}')", agentName1, agentName2);
		}


	}
}
