using System.Threading;
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
			Browser.Interactions.AssertNotExists("#regional-settings", "#signin-as-another");
		}

		[When(@"I choose to sign out")]
		public void WhenIChooseToSignOut()
		{
			Browser.Interactions.Click(".user-name-link");
			Browser.Interactions.Click("#signout");
		}

		[When(@"I choose teleopti identity provider")]
		public void WhenIChooseTeleoptiIdentityProvider()
		{
			Browser.Interactions.Click(".teleopti");
		}
	}
}