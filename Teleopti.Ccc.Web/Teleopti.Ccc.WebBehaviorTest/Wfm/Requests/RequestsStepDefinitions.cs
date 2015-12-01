using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
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

		[Then(@"I should see a request from '(.*)' in the list")]
		public void ThenIShouldSeeARequestFromInTheList(string userName)
		{
			Browser.Interactions.AssertAnyContains(".request-name .ui-grid-cell-contents", userName);
		}

	}
}
