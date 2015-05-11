using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class LogonAsAnotherUserStepDefinitions
	{
		[When(@"I choose to sign out")]
		public void WhenIChooseToSignOut()
		{
			Browser.Interactions.Click(".user-name-link");

			const string script = @" $('#signout')[0].click(); return 1;";
			Browser.Interactions.AssertJavascriptResultContains(script, "1");
		
		}

		[When(@"I choose teleopti identity provider")]
		public void WhenIChooseTeleoptiIdentityProvider()
		{
			Browser.Interactions.Click(".teleopti");
		}
	}
}