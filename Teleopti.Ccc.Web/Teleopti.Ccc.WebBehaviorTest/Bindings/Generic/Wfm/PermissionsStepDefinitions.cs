using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class PermissionsStepDefinitions
	{
		[When(@"I create a role '(.*)'")]
		public void WhenICreateARole(string p0)
		{
			Browser.Interactions.FillWith(".role-input",p0);
			Browser.Interactions.PressEnter(".role-input");
		}

		[Then(@"I should see a role '(.*)' in the list")]
		public void ThenIShouldSeeARoleInTheList(string p0)
		{
			Browser.Interactions.AssertAnyContains(".wfm-list li", p0);
		}

		[Then(@"I should not see a role '(.*)' in the list")]
		public void ThenIShouldNotSeeARoleInTheList(string p0)
		{
			Browser.Interactions.AssertNoContains(".wfm-list", ".wfm-list li", p0);
		}

		[When(@"I select a role '(.*)'")]
		public void WhenISelectARole(string p0)
		{
			Browser.Interactions.ClickContaining(".wfm-list li", p0);
		}

		[When(@"I select the first permission")]
		public void WhenISelectTheFirstPermission()
		{
			Browser.Interactions.Click(".functions-container .angular-ui-tree-handle");
		}

		[Then(@"I should see the first permission selected in the list")]
		public void ThenIShouldSeeTheFirstPermissionSelectedInTheList()
		{
			Browser.Interactions.AssertExists(".functions-container .angular-ui-tree-handle.selected");
		}


	}
}