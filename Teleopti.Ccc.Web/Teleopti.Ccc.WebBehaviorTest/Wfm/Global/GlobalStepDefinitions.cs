using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm
{
	[Binding]
	public class GlobalStepDefinitions
	{
		[Then(@"I should have available business units with")]
		public void ThenIShouldHaveAvailableBusinessUnitsWith(Table table)
		{
			var buNames = table.CreateSet<BusinessUnitData>();
			Browser.Interactions.Click("[data-test-bu-select]");
			buNames.ForEach(bu => Browser.Interactions.AssertExistsUsingJQuery($"[data-test-bu-list] > li:contains({bu.Name})"));
		}


		[When(@"I pick business unit '(.*)'")]
		public void WhenIPickBusinessUnit(string businessUnit)
		{
			Browser.Interactions.Click("[data-test-bu-select]");
			Browser.Interactions.ClickUsingJQuery($"[data-test-bu-list] > li:contains({businessUnit})");
		}

		[When(@"I open group pages picker")]
		public void WhenIOpenGroupPagesPicker()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true, () =>
			{
				Browser.Interactions.Click("group-page-picker");
			});
		}

		[Then(@"I close group pages picker")]
		public void ThenICloseGroupPagesPicker()
		{
			Browser.Interactions.ClickVisibleOnly(".group-page-picker-menu .footer .selection-done");
		}

		[Then(@"I click clear button in group pages picker")]
		public void WhenIClearSelectionInTheHierarchy_Picker()
		{
			Browser.Interactions.Click(".group-page-picker-menu .footer .selection-clear");
		}

		[Then(@"I should see group pages picker tab")]
		public void ThenIShouldSeeGroupPagesPickerTab()
		{
			Browser.Interactions.AssertExistsUsingJQuery(
				".group-page-picker-menu md-pagination-wrapper .md-tab:nth-child(2)");
		}

		[Then(@"I click on group page picker icon")]
		public void ThenIClickOnGroupPagePickerIcon()
		{
			Browser.Interactions.WaitScopeCondition(".group-page-picker-menu md-tabs-content-wrapper",
				"$parent.$ctrl.isDataAvailable", true,
				() =>
				{
					Browser.Interactions.ClickUsingJQuery(
						".group-page-picker-menu md-pagination-wrapper .md-tab:nth-child(2)");
				});
		}

		[Then(@"I select all skills on group page picker")]
		public void ThenISelectAllSkillsOnGroupPagePicker()
		{
			Browser.Interactions.WaitScopeCondition(".group-page-picker-menu md-tabs-content-wrapper", "$parent.$ctrl.isDataAvailable", true,
				() =>
				{
					Browser.Interactions.ClickUsingJQuery(
						".group-page-picker-menu md-pagination-wrapper .md-tab:nth-child(2)");

					Browser.Interactions.ClickContaining(".group-page-picker-menu .virtual-repeat .repeated-item .md-button",
						Resources.Skill);
				}
			);
		}

		[Then(@"I select group '(.*)' on group page picker")]
		public void ThenISelectSite(string group)
		{
			Browser.Interactions.WaitScopeCondition(".group-page-picker-menu md-tabs-content-wrapper", "$parent.$ctrl.isDataAvailable", true, () =>
				{
					Browser.Interactions.ClickContaining(".group-page-picker-menu .virtual-repeat .repeated-item .md-button", group);
				});
		}

		public class BusinessUnitData
		{
			public string Name { get; set; }
		}
	}
}