using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class ConfigurableUserStepDefinitions
	{
		[Given(@"I am a user")]
		public void GivenIAmAUser()
		{
		}

		[Given(@"I have the role '(.*)'")]
		public void GivenIHaveTheRoleAccessToMytime(string name)
		{
			var userRole = new RoleForUser { Name = name };
			UserFactory.User().Setup(userRole);
		}

		[Given(@"I have the workflow control set '(.*)'")]
		public void GivenIHaveTheWorkflowControlSetPublishedSchedule(string name)
		{
			var userWorkflowControlSet = new WorkflowControlSetForUser { Name = name };
			UserFactory.User().Setup(userWorkflowControlSet);
		}

		[Given(@"I have a schedule period with")]
		public void GivenIHaveASchedulePeriodWith(Table table)
		{
			var schedulePeriod = table.CreateInstance<SchedulePeriodConfigurable>();
			UserFactory.User().Setup(schedulePeriod);
		}

		[Given(@"I have a person period with")]
		public void GivenIHaveAPersonPeriodWith(Table table)
		{
			var personPeriod = table.CreateInstance<PersonPeriodConfigurable>();
			UserFactory.User().Setup(personPeriod);
		}


	}
}