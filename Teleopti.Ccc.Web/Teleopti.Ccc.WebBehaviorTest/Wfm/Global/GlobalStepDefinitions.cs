using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
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
			Browser.Interactions.Click("#business-unit-select .pointer");
			buNames.ForEach(bu => Browser.Interactions.AssertExistsUsingJQuery($"#business-unit-select li:contains({bu.Name})"));
		}


		[When(@"I pick business unit '(.*)'")]
		public void WhenIPickBusinessUnit(string businessUnit)
		{
			Browser.Interactions.Click("#business-unit-select .pointer");
			Browser.Interactions.ClickUsingJQuery($"#business-unit-select li:contains({businessUnit})");
		}

		[When(@"I open group pages picker")]
		public void WhenIOpenGroupPagesPicker()
		{
			Browser.Interactions.ClickVisibleOnly(".group-picker-container");
		}

		[Then(@"I close group pages picker")]
		public void ThenICloseGroupPagesPicker()
		{
			Browser.Interactions.ClickVisibleOnly(".group-page-picker-menu .footer .selection-done");
		}

		[Then(@"I should see group pages picker tab")]
		public void ThenIShouldSeeGroupPagesPickerTab()
		{
			Browser.Interactions.AssertExists(".group-page-picker-menu i[title='GroupPages']");
		}

		[Then(@"I click on group page picker icon")]
		public void ThenIClickOnGroupPagePickerIcon()
		{
			Browser.Interactions.ClickVisibleOnly(".group-page-picker-menu i[title='GroupPages']");
		}

		[Then(@"I select all skills on group page picker")]
		public void ThenISelectAllSkillsOnGroupPagePicker()
		{
			Browser.Interactions.ClickContaining(".group-page-picker-menu .virtual-repeat .repeated-item .md-button", "Skill");
		}

		public class BusinessUnitData
		{
			public string Name { get; set; }
		}
	}
}