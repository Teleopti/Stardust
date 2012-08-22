using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class RoleStepDefinitions
	{
		[Given(@"there is a role with")]
		public void GivenThereIsARoleWith(Table table)
		{
			var role = table.CreateInstance<RoleConfigurable>();
			UserFactory.User().Setup(role);
		}

		[Given(@"I have the role '(.*)'")]
		public void GivenIHaveTheRoleAccessToMytime(string name)
		{
			var userRole = new RoleForUser { Name = name };
			UserFactory.User().Setup(userRole);
		}

	}
}