using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Permissions
{
	[Binding]
	public class PermissionsStepDefinitionsNew
	{

		[Given(@"there is a function named '(.*)'")]
		public void GivenThereIsAFunctionNamed(string name)
		{
			var function = new ApplicationFunctionConfigurable
			{
				LocalizedFunctionDescription = name,
				Name = name
			};
			DataMaker.Data().Apply(function);
		}

		[When(@"I create a new role '(.*)'")]
		public void WhenICreateANewRole(string name)
		{
			Browser.Interactions.Click(".wfm-fab");
			Browser.Interactions.FillWith("i .ng-pristine", name);
			Browser.Interactions.Click(".wfm-btn");
		}

		[When(@"I delete a role '(.*)'")]
		public void WhenIDeleteARole(string name)
		{
			Browser.Interactions.AssertAnyContains(".wfm-list li", name);
			Browser.Interactions.ClickContaining(".wfm-list li", name);
			Browser.Interactions.Click(".wfm-leave-behind span .mdi-delete");
			Browser.Interactions.Click("#delete-role");
		}

		[When(@"I edit the name of role '(.*)' and write '(.*)'")]
		public void WhenIEditTheNameOfRoleAndWrite(string oldName, string newName)
		{
			Browser.Interactions.AssertAnyContains(".wfm-list li", oldName);
			Browser.Interactions.ClickContaining(".wfm-list li", oldName);
			Browser.Interactions.Click("#edit-role-trigger");
			Browser.Interactions.IsVisible("#edit-role-input");
			Browser.Interactions.FillWith("#edit-role-input", newName);
			Browser.Interactions.Click("#edit-role-confirm");
		}

		[When(@"I copy role '(.*)'")]
		public void WhenICopyRole(string name)
		{	
			Browser.Interactions.AssertAnyContains(".wfm-list li", name);
			Browser.Interactions.ClickContaining(".wfm-list li", name);
			Browser.Interactions.Click(".wfm-leave-behind span .mdi-content-copy");
		}

		[When(@"I select role '(.*)'")]
		public void WhenISelectRole(string name)
		{
			Browser.Interactions.ClickContaining(".wfm-list li", name);
		}

		[When(@"I select function '(.*)'")]
		public void WhenISelectFunction(string name)
		{
			Browser.Interactions.ClickContaining("div.tree-toggle-group > div.tree-handle-wrapper > div", name);
		}

		[When(@"I select organization selection '(.*)'")]
		public void WhenISelectOrganizationSelection(string name)
		{
			Browser.Interactions.Click("md-tab-item:nth-child(2)");
			Browser.Interactions.ClickContaining(".wfm-list li", name);
		}

		[Then(@"I should see a role '(.*)'")]
		public void ThenIShouldSeeARole(string name)
		{
			Browser.Interactions.AssertAnyContains(".wfm-list li", name);
		}
		
		[Then(@"I should not see role '(.*)'")]
		public void ThenIShouldNotSeeRole(string name)
		{
			Browser.Interactions.AssertNoContains(".wfm-list", ".wfm-list li", name);
		}

		[Then(@"I should see the function selected in the list '(.*)'")]
		public void ThenIShouldSeeTheFunctionSelectedInTheList(string name)
		{
			Browser.Interactions.AssertAnyContains(".angular-ui-tree .angular-ui-tree-handle.selected", name);
		}

		[Then(@"I should not see '(.*)' selected")]
		public void ThenIShouldNotSeeSelected(string name)
		{
			Browser.Interactions.AssertAnyContains(".angular-ui-tree .angular-ui-tree-handle", name);
		}

		[Then(@"I should see organization '(.*)' selected")]
		public void ThenIShouldSeeOrganizationSelected(string name)
		{
			Browser.Interactions.AssertAnyContains("#team-list-label", name);
			Browser.Interactions.AssertExists("#team-selection-indicator");
		}

		[Then(@"I should not see organization '(.*)' selected")]
		public void ThenIShouldNotSeeOrganizationSelected(string name)
		{	
			Browser.Interactions.AssertNotExists("#team-selection-indicator",".test-class");
		}
	}
}