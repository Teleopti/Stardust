using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	class PeopleListStepDefinitions
	{
		[Then(@"I should see '(.*)' in people list")]
		public void ThenIShouldSeeInPeopleList(string name)
		{
			var selector = string.Format(".people-list .person .first-name:contains({0})", name);
			Browser.Interactions.AssertExists(selector);
		}

		[Then(@"I should not see '(.*)' in people list")]
		public void ThenIShouldNotSeeInPeopleList(string name)
		{
			const string existSelector = ".people-list";
			var notExistSelector = string.Format(".people-list .person .first-name:contains({0})", name);
			Browser.Interactions.AssertNotExists(existSelector,notExistSelector);
		}

	}
}
