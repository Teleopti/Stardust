using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

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

		[When(@"I select a permission called '(.*)'")]
		public void WhenISelectAPermissionCalled(string p0)
		{
			Browser.Interactions.ClickContaining(".ui-tree-handle", p0);
		}

		[Then(@"I should see '(.*)' selected in the list")]
		public void ThenIShouldSeeSelectedInTheList(string p0)
		{
			Browser.Interactions.AssertAnyContains(".ui-tree-handle.selected", p0);
		}


	}
}