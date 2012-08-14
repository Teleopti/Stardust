using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class ConfigurableDataStepDefinitions
	{
		[Given(@"there is a workflow control set with")]
		public void GivenThereIsAWorkflowControlSetWith(Table table)
		{
			var workflowControlSet = table.CreateInstance<WorkflowControlSetConfigurable>();
			UserFactory.User().Setup(workflowControlSet);
		}

		[Given(@"there is a role with")]
		public void GivenThereIsARoleWith(Table table)
		{
			var role = table.CreateInstance<RoleConfigurable>();
			UserFactory.User().Setup(role);
		}

		[Given(@"there is a business unit with")]
		public void GivenThereIsABusinessUnitWith(Table table)
		{
			var businessUnit = table.CreateInstance<BusinessUnitConfigurable>();
			UserFactory.User().Setup(businessUnit);
		}

	}
}