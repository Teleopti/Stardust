using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class RoleStepDefinitions
	{
		[Given(@"there is a role with")]
		public void GivenThereIsARoleWith(Table table)
		{
			var role = table.CreateInstance<RoleConfigurable>();
			DataMaker.Data().Apply(role);
		}

		[Given(@"I have the role '(.*)'")]
		public void GivenIHaveTheRoleAccessToMytime(string name)
		{
			var userRole = new RoleForUser { Name = name };
			DataMaker.Data().Apply(userRole); // creates and persists role 
		}

		[Given(@"I have a role with")]
		public void GivenIHaveARoleWith(Table table)
		{
			var role = table.CreateInstance<RoleConfigurable>();
			DataMaker.Data().Apply(role); // creates and persists role 
			var userRole = new RoleForUser { Name = role.Name }; // loads the role again
			DataMaker.Data().Apply(userRole); // adds the role to the user and persist
		}

		[Given(@"I have a role with full access")]
		public void GivenIHaveARoleWithFullAccess()
		{
			var role = new RoleConfigurable
			{
				AccessToEveryone = true,
				AccessToAnywhere = true,
				// put any that defaults to false to true here as needed
			};
			DataMaker.Data().Apply(role);
			DataMaker.Data().Apply(new RoleForUser {Name = role.Name});
		}
		
		[Given(@"I have a role named '(.*)'")]
		public void GivenIHaveARoleNamed(string name)
		{
			DataMaker.Data().Apply(new RoleConfigurable {Name = name});
			DataMaker.Data().Apply(new RoleForUser {Name = name});
		}
	}
}