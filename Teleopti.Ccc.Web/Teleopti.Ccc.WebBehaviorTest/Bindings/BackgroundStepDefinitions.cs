using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Team = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific.Team;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class BackgroundStepDefinitions
	{
		[Given(@"I am a user")]
		public void GivenIAmAUser()
		{
		}

		[Given(@"I have the role '(.*)'")]
		public void GivenIHaveTheRoleAccessToMytime(string name)
		{
			var userRole = new RoleForUser {Name = name};
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

		[Given(@"I have a preference with")]
		public void GivenIHaveAPreferenceWith(Table table)
		{
			var preference = table.CreateInstance<PreferenceConfigurable>();
			UserFactory.User().Setup(preference);
		}

		[Given(@"I have an extended preference on '(.*)'")]
		public void GivenIHaveAnExtendedPreferenceOn2012_06_20(DateTime date)
		{
			UserFactory.User().Setup(new PreferenceConfigurable { Date = date, IsExtended = true });
		}

		[When(@"I view preferences for date '(.*)'")]
		public void WhenIViewPreferencesForDate2012_06_20(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoPreference(date);
		}





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