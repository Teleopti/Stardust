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
			UserFactory.User().Setup(userRole); // creates and persists role 
		}

		[Given(@"I have a role with")]
		public void GivenIHaveARoleWith(Table table)
		{
			var role = table.CreateInstance<RoleConfigurable>();
			UserFactory.User().Setup(role); // creates and persists role 
			var userRole = new RoleForUser { Name = role.Name }; // loads the role again
			UserFactory.User().Setup(userRole); // adds the role to the user and persist
		}

		[Given(@"I have a role named '(.*)'")]
		public void GivenIHaveARoleNamedWith(string name)
		{
			UserFactory.User().Setup(new RoleConfigurable {Name = name});
			UserFactory.User().Setup(new RoleForUser {Name = name});
		}
	}
}