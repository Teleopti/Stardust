using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class LogonAsAnotherUserStepDefinitions
	{
		[Then(@"I should not be able to logon as another user")]
		public void ThenIShouldNotBeAbleToLogonAsAnotherUser()
		{
			Browser.Interactions.AssertNotExists("#regional-settings", "#logon-as-another");
		}

	}
}