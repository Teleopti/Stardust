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
			Browser.Interactions.FillWith("input#planningGroupName", planningGroupName);
		}

		[When(@"I input scheduling setting name '(.*)'")]
		public void WhenIInputschedulingSettingName(string planningGroupName)
		{
			Browser.Interactions.FillWith("input#schedulingSettingName", planningGroupName);
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

		[When(@"I open block scheduling panel")]
		public void WhenIOpenTheBlockSchedulingPanel()
		{
			Browser.Interactions.Click(".block-scheduling-panel");
		}

		[When(@"I open panel for scheduling setting '(.*)'")]
		public void WhenIOpenPanelForSchedulingSetting(string planningGroupSettingName)
		{
			Browser.Interactions.ClickContaining(".scheduling-setting-name", planningGroupSettingName);
		}

		[When(@"I click delete scheduling setting '(.*)'")]
		public void WhenIClickDeleteSchedulingSetting(string planningGroupSettingName)
		{
			Browser.Interactions.ClickUsingJQuery("card-panel-list card-panel:has(b:contains('" + planningGroupSettingName + "')) div.card-context:has(i.mdi-delete)");
		}

		[When(@"I turn on block scheduling setting")]
		public void WhenITurnOnTheBlockScheduling()
		{
			Browser.Interactions.Click("md-switch");
		}

		[When(@"I turn off block scheduling setting")]
		public void WhenITurnOffTheBlockScheduling()
		{
			Browser.Interactions.Click("md-switch");
		}

		[When(@"I save planning group")]
		public void WhenISavePlanningGroup()
		{
			Browser.Interactions.Click(".save-plan-group");
		}

		[When(@"I save scheduling setting")]
		public void WhenISaveSchedulingSetting()
		{
			Browser.Interactions.Click(".save-scheduling-setting");
		}

		[Then(@"I should see '(.*)' in the planning group list")]
		public void ThenIShouldSeeInThePlanningGroupList(string planningGroupName)
		{
			Browser.Interactions.AssertAnyContains(".plan-group > div.list-header h2", planningGroupName);
		}

		[Then(@"I should see '(.*)' in the scheduling setting list")]
		public void ThenIShouldSeeInTheSchedulingSettingList(string planningGroupName)
		{
			Browser.Interactions.AssertAnyContains(".scheduling-setting-name", planningGroupName);
		}

		[Given(@"there is an planning group with")]
		public void GivenThereIsAnPlanningGroupNamedWith(Table table)
		{
			var planningGroupConfigurable = table.CreateInstance<PlanningGroupConfigurable>();
			DataMaker.Data().Apply(planningGroupConfigurable);
		}

		[Given(@"there is an planning group and one custom scheduling setting with")]
		public void GivenThereIsAnPlanningGroupSchedulingSettingNamedWith(Table table)
		{
			var planningGroupSchedulingSettingConfigurable = table.CreateInstance<PlanningGroupSchedulingSettingConfigurable>();
			DataMaker.Data().Apply(planningGroupSchedulingSettingConfigurable);
		}

		[When(@"I click edit planning group '(.*)'")]
		public void WhenIClickEditPlanningGroup(string planningGroupName)
		{
			Browser.Interactions.AssertAnyContains(".plan-group > div.list-header h2", planningGroupName);
			Browser.Interactions.Click(".edit-plan-group");
		}

		[When(@"I click delete planning group")]
		public void WhenIClickDeletePlanningGroup()
		{
			Browser.Interactions.Click(".delete-plan-group");
		}

		[When(@"I confirm deletion")]
		public void WhenIConfirmDeletion()
		{
			Browser.Interactions.Click(".confirm-delete-plan-group");
		}

		[When(@"I confirm delete scheduling setting")]
		public void WhenIConfirmSchedulingSettingDeletion()
		{
			Browser.Interactions.Click(".confirm-delete-scheduling-setting");
		}

		[Then(@"I should not see '(.*)' in the planning group list")]
		public void ThenIShouldNotSeeInThePlanningGroupList(string planningGroupName)
		{
			Browser.Interactions.AssertNoContains(".con-fluid-row", ".plan-group > div.list-header h1", planningGroupName);
		}

		[Then(@"I should not see '(.*)' in the scheduling setting list")]
		public void ThenIShouldNotSeeInTheSchedulingSettingList(string planningGroupSettingName)
		{
			Browser.Interactions.AssertNoContains("card-panel-list", ".scheduling-setting-name", planningGroupSettingName);
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