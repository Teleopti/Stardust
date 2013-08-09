using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class UserLogonDetailsStepDefinitions
	{
		[Given(@"I have user logon details with")]
		public void GivenIHaveUserLogonDetailsWith(Table table)
		{
			var userLogonDetai = table.CreateInstance<UserLogonDetailConfigurable>();
			UserFactory.User().Setup(userLogonDetai);
		}

	}
}