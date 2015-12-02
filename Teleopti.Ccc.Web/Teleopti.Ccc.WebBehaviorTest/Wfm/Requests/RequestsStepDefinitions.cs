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

		[When(@"I select to loading requests from '(.*)' to '(.*)'")]
		public void WhenISelectToLoadingRequestsFromTo(string from, string to)
		{
			Browser.Interactions.SetScopeValues("date-range-picker", new Dictionary<string, string>
			{
				{ "requestsOverview.requestsFilter.period.startDate", string.Format("new Date('{0}')", from)},
				{ "requestsOverview.requestsFilter.period.endDate", string.Format("new Date('{0}')", to) },
			});
		}

		[Then(@"I should see a request from '(.*)' in the list")]
		public void ThenIShouldSeeARequestFromInTheList(string userName)
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", Is.EqualTo("true"));
			Browser.Interactions.AssertAnyContains(".request-name .ui-grid-cell-contents", userName);
		}

	}
}
