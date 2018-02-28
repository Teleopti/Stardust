using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public sealed class PeopleStepDefinition
	{
		private List<PersonDataFactory> persons = new List<PersonDataFactory>();
		private List<RoleForUser> roles = new List<RoleForUser>();

		[Given("Role '(.*)' exists")]
		public void RoleXExists(string roleName)
		{
			// Create role X
			var roleConfig = new RoleConfigurable
			{
				AccessToPeople = true, // Should we do it like this?
				Name = roleName
			};

			DataMaker.Data().Apply(roleConfig);
			var role = new RoleForUser
			{
				Name = roleConfig.Name
			};

			roles.Add(role);
			DataMaker.Data().Apply(role);
		}

		[Given("Person '(.*)' exists")]
		public void PersonXExists(string name)
		{
			var person = DataMaker.Person(name); // Does this work?
			persons.Add(person);
		}

		[Given("All of them has role '(.*)'")]
		public void AllPeopleHasRoleX(string roleName)
		{
			var role = roles.Find(r => r.Name == roleName);
			foreach (PersonDataFactory person in persons)
			{
				person.Apply(role);
			}
		}

		[Given("Person '(.*)' is selected")]
		public void PersonXIsSelected(string name)
		{
			
		}

		[Then("I should see '(.*)' in the workspace")]
		public void IShouldSeeXInTheWorkspace(string name)
		{ }
	}
}