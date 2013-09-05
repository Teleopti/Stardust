using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	[Binding]
	public class SharedSignInStepDefinitions
	{
		[Given(@"I am a user with multiple business units")]
		public void GivenIAmAUserWithMultipleBusinessUnits()
		{
			UserFactory.User().Setup(new Agent());
			UserFactory.User().Setup(new AgentSecondBusinessUnit());
		}


		[Then(@"I should see an error message ""(.*)""")]
		public void ThenIShouldSeeAnErrorMessage(string msg)
		{
		}

		[Then(@"I should see the sign in page")]
		[Then(@"I should not be signed in")]
		[Then(@"I should be signed out")]
		[Then(@"I should be signed out from MobileReports")]
		public void ThenIAmNotSignedIn()
		{
			EventualAssert.That(() => Pages.Pages.CurrentSignInPage.UserNameTextField.Exists, Is.True);
		}
	}
}