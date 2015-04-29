using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	class PeopleListStepDefinitions
	{
		[Then(@"I should see '(.*)' in people list")]
		public void ThenIShouldSeeInPeopleList(string name)
		{
			Browser.Interactions.AssertAnyContains(".people-list .person", name);
		}

		[Then(@"I should not see '(.*)' in people list")]
		public void ThenIShouldNotSeeInPeopleList(string name)
		{
			const string existSelector = ".people-list";
			var notExistSelector = string.Format(".people-list .person .first-name:contains({0})", name);
			Browser.Interactions.AssertNotExists(existSelector,notExistSelector);
		}

		[When(@"I search people with keyword '(.*)'")]
		public void WhenISearchPeopleWithKeyword(string value)
		{
			var selector = ".people-search input";
			Browser.Interactions.Clear(selector);
			Browser.Interactions.FillWith(selector, value);
			Browser.Interactions.PressEnter(selector);
		}
	}
}
