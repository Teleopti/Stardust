using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;

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
			const string existSelector = ".people-list .person";
			const string notExistSelector = ".people-list .person";
			Browser.Interactions.AssertNoContains(existSelector,notExistSelector, name);
		}

		[When(@"I search people with keyword '(.*)'")]
		public void WhenISearchPeopleWithKeyword(string value)
		{
			var selector = ".people-search #simple-search";
			Browser.Interactions.Clear(selector);
			Browser.Interactions.FillWith(selector, value);
			Browser.Interactions.PressEnter(selector);
		}

		[When(@"I search with")]
		public void WhenISearchWith(Table table)
		{
			Browser.Interactions.Click("#simple-search");

			var criterias = table.CreateSet<SearchCriteria>();

			foreach (var criteria in criterias)
			{
				var selector = string.Format("#criteria-{0}", criteria.Field.Replace(' ', '-').ToLower());
				Browser.Interactions.FillWith(selector, criteria.Value);
			}

			Browser.Interactions.Click("#go-advanced-search");
		}
	}

	public class SearchCriteria
	{
		public string Field { get; set; }
		public string Value { get; set; }
	}
}
