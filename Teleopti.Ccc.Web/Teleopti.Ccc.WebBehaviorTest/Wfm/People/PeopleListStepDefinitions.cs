using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	class PeopleListStepDefinitions
	{
		[Then(@"I should see '(.*)' in people list")]
		public void ThenIShouldSeeInPeopleList(string name)
		{
			Browser.Interactions.AssertAnyContains(".people-list .ui-grid-row", name);
		}

		[Then(@"I should not see '(.*)' in people list")]
		public void ThenIShouldNotSeeInPeopleList(string name)
		{
			const string existSelector = ".people-list";
			const string notExistSelector = ".people-list .ui-grid-row";
			Browser.Interactions.AssertNoContains(existSelector,notExistSelector, name);
		}

		[When(@"I search people with keyword '(.*)'")]
		public void WhenISearchPeopleWithKeyword(string value)
		{
			var selector = ".people-search #simple-people-search";
			Browser.Interactions.Clear(selector);
			Browser.Interactions.FillWith(selector, value);
			Browser.Interactions.PressEnter(selector);
		}

		[When(@"I search with")]
		public void WhenISearchWith(Table table)
		{
			Browser.Interactions.Click("#simple-people-search");

			var criterias = table.CreateSet<SearchCriteria>();

			foreach (var criteria in criterias)
			{
				var selector = string.Format("#criteria-{0}", criteria.Field.Replace(' ', '-').ToLower());
				Browser.Interactions.FillWith(selector, criteria.Value);
			}

			
			Browser.Interactions.AssertVisibleUsingJQuery("#go-advanced-search");
			Browser.Interactions.Click("#go-advanced-search");
		}

		[When(@"I open the action panel")]
		public void WhenIOpenTheActionPanel()
		{
			Browser.Interactions.Click(".action-panel");
		}

		[Then(@"I should see import user command")]
		public void ThenIShouldSeeImportUserCommand()
		{
			Browser.Interactions.AssertExists(".action-panel .mdi-file");
		}

		[When(@"I open the import user command")]
		public void WhenIOpenTheImportUserCommand()
		{
			Browser.Interactions.Click(".action-panel .mdi-file");
		}

		[Then(@"I should see import panel")]
		public void ThenIShouldSeeImportPanel()
		{
			Browser.Interactions.AssertExists(".import-people");
		}

		[When(@"I select (.*) in people list")]
		public void WhenISelect(string name)
		{
			Browser.Interactions.ClickContaining(".ui-grid-row", name);
		}

		[Then(@"I should see an indicator telling me (.*) (person|people) selected")]
		public void ThenIShouldSeeAnIndicatorTellingMePersonSelected(int count, string personPeople)
		{
			Browser.Interactions.AssertAnyContains(".selection-cart-indicator", count.ToString());
		}

		[When(@"I open the command (.*)")]
		public void WhenIOpenTheCommand(string commandName)
		{
			var selector = commandName.ToLower().Replace(" ", "");
			Browser.Interactions.Click("." + selector);
		}


	}

	public class SearchCriteria
	{
		public string Field { get; set; }
		public string Value { get; set; }
	}
}
