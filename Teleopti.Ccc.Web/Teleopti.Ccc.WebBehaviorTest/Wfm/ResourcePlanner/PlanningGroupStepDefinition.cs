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
	public class PlanningGroupStepDefinition
	{
		[When(@"I input planning group name '(.*)'")]
		public void WhenIInputPlanningGroupName(string planningGroupName)
		{
			Browser.Interactions.FillWith("input#agentGroupName", planningGroupName);
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

		[When(@"I save planning group")]
		public void WhenISavePlanningGroup()
		{
			Browser.Interactions.Click(".save-agent-group");
		}

		[Then(@"I should see '(.*)' in the planning group list")]
		public void ThenIShouldSeeInThePlanningGroupList(string planningGroupName)
		{
			Browser.Interactions.AssertAnyContains(".agent-group-name", planningGroupName);
		}

		[Given(@"there is an planning group with")]
		public void GivenThereIsAnPlanningGroupNamedWith(Table table)
		{
			var planningGroupConfigurable = table.CreateInstance<PlanningGroupConfigurable>();
			DataMaker.Data().Apply(planningGroupConfigurable);
		}

		[When(@"I click more actions for planning group '(.*)'")]
		public void WhenIClickMoreActionsForPlanningGroup(string planningGroupName)
		{
			Browser.Interactions.AssertAnyContains(".agent-group-name", planningGroupName);
			// TODO: We should find the actual agent group, this only works with one agent group also this is HORRIBLE
			Browser.Interactions.Javascript(@"
				var el = document.querySelector('.ag-menu > div'); 
				var classes = el.classList;
				classes.remove('context-menu');
				classes.add('context-menu:focus');
				el.classList = classes;
			");
		}

		[When(@"I click edit planning group")]
		public void WhenIClickEditPlanningGroup()
		{
			Browser.Interactions.Click(".edit-agent-group");
		}

		[When(@"I click delete planning group")]
		public void WhenIClickDeletePlanningGroup()
		{
			Browser.Interactions.Click(".delete-agent-group");
		}

		[When(@"I confirm deletion")]
		public void WhenIConfirmDeletion()
		{
			Browser.Interactions.Click(".confirm-delete-agent-group");
		}

		[Then(@"I should not see '(.*)' in the planning group list")]
		public void ThenIShouldNotSeeInThePlanningGroupList(string planningGroupName)
		{
			Browser.Interactions.AssertNoContains(".wfm-grid", ".agent-group-name", planningGroupName);
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