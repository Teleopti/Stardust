using System.Collections.Generic;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.ResourcePlanner
{
	[Binding]
	public class AgentGroupStepDefinition
	{
		[When(@"I input agent group name '(.*)'")]
		public void WhenIInputAgentGroupName(string agentGroupName)
		{
			Browser.Interactions.FillWith("input#agentGroupName", agentGroupName);
		}

		[When(@"I select the team")]
		public void WhenISelectTheTeam(Table table)
		{
			foreach (var teamFilter in table.CreateSet<TeamFilter>())
			{
				searchAndSelect(teamFilter.Team, $"{teamFilter.Site}/{teamFilter.Team}");
			}
		}

		[When(@"I select the skill")]
		public void WhenISelectTheSkill(Table table)
		{
			foreach (var skillFilter in table.CreateSet<SkillFilter>())
			{
				searchAndSelect(skillFilter.Skill, skillFilter.Skill);
			}
		}

		[When(@"I save agent group")]
		public void WhenISaveAgentGroup()
		{
			Browser.Interactions.Click(".save-agent-group");
		}

		[Then(@"I should see '(.*)' in the agent group list")]
		public void ThenIShouldSeeInTheAgentGroupList(string agentGroupName)
		{
			Browser.Interactions.AssertAnyContains(".agent-group-name", agentGroupName);
		}

		[Given(@"there is an agent group with")]
		public void GivenThereIsAnAgentGroupNamedWith(Table table)
		{
			var agentGroup = table.CreateInstance<AgentGroupConfigurable>();
			DataMaker.Data().Apply(agentGroup);
		}

		[When(@"I click more actions for agent group '(.*)'")]
		public void WhenIClickMoreActionsForAgentGroup(string agentGroupName)
		{
			Browser.Interactions.AssertAnyContains(".agent-group-name", agentGroupName);
			// TODO: We should find the actual agent group, this only works with one agent group also this is HORRIBLE
			Browser.Interactions.Javascript(@"
				var el = document.querySelector('.ag-menu > div'); 
				var classes = el.classList;
				classes.remove('context-menu');
				classes.add('context-menu:focus');
				el.classList = classes;
			");
		}

		[When(@"I click edit agent group")]
		public void WhenIClickEditAgentGroup()
		{
			Browser.Interactions.Click(".edit-agent-group");
		}

		[When(@"I click delete agent group")]
		public void WhenIClickDeleteAgentGroup()
		{
			Browser.Interactions.Click(".delete-agent-group");
		}

		[When(@"I confirm deletion")]
		public void WhenIConfirmDeletion()
		{
			Browser.Interactions.Click(".confirm-delete-agent-group");
		}

		[Then(@"I should not see '(.*)' in the agent group list")]
		public void ThenIShouldNotSeeInTheAgentGroupList(string agentGroupName)
		{
			Browser.Interactions.AssertNoContains(".wfm-grid", ".agent-group-name", agentGroupName);
		}

		private void searchAndSelect(string searchText, string selectItemText)
		{
			Browser.Interactions.FillWith("md-autocomplete md-autocomplete-wrap input", searchText);
			Browser.Interactions.SetScopeValues("md-autocomplete md-autocomplete-wrap", new Dictionary<string, string> // Hack to show dropdown
			{
				{"$mdAutocompleteCtrl.hidden",  "false"}
			});
			Browser.Interactions.ClickContaining(".wfm-chip span.chip-text", selectItemText);
		}
	}

	public class TeamFilter
	{
		public string Team { get; set; }
		public string Site { get; set; }
	}

	public class SkillFilter
	{
		public string Skill { get; set; }
	}
}